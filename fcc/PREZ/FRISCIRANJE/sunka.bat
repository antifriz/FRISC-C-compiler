@echo off

::GASI CYGWIN AKO JE VEC UPALJEN I PALI GA OPET
taskkill /IM XWin.exe
taskkill /IM Xconas.exe

::LOKACIJA CYGWINA
cd "C:\Program Files (x86)\Atlas\cygwin"

::NE DIRAJTE AKO NE ZNATE
for /f "tokens=*" %%a in ('cd') do set CYGWIN_ROOT=%%a
set HOME=%HOMEDRIVE%%HOMEPATH%
set HOME=%~dp0
set PATH=/bin:/usr/bin:/usr/local/bin:/cygdrive/c/WINDOWS/system32:/cygdrive/c/WINDOWS:/cygdrive/c/WINDOWS/System32/Wbem
set SHELL=/bin/bash

::RUNNANJE CYGWINA IZ LOKACIJE .bat FAJLA
cd %~dp0
"%CYGWIN_ROOT%\bin\run.exe" /bin/bash.exe -l -c "/bin/startxwin.exe"