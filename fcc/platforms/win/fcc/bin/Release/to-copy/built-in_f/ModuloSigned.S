ModuloSigned	PUSH R0
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
			JP_EQ MODUSG7 ;ne smijes djeliti sa nulom
			JP_SGT MODUSG1
			ADD R4, 1, R4 ;zbroji sa 1 ako je negativno 
			SUB R0, R2, R2
MODUSG1			CMP R1, 0 	   ;ako je pozitivan idi MODUSG3 xd
			JP_SGT MODUSG2
			ADD R4, 1, R4 ;zbroji sa 1 ako je negativno
			SUB R0, R1, R1

MODUSG2			CMP R2, R1
			JP_SGT MODUSG3
			SHL R2, 1, R2
			SHL R3, 1, R3
			JP MODUSG2
MODUSG3		CMP R1, R2	   ;zelimo da R1>R2
			JP_SLT MODUSG4 ;ako nije 
			SUB R1,R2,R1
			ADD R0,R3,R0
MODUSG4		SHR R3,1,R3
			JP_C MODUSG5
			SHR R2,1,R2
			JP MODUSG3
MODUSG5			CMP R4,0
			JP_EQ MODUSG6
			MOVE 0, R3
			SUB R3,R1,R1 ;baci minus na rezultat
MODUSG6		STORE R1,(SP+1C)
			POP R4
			POP R3
			POP R2
			POP R1
			POP R0
			RET
MODUSG7		HALT
