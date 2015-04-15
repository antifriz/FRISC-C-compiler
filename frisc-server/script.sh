#!/bin/bash
#gcc $1
#mv /tmp/$1 /tmp/$1.c
#cd ./tmp/
#cp ./$1.c ./$1.S
#../fcc-main/fcc.exe ./$1.c
#gcc -ffreestanding -fdump-tree-gimple -Ifcc-main/data/headers/system

#rm -f ./$1.c
#../fcc-main/fcc.exe ../$1.c
#mv $1.S ../tmp/$1.S


cd ./fcc-main/
#cp ./../tmp/$1.c ./../tmp/$1.s
./fcc.exe ./../tmp/$1.c
rm ./data/*.*
rm -f ./../tmp/$1.c
echo $(date)>>./../tmp/data.txt
