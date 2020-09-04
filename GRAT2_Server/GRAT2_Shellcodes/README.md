## How to generate position-independent shellcode using Donut

- Open Grat2 Client (GRAT2_Client.sln) project using Visual Studio, change the solution configuration from Debug to Release and then Build Solution.
- Download [Donut](https://github.com/TheWover/donut/releases), navigate to the folder and execute the following command:
 .\donut \Path\GRAT2_Client\bin\Release\GRAT2_Client.exe
- Rename the output loader.bin file to grat2shellcode.bin and move it under GRAT2_Server\GRAT2_Shellcodes.