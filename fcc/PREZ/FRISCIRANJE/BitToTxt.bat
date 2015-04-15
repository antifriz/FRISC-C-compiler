@echo on
echo -----------------------------------------------------------
echo Converting bit to text format
data2mem -bm ..\util\kovac_bram.bmm -bt %1 -d > %2
exit