#!/bin/sh

TARGET=*

# clean up old 
rm -f $TARGET.p

# xconas neither provides exit status nor ever returns, so fork it in bg
xconas asm.adl $TARGET.S &

# wait for xconas to produce .hex output (may never succeed)
while [ ! -e $TARGET.p ]
do
	sleep 0.1
done

# terminate xconas, since it won't die by itself
killall -9 xconas

exit 0

../tools/friscasm2srec.tcl $TARGET.p > $TARGET.hex

cd ../../rtl/lattice
../../src/tools/hex2bram.tcl ../../src/$TARGET/$TARGET.hex

