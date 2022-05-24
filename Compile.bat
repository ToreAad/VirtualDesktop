@echo off
:: Markus Scholtes, 2022
:: Compile VirtualDesktop in .Net 4.x environment
setlocal

C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe -out:VirtualDesktop11Insider.exe "%~dp0VDeskTool.cs" "%~dp0VirtualDesktop11Insider.cs" /win32icon:"%~dp0MScholtes.ico" 
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe -out:dp0VirtualDesktop11.exe "%~dp0VDeskTool.cs" "%~dp0VirtualDesktop11.cs" /win32icon:"%~dp0MScholtes.ico" 
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe -out:dp0VirtualDesktop.exe "%~dp0VDeskTool.cs" "%~dp0VirtualDesktop.cs" /win32icon:"%~dp0MScholtes.ico" 
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe -out:dp0VirtualDesktopServer2022.exe "%~dp0VDeskTool.cs" "%~dp0VirtualDesktopServer2022.cs" /win32icon:"%~dp0MScholtes.ico" 
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe -out:dp0VirtualDesktop1803.exe "%~dp0VDeskTool_1607_1803.cs" "%~dp0VirtualDesktop1803.cs" /win32icon:"%~dp0MScholtes.ico" 
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe -out:dp0VirtualDesktop1607.exe "%~dp0VDeskTool_1607_1803.cs" "%~dp0VirtualDesktop1607.cs" /win32icon:"%~dp0MScholtes.ico" 

:: was batch started in Windows Explorer? Yes, then pause
echo "%CMDCMDLINE%" | find /i "/c" > nul
if %ERRORLEVEL%==0 pause
