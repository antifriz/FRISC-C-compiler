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

