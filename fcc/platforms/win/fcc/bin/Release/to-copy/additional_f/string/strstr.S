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

