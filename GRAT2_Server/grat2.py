#!/usr/bin/env python3
# https://medium.com/@andrewklatzke/creating-a-python3-webserver-from-the-ground-up-4ff8933ecb96
import time
from http.server import HTTPServer
from handlers import MultiThreading, Server
from tasks import Console,AgentConsole
from threading import Thread
import sys, os
from termcolor import colored
import shutil

HOST_NAME = '0.0.0.0'
PORT_NUMBER = 80

if __name__ == '__main__':

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
    banner[9]="v1.0 beta!"
    for i,val in enumerate(banner):
        if i==4:
            print(colored(val.center(columns),'red'))
            continue
        print(colored(val.center(columns),'blue'))

    try:
        http_server = MultiThreading((HOST_NAME, PORT_NUMBER), Server)
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
    
