#!/usr/bin/env python3
# https://medium.com/@andrewklatzke/creating-a-python3-webserver-from-the-ground-up-4ff8933ecb96
# https://rosettacode.org/wiki/HTTPS/Client-authenticated , https://gist.github.com/dergachev/7028596
import time
from http.server import HTTPServer
from handlers import MultiThreading, Server
from tasks import Console,AgentConsole
from threading import Thread
import sys, os
from termcolor import colored
import shutil
import ssl
import subprocess
import tasks

# Configure HTTP/s Listener
HOST_NAME = '0.0.0.0'
PORT_NUMBER = 80
server_pem = "./server.pem"
checkif_pem_exists = os.path.isfile(server_pem)

# Configure DNS Listener
tasks.define_dns_domain.dnsDomainName = ""

if __name__ == '__main__':

    if PORT_NUMBER == 443 and checkif_pem_exists != True:
        print("\n")
        print("Generating Privacy Enhanced Mail (PEM) file.\n")
        subprocess.Popen("openssl req -new -x509 -keyout server.pem -out server.pem -days 365 -nodes -subj '/C=US/ST=NRW/L=Earth/O=CompanyName/OU=IT/CN=www.example.com/emailAddress=email@example.com'",stdin=subprocess.PIPE, shell=True)
        time.sleep(2)
        print("\n")

    columns=shutil.get_terminal_size().columns
    banner=['']*10
    banner[0]="(c).-.(c)    (c).-.(c)    (c).-.(c)    (c).-.(c)    (c).-.(c)" 
    banner[1]=" / ._. \      / ._. \      / ._. \      / ._. \      / ._. \ " 
    banner[2]=" \( Y )/__  __\( Y )/__  __\( Y )/__  __\( Y )/__  __\( Y )/__"
    banner[3]="(_.-/'-'\-._)(_.-/'-'\-._)(_.-/'-'\-._)(_.-/'-'\-._)(_.-/'-'\-._)"
    banner[4]="""   || G ||      || R ||      || A ||      || T ||      || 2 ||   """
    banner[5]=" _.' `-' '._  _.' `-' '._  _.' `-' '._  _.' `-' '._  _.' `-' '._"
    banner[6]="(.-./`-'\.-.)(.-./`-`\.-.)(.-./`-'\.-.)(.-./`-'\.-.)(.-./`-'\.-.)"
    banner[7]="`-'     `-'  `-'     `-'  `-'     `-'  `-'     `-'  `-'     `-' "
    banner[8]=""
    banner[9]="v1.1 beta!"

    for i,val in enumerate(banner):
        if i==4:
            print(colored(val.center(columns),'red'))
            continue
        print(colored(val.center(columns),'blue'))
    try:
        if PORT_NUMBER==80:
            http_server = MultiThreading((HOST_NAME, PORT_NUMBER), Server)
        else:
            http_server = MultiThreading((HOST_NAME, PORT_NUMBER), Server)
            http_server.socket = ssl.wrap_socket (http_server.socket, certfile=server_pem, server_side=True)
    except OSError:
        sys.exit(1)

    print(time.asctime(), 'GRAT2 UP - %s:%s' % (HOST_NAME, PORT_NUMBER))
    Openconsole = Thread(target = Console().cmdloop)
    Openconsole.start()

    try:
        http_server.serve_forever()
    except KeyboardInterrupt:
        http_server.shutdown()
        os._exit(0)
    