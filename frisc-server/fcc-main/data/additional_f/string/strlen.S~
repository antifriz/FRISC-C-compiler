rs_puts
		PUSH R0
		PUSH R1
		PUSH R2
		LOAD R1, (SP+010)

rs1puts	
		LOADB R0, (R1)
		CMP R0, 0
		JR_Z rs3puts
		
rs2puts
		LOADB R2, (IO_SIO_STATUS)
		AND R2, SIO_TX_BUSY, R2
		JR_NZ rs2puts
		STOREB R0, (IO_SIO_BYTE)	
		ADD R1, 1, R1
		JR rs1puts		

rs3puts		
		POP R2
		POP R1
		POP R0
		RET
