using System;
using System.Runtime.InteropServices;

namespace GRAT2_Client.PInvoke
{
    class flags
    {
        [Flags]

        public enum LOGON_TYPE
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK,
            LOGON32_LOGON_BATCH,
            LOGON32_LOGON_SERVICE,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT,
            LOGON32_LOGON_NEW_CREDENTIALS
        }

        public enum LOGON_PROVIDER
        {
            LOGON32_PROVIDER_DEFAULT,
            LOGON32_PROVIDER_WINNT35,
            LOGON32_PROVIDER_WINNT40,
            LOGON32_PROVIDER_WINNT50
        }

        public enum ACCESS_MASK : uint
        {
            MAXIMUM_ALLOWED = 0x02000000,

        };
        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }
        public enum ProcThreadAttribute : int
        {
            MITIGATION_POLICY = 0x20007,
            PARENT_PROCESS = 0x00020000
        }

        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }
        public enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200),
            THREAD_ALL = TERMINATE | SUSPEND_RESUME | GET_CONTEXT | SET_CONTEXT | SET_INFORMATION | QUERY_INFORMATION | SET_THREAD_TOKEN | IMPERSONATE | DIRECT_IMPERSONATION
    }

        public enum ProcessAccessRights
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        public enum MemAllocation
        {
            MEM_COMMIT = 0x00001000,
            MEM_RESERVE = 0x00002000,
            MEM_RESET = 0x00080000,
            MEM_RESET_UNDO = 0x1000000,
            SecCommit = 0x08000000
        }

        public enum MemProtect
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000,
        }

        #region Process Hollowing Structs

        public enum HANDLE_FLAGS : uint
        {
            None = 0,
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }



        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebAddress;
            public IntPtr Reserved2;
            public IntPtr Reserved3;
            public IntPtr UniquePid;
            public IntPtr MoreReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        //internal struct STARTUPINFO
        public struct STARTUPINFO
        {
            public Int32 cb;
            IntPtr lpReserved;
            IntPtr lpDesktop;
            IntPtr lpTitle;
            uint dwX;
            uint dwY;
            uint dwXSize;
            uint dwYSize;
            uint dwXCountChars;
            uint dwYCountChars;
            uint dwFillAttributes;
            public uint dwFlags;
            public ushort wShowWindow;
            ushort cbReserved;
            IntPtr lpReserved2;
            IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public uint dwOem;
            public uint dwPageSize;
            public IntPtr lpMinAppAddress;
            public IntPtr lpMaxAppAddress;
            public IntPtr dwActiveProcMask;
            public uint dwNumProcs;
            public uint dwProcType;
            public uint dwAllocGranularity;
            public ushort wProcLevel;
            public ushort wProcRevision;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LARGE_INTEGER
        {
            public uint LowPart;
            public int HighPart;
        }
        #endregion End of Process Hollowing Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        #region DNSFlags

        public struct IP4_ARRAY
        {
            /// DWORD->unsigned int
            public UInt32 AddrCount;
            /// IP4_ADDRESS[1]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.U4)] public UInt32[] AddrArray;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TXTRecord
        {
            // Generic DNS record structure
            public IntPtr pNext;
            public string pName;
            public short wType;
            public short wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;

            // TXT record specific
            public int dwStringCount;
            public IntPtr pStringArray;

        }

        /// <summary>
        /// See http://msdn.microsoft.com/en-us/library/windows/desktop/cc982162(v=vs.85).aspx
        /// </summary>
        [Flags]
        public enum DnsQueryOptions
        {
            DNS_QUERY_STANDARD = 0x0,
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 0x1,
            DNS_QUERY_USE_TCP_ONLY = 0x2,
            DNS_QUERY_NO_RECURSION = 0x4,
            DNS_QUERY_BYPASS_CACHE = 0x8,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_WIRE_ONLY = 0x100,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_MULTICAST_ONLY = 0x400,
            DNS_QUERY_NO_MULTICAST = 0x800,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_ADDRCONFIG = 0x2000,
            DNS_QUERY_DUAL_ADDR = 0x4000,
            DNS_QUERY_MULTICAST_WAIT = 0x20000,
            DNS_QUERY_MULTICAST_VERIFY = 0x40000,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_DISABLE_IDN_ENCODING = 0x200000,
            DNS_QUERY_APPEND_MULTILABEL = 0x800000,
            DNS_QUERY_RESERVED = unchecked((int)0xF0000000)
        }

        /// <summary>
        /// See http://msdn.microsoft.com/en-us/library/windows/desktop/cc982162(v=vs.85).aspx
        /// Also see http://www.iana.org/assignments/dns-parameters/dns-parameters.xhtml
        /// </summary>
        public enum DnsRecordTypes
        {
            DNS_TYPE_A = 0x1,
            DNS_TYPE_NS = 0x2,
            DNS_TYPE_MD = 0x3,
            DNS_TYPE_MF = 0x4,
            DNS_TYPE_CNAME = 0x5,
            DNS_TYPE_SOA = 0x6,
            DNS_TYPE_MB = 0x7,
            DNS_TYPE_MG = 0x8,
            DNS_TYPE_MR = 0x9,
            DNS_TYPE_NULL = 0xA,
            DNS_TYPE_WKS = 0xB,
            DNS_TYPE_PTR = 0xC,
            DNS_TYPE_HINFO = 0xD,
            DNS_TYPE_MINFO = 0xE,
            DNS_TYPE_MX = 0xF,
            DNS_TYPE_TEXT = 0x10,       // This is how it's specified on MSDN
            DNS_TYPE_TXT = DNS_TYPE_TEXT,
            DNS_TYPE_RP = 0x11,
            DNS_TYPE_AFSDB = 0x12,
            DNS_TYPE_X25 = 0x13,
            DNS_TYPE_ISDN = 0x14,
            DNS_TYPE_RT = 0x15,
            DNS_TYPE_NSAP = 0x16,
            DNS_TYPE_NSAPPTR = 0x17,
            DNS_TYPE_SIG = 0x18,
            DNS_TYPE_KEY = 0x19,
            DNS_TYPE_PX = 0x1A,
            DNS_TYPE_GPOS = 0x1B,
            DNS_TYPE_AAAA = 0x1C,
            DNS_TYPE_LOC = 0x1D,
            DNS_TYPE_NXT = 0x1E,
            DNS_TYPE_EID = 0x1F,
            DNS_TYPE_NIMLOC = 0x20,
            DNS_TYPE_SRV = 0x21,
            DNS_TYPE_ATMA = 0x22,
            DNS_TYPE_NAPTR = 0x23,
            DNS_TYPE_KX = 0x24,
            DNS_TYPE_CERT = 0x25,
            DNS_TYPE_A6 = 0x26,
            DNS_TYPE_DNAME = 0x27,
            DNS_TYPE_SINK = 0x28,
            DNS_TYPE_OPT = 0x29,
            DNS_TYPE_DS = 0x2B,
            DNS_TYPE_RRSIG = 0x2E,
            DNS_TYPE_NSEC = 0x2F,
            DNS_TYPE_DNSKEY = 0x30,
            DNS_TYPE_DHCID = 0x31,
            DNS_TYPE_UINFO = 0x64,
            DNS_TYPE_UID = 0x65,
            DNS_TYPE_GID = 0x66,
            DNS_TYPE_UNSPEC = 0x67,
            DNS_TYPE_ADDRS = 0xF8,
            DNS_TYPE_TKEY = 0xF9,
            DNS_TYPE_TSIG = 0xFA,
            DNS_TYPE_IXFR = 0xFB,
            DNS_TYPE_AFXR = 0xFC,
            DNS_TYPE_MAILB = 0xFD,
            DNS_TYPE_MAILA = 0xFE,
            DNS_TYPE_ALL = 0xFF,
            DNS_TYPE_ANY = 0xFF,
            DNS_TYPE_WINS = 0xFF01,
            DNS_TYPE_WINSR = 0xFF02,
            DNS_TYPE_NBSTAT = DNS_TYPE_WINSR
        }

        #endregion DNSFlags


    }
}
