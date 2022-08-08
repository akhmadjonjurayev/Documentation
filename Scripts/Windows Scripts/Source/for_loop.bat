echo off
for /l %%i in (10,-2,0) do ( echo %%i)
FOR /F "tokens=1-5" %%A IN ("This is a short sentence") DO @echo %%A %%B %%D %%C