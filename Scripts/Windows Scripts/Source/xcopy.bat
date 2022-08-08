@echo off
set mypath=%cd%
set parol=Men1ng)f1sK0mpyuter1m
set serverpath=192.168.40.95\D:\Test
rem set serverpath=KALINUS\D:\Test
@echo %mypath%
@echo %serverpath%
@echo %parol%
net use \\%serverpath% /user:kalin %parol%
copy \\%serverpath% %mypath% *.* /s
