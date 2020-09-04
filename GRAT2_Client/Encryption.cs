using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace GRAT2_Client.PInvoke
{
    class Encryption
    { 
        public static string base64_Decode(string str)
        {
            byte[] data = Convert.FromBase64String(str);//decode to base64
            string decodedString = Encoding.UTF8.GetString(data);//decode to base64
            string command = Encoding.UTF8.GetString(Convert.FromBase64String(xor(decodedString)));
            return command;
        }

        public static string xor(string results)
        {
            int len = results.Length;
            char[] plaintext = results.ToCharArray();
            char[] enc = new char[len];
            for (int i = 0; i < len; i++)
            {
                enc[i] = (char)(0x6A ^ plaintext[i]);

            }
            string r1 = new String(enc);

            return Base64Encode(r1);

        }
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        //https://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes("password");
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            passwordBytes = Encoding.UTF8.GetBytes("password");
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] decryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        // https://gist.github.com/hoiogi/89cf2e9aa99ffc3640a4
        // http://entitycrisis.blogspot.com/2011/04/encryption-between-python-and-c.html?m=1

        public static byte[] RC4Encrypt(byte[] data)
        {
            var pwd = Encoding.UTF8.GetBytes("password");
            Console.WriteLine("inside rc4 encrypt");
            Console.WriteLine("print data " + data);
            int a, i, j, k, tmp;
            int[] key, box;
            byte[] cipher;

            key = new int[256];
            box = new int[256];
            cipher = new byte[data.Length];

            for (i = 0; i < 256; i++)
            {
                key[i] = pwd[i % pwd.Length];
                box[i] = i;
            }
            for (j = i = 0; i < 256; i++)
            {
                j = (j + box[i] + key[i]) % 256;
                tmp = box[i];
                box[i] = box[j];
                box[j] = tmp;
            }
            for (a = j = i = 0; i < data.Length; i++)
            {
                a++;
                a %= 256;
                j += box[a];
                j %= 256;
                tmp = box[a];
                box[a] = box[j];
                box[j] = tmp;
                k = box[((box[a] + box[j]) % 256)];
                cipher[i] = (byte)(data[i] ^ k);
            }

            return cipher;
        }

        public static byte[] RC4Decrypt(byte[] data)
        {
            return RC4Encrypt(data);
        }

    }
}
