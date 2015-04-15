rs_writeln
		PUSH R1
		
		LOAD R1, (SP+8) ; txt
		PUSH R1
		CALL rs_puts

		MOVE 0A, R1 ; \r
		STORE R1, (SP)
		CALL rs_putc

		MOVE 0D,R1 ; \n
		STORE R1, (SP)
		CALL rs_putc
		ADD SP,4,SP

		POP R1
		RET	
