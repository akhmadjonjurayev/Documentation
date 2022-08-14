@echo off
set parol=123456
:label_1
set /p tek=parolni kiriting :
if NOT %parol% == %tek% goto label_1
echo parol to'g'ri kiritildi