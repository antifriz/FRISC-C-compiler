#------------------------------------------------------------------------------
#      Name: p2mem
#      File: p2mem.py
#   Author(s): M. Rozic (miroslav@intesis.hr) - MR
#    Date: 18.11.2007.
#------------------------------------------------------------------------------
# FEATURES:
#    Conversion of CONAS output .p files to the xilinx .mem file used for
#    loading object code into FPGA BRAMs
#------------------------------------------------------------------------------
# CLASSES:
#------------------------------------------------------------------------------
# REQUIREMENTS:
#------------------------------------------------------------------------------
# REVISIONS:
#  * 0.1   18.11.07   MR initial
#------------------------------------------------------------------------------

import sys

def parse_line(in_line):
    "parses a line and returns either none or a list describing the code line"
    # split line
    items = in_line.split()
    # the line is not a code line if it: 
    #a) empty
    #b)starts with a ';'
    #c) any line item has an illegal conas pseudocommand in it 
    # * `ORG
    # * `EQU
    # * `DS
    # * `BASE
    # * `END
    # case a)
    if len(items) == 0:
        return None
    # case b)
    if items[0].find(';') >= 0:
        return None
    # case c)
    for i in items:
        assert isinstance(i,str)
        if (i.find('`ORG') >= 0) or \
           (i.find('`EQU') >= 0) or \
           (i.find('`BASE') >= 0) or \
           (i.find('`END') >= 0) or \
           (i.find('`DS') >= 0):
            return None
    # convert first data item into a number
    try:
        adr = int(items[0],16)
    except ValueError:
        return None
    return [adr,items[1:5]]

        
        

# main function
def main():
    """p2mem v0.1."""
    if len(sys.argv) < 2:
        print "p2mem v0.1"
        print "Converts CONAS output .p file to Xilinx .mem file."
        print "Usage: p2mem input_file [output_file]"
    else:
        print "Opening input file: "+sys.argv[1]
        # attempt opening input file
        try:
            ifile = open(sys.argv[1],'r')
        except IOError:
            print "Invalid input file."
            return None
        # attempt opening output file
        try:
            if len(sys.argv) < 3:    
                print "Opening output file: "+sys.argv[1]+'.mem'
                ofile = open(sys.argv[1]+'.mem','w')  
            else:
                print "Opening output file: "+sys.argv[2]
                ofile = open(sys.argv[2],'w')
        except IOError:
            print "Invalid output file."
            return None
        assert isinstance(ifile,file)
        assert isinstance(ofile,file)
        adr_declared = False
        code_size = 0
        for line in ifile:
            assert isinstance(line,str)
            items = parse_line(line)
            # the line is not valid if parse_line returns None
            if items != None:
                # first iteration, in case the address was not previously declared.
                if not(adr_declared):
                    prev_adr = items[0]
                    code_size_total = 0
                # fetch address from parsed items.
                adr = items[0]
                if(adr != (prev_adr+4)):
                    # if address is already declared, write bytes per block report
                    if(adr_declared):
                        print '%d bytes.'%code_size
                        code_size_total += code_size
                        code_size = 0
                    # write block address and block report
                    print "Writing block of code at:"+('0x%04X'%adr)
                    ofile.write(('@%04X'%adr)+'\n')
                    # update address declared
                    if not(adr_declared):
                        adr_declared = True
                # write instruction data
                ofile.write((' '.join(items[1]))+'\n')
                # increment code size and address pointer
                prev_adr = adr
                code_size += 4
        # close input and output files
        ifile.close()
        ofile.close()
        # write final report
        print '%d bytes.'%code_size
        code_size_total += code_size
        print 'Written %d bytes in total.'%code_size_total
        print 'Address space used: %d (%04X) bytes.'%(adr+4,adr+4)
        print 'Check not to exceed address space size.'
        print 'Finished.'
        
# entry point invocation            
if __name__ == '__main__':
    main()
