strlen
		PUSH R0
		PUSH R1
		PUSH R2
		LOAD R0, (SP+10)
			
		MOVE -1, R2
strlen1
		LOADB R1, (R0)
		ADD R2, 1, R2
		ADD R0, 1, R0
		CMP R1, 0
		JR_NE strlen1
			
		STORE R2, (SP+10)
strlen2			
		POP R2
		POP R1
		POP R0	
		RET
		
		
		
		
		
strcat
		PUSH R0
		PUSH R1
		PUSH R2
		LOAD R0, (SP+14)
		LOAD R1, (SP+10)

strcat1
		LOADB R2, (R0)
		ADD R0, 1, R0
		CMP R2, 0
		JR_NE strcat1
		
strcat2
		LOADB R2, (R1)
		STOREB R2, (R0-1)
		ADD R0, 1, R0
		ADD R1, 1, R1
		CMP R2, 0
		JR_NE strcat2
		
		;rez vec je na (SP+14)	 
		POP R2
		POP R1
		POP R0
		RET




strcmp
		PUSH R0
		PUSH R1
		PUSH R2
		PUSH R3
		LOAD R0, (SP+18)
		LOAD R1, (SP+14)

strcmp1
		LOADB R2, (R0)
		LOADB R3, (R1)
		CMP R2, 0
		JR_Z strcmp2
		
		CMP R3, 0
		JR_Z strcmp3
		ADD R0, 1, R0
		ADD R1, 1, R1
		CMP R2, R3
		JR_Z strcmp1
		
		SUB R3, R2, R0
		STORE R0, (SP+18)
		JR strcmp5
		
strcmp2
		CMP R3, 0
		JR_Z strcmp4
		MOVE 0, R2
		SUB R2, R3, R2

strcmp3		
		MOVE R2,R3

strcmp4 
		STORE R3, (SP+18)
		
strcmp5
		POP R3
		POP R2
		POP R1 
		POP R0
		RET
		
		
		
		
		
strcpy
		PUSH R0
		PUSH R1
		PUSH R2
		LOAD R0, (SP+14)
		LOAD R1, (SP+10)
		
strcpy1
		LOADB R2, (R1)
		STOREB R2, (R0)
		ADD R0, 1, R0
		ADD R1, 1, R1
		CMP R2, 0
		JR_NE strcpy1
		
		;rez je vec na (SP+14)
		POP R2
		POP R1
		POP R0
		RET
		
		
		
		
		
strstr
		PUSH R0
		PUSH R1
		PUSH R2
		PUSH R3
		PUSH R4
		LOAD R0, (SP+1C)

strstr1		
		MOVE R0, R4
		LOAD R1, (SP+18)
		
strstr2		
		LOADB R2, (R4)
		LOADB R3, (R1)

		CMP R3,0
		JR_Z strstr4
		
		CMP R2,0
		JR_Z strstr3		

		ADD R4,1,R4
		ADD R1,1,R1
		CMP R2,R3
		JR_Z strstr2
		
		ADD R0,1,R0
		JR strstr1

strstr3
		MOVE 0,R0	

strstr4
		STORE R0, (SP+1C)				
		POP R4
		POP R3
		POP R2
		POP R1
		POP R0		
		RET
		
		
		
		
		
		
atoi
		PUSH R0
		PUSH R1
		PUSH R2
		PUSH R3
		LOAD R0, (SP+14)
		MOVE 0, R2
		
atoi1
		SHL R2, 3, R3
		SHL R2, 1, R2
		ADD R2, R3, R2
		ADD R2, R1, R2
		
		LOADB R1, (R0)
		CMP R1,30
		JR_ULT atoi2
				
		CMP R1,39
		JR_UGT atoi2

		SUB R1, 30, R1
		ADD R0, 1, R0
		JR atoi1
				
atoi2	
		STORE R2, (SP+14)			
		POP R3
		POP R2
		POP R1
		POP R0
		RET		

