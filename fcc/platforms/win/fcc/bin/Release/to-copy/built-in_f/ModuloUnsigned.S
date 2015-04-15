ModuloUnsigned	PUSH R0
			PUSH R1
			PUSH R2
			PUSH R3
			LOAD R1, (SP+18) ;ucitaj veci ili prvi
			LOAD R2, (SP+1C) ;ucitaj manji ili drugi
			CMP R2, 0
			JP_EQ MODSIG5 ;ne smijes djeliti sa nulom
			MOVE 0, R0 ;rezultat se sprema u R0
			MOVE 1, R3 ;na neki nacin kompas 
MODSIG1			CMP R2, R1
			JP_UGT MODSIG2
			SHL R2, 1, R2
			SHL R3, 1, R3
			JP MODSIG1
MODSIG2		CMP R1, R2	   ;zelimo da R1>R2
			JP_ULT MODSIG3 ;ako nije 
			SUB R1,R2,R1
			ADD R0,R3,R0
MODSIG3		SHR R3,1,R3
			JP_C MODSIG4
			SHR R2,1,R2
			JP MODSIG2
MODSIG4			STORE R1,(SP+1C)
			POP R3
			POP R2
			POP R1
			POP R0
			RET
MODSIG5		HALT
