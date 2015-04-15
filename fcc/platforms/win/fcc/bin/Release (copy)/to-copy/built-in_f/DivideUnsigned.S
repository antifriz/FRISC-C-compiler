DivideUnsigned	PUSH R0
			PUSH R1
			PUSH R2
			PUSH R3
			LOAD R1, (SP+18) ;ucitaj veci ili prvi
			LOAD R2, (SP+14) ;ucitaj manji ili drugi
			CMP R2, 0
			JP_EQ DIVUSG5 ;ne smijes djeliti sa nulom
			MOVE 0, R0 ;rezultat se sprema u R0
			MOVE 1, R3 ;na neki nacin kompas 
DIVUSG1			CMP R2, R1
			JP_UGT DIVUSG2
			SHL R2, 1, R2
			SHL R3, 1, R3
			JP DIVUSG1
DIVUSG2		CMP R1, R2	   ;zelimo da R1>R2
			JP_ULT DIVUSG3 ;ako nije 
			SUB R1,R2,R1
			ADD R0,R3,R0
DIVUSG3		SHR R3,1,R3
			JP_C DIVUSG4
			SHR R2,1,R2
			JP DIVUSG2
DIVUSG4			STORE R0,(SP+14)
			POP R3
			POP R2
			POP R1
			POP R0
			RET
DIVUSG5		HALT
