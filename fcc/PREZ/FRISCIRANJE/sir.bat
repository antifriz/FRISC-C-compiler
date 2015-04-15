@echo on
::LOCIRANJE U TRENUTNI DIREKTORIJ
cd %~dp0

::BIN TO BIT
bintobit impact.bin vj00.bit vj000.bit

PAUSE

::BIT TO TXT
start bittotxt vj000.bit rezultat.txt
PAUSE

::RADI PREGLEDNOSTI
prevedime

PAUSE

::REZULTAT !!! SU NEKAKVE MOGUCE PROMJENE S OBZIROM NA POCETNO STANJE MEMORIJE (.p FAJL)
start SENDVIC.txt

exit
