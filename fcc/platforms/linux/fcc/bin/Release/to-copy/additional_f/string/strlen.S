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
