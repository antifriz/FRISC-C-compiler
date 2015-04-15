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

