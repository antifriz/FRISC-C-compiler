strrev
		PUSH R0
		PUSH R1
		PUSH R2
		PUSH R3
		LOAD R0, (SP+14)
		MOVE R0, R1

strrev1
		LOADB R2, (R1)
		ADD R1, 1, R1
		CMP R2, 0
		JR_NE strrev1 
		
		SUB R1, 2, R1		
		
strrev2
		CMP R1,R0
		JR_ULE strrev3
		
		LOADB R2, (R0)
		LOADB R3, (R1)
		STOREB R2, (R1)
		STOREB R3, (R0)
		ADD R0, 1, R0
		SUB R1, 1, R1
		JR strrev2		
		
strrev3
		POP R3
		POP R2
		POP R1
		POP R0		
		RET

