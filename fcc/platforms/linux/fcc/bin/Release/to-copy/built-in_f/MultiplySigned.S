;mnozenje 2k 4byte registri, spremanje rezultata na stog iznad parametra
MultiplySigned
		PUSH R0
		PUSH R1
		PUSH R2
		MOVE 0, R2
		LOAD R0, (SP+14); ucitaj prvi broj
		LOAD R1, (SP+10); ucitaj drugi broj
MULSIG1			SHR R1, 1, R1 ;pomakni drugi broj za 1 udesno
		JP_NC MULSIG2
		ADD R0,R2,R2
MULSIG2		SHL R0,1,R0
		CMP R1, 0
		JP_NE MULSIG1 ;rezultat je u R2
		STORE R2, (SP+14) 
		POP R2
		POP R1
		POP R0
		RET
