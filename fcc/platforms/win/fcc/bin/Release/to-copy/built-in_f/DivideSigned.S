DivideSigned	PUSH R0
			PUSH R1
			PUSH R2
			PUSH R3
			PUSH R4 ;boolean za predznak
			MOVE 0, R4
			LOAD R1, (SP+1C) ;ucitaj veci ili prvi
			LOAD R2, (SP+18) ;ucitaj manji ili drugi
			MOVE 0, R0 ;rezultat se sprema u R0
			MOVE 1, R3 ;na neki nacin kompas
			CMP R2, 0
			JP_EQ DIVSIG7 ;ne smijes djeliti sa nulom
			JP_SGT DIVSIG1
			ADD R4, 1, R4 ;zbroji sa 1 ako je negativno 
			SUB R0, R2, R2
DIVSIG1			CMP R1, 0 	   ;ako je pozitivan idi DIVSIG3 xd
			JP_SGT DIVSIG2
			ADD R4, 1, R4 ;zbroji sa 1 ako je negativno
			SUB R0, R1, R1

DIVSIG2			CMP R2, R1
			JP_SGT DIVSIG3
			SHL R2, 1, R2
			SHL R3, 1, R3
			JP DIVSIG2
DIVSIG3		CMP R1, R2	   ;zelimo da R1>R2
			JP_SLT DIVSIG4 ;ako nije 
			SUB R1,R2,R1
			ADD R0,R3,R0
DIVSIG4		SHR R3,1,R3
			JP_C DIVSIG5
			SHR R2,1,R2
			JP DIVSIG3
DIVSIG5			SHR R4,1,R4
			JP_NC DIVSIG6
			MOVE 0, R3
			SUB R3,R0,R0 ;baci minus na rezultat
DIVSIG6		STORE R0,(SP+1C)
			POP R4
			POP R3
			POP R2
			POP R1
			POP R0
			RET
DIVSIG7		HALT
