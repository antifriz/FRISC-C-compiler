LCDcaretM	PUSH R0

LCDCAM1	LOAD R0, (0FFFFF008)
		AND R0, 1, R0
		JP_Z LCDCAM1

		LOAD R0,(SP+8)
		AND R0,7F,R0
		SHL R0,9,R0
		ADD R0,100,R0
		STORE R0, (0FFFFF004)
			
		POP R0
		RET
