import sys

def parseLine(string):
	ret = []
	for i in range(4, 36):
		ret.append(string.split(' ')[i])
	return ret

def listToMem(list, bram, lineNum):
	startAdr = lineNum*32*4+bram
	for i in range(32):
		adr = startAdr+i*4
		mem[adr] = list[i]

try:
	file = sys.argv[1]
except:
	print 'Usage: simpleMem.py <input file> <hex range start> <hex range end>'
	exit()

try:
	fp = open(file)
except IOError as e:
	print e.strerror + ' ' + sys.argv[1]
	exit()

try:
	rStart = int(sys.argv[2], 16)//4*4
except IndexError:
	rStart = 0
except:
	print 'Usage: simpleMem.py <input file> <hex range start> <hex range end>'
	exit()

try:
	rEnd = (int(sys.argv[3], 16)//4+1)*4
except IndexError:
	rEnd = int('1fff', 16)
except:
	print 'Usage: simpleMem.py <input file> <hex range start> <hex range end>'
	exit()

mem = [None]*(int('1fff', 16)+1)
rambStart = [6807, 6739, 6875, 6671]

fLine = fp.readlines()

for bram in range(4):
	for lineNum in range(64):
		list = parseLine(fLine[rambStart[bram]+lineNum])
		listToMem(list, bram, lineNum)

fp.close()

adrPrintWidth = len(hex(rEnd))-2

print ''

for i in range (rStart, rEnd, 4):
	pAdr = hex(i).upper()[2:]
	print pAdr.rjust(adrPrintWidth) + ': '+str(mem[i]) + ' '+str(mem[i+1]) + ' '+str(mem[i+2]) + ' '+str(mem[i+3])

print ''
