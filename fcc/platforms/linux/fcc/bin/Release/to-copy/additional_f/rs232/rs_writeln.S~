;using rs_putc
;using rs_puts
rs_writeln
		PUSH R0
		
		LOAD R0, (SP+8) ; txt
		PUSH R0
		CALL rs_puts

		MOVE 0A, R0 ; \r
		STORE R0, (SP)
		CALL rs_putc

		MOVE 0D,R0 ; \n
		STORE R0, (SP)
		CALL rs_putc
		ADD SP,4,SP

		POP R0
		RET	
