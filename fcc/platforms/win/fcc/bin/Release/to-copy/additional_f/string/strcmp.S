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

