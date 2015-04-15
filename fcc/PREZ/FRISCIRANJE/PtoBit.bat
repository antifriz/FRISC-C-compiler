echo -----------------------------------------------------------
echo Converting program to .mem format
@echo off
python ..\util\p2mem.py %1.p %2.mem
echo.
echo Running data2mem
data2mem -bm ..\util\kovac_bram.bmm -bt ..\util\frisc_v3a_world.bit -bd %2.mem tag ramb_inst -o b %2.bit
exit