@echo off 
setlocal 
set LOG=%temp%\myPing.log 
set TIMEOUTSEC=300 
if %1.==. ( 
   echo. 
   echo usage:  myPing hostname [logfilename] 
   goto:EOF 
) 
if NOT %2.==. ( 
  set LOG=%2 
) 
 
:PING 
ping "%1" >> "%log%" 
timeout /T %TIMEOUTSEC% 
 
goto :PING 