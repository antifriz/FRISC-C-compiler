IO_GPIO_DATA	`EQU	ffffff00	; word, RW
IO_GPIO_CTL	`EQU	ffffff04	; word, WR
IO_LED		`EQU	ffffff10	; byte, WR
IO_PUSHBTN	`EQU	ffffff10	; byte, RD
IO_DIPSW	`EQU	ffffff11	; byte, RD
IO_SIO_BYTE	`EQU	ffffff20	; byte, RW
IO_SIO_STATUS	`EQU	ffffff21	; byte, RD
IO_SIO_BAUD	`EQU	ffffff22	; half, WR
IO_SPI_FLASH	`EQU	ffffff30	; half, RW
IO_SPI_SDCARD	`EQU	ffffff34	; half, RW
IO_FB		`EQU	ffffff40	; word, WR
IO_PCM_FIRST	`EQU	ffffff50	; word, WR
IO_PCM_LAST	`EQU	ffffff54	; word, WR
IO_PCM_FREQ	`EQU	ffffff58	; word, WR
IO_PCM_VOLUME	`EQU	ffffff5c	; word, WR

; definicije kojekakvih bitmaska
SIO_TX_BUSY	`EQU	04
SIO_RX_OVERRUN	`EQU	02
SIO_RX_FULL	`EQU	01

BTN_CENTER	`EQU	10
BTN_UP		`EQU	08
BTN_DOWN	`EQU	04
BTN_LEFT	`EQU	02
BTN_RIGHT	`EQU	01
	
	
		`ORG 400
		MOVE BUFF, R0
		PUSH R0
		CALL rs_readln
		POP R0

		PUSH R0
		CALL rs_writeln
		POP R0
		PUSH R0
		CALL rs_writeln
		POP R0
		PUSH R0
		CALL rs_writeln
		POP R0
		RET
		
rs_readln
		PUSH R0
		PUSH R1
		LOAD R1,(SP+0C)
		SUB R1,1,R1
		
rs1readln
		LOADB R0, (IO_SIO_STATUS)
		AND R0, SIO_RX_FULL, R0
		JR_Z rs1readln
		LOADB R0, (IO_SIO_BYTE)
				
		
		ADD R1,1,R1
		CMP R0,0D ; == \r
		JR_Z rs2readln
		CMP R0,0A ; == \n
		JR_Z rs2readln		
		
		PUSH R0
		CALL rs_putc
		POP R0
		
		STOREB R0, (R1)
		JR rs1readln

rs2readln
		MOVE 0,R0
		STOREB R0,(R1)
		
		POP R1
		POP R0
		RET
		
		
rs_getc
		PUSH R0

rs1getc
		LOADB R0, (IO_SIO_STATUS)
		AND R0, SIO_RX_FULL, R0
		JR_Z rs1getc
		LOADB R0, (IO_SIO_BYTE)
		STORE R0, (SP+08)

		POP R0
		RET

		
		
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
		
		
rs_putc		
		PUSH R0
		PUSH R1
		LOAD R0, (SP+0C)
		
rs1putc
		LOADB R1, (IO_SIO_STATUS)
		AND R1, SIO_TX_BUSY, R1
		JR_NZ rs1putc
		STOREB R0, (IO_SIO_BYTE)
		POP R1
		POP R0
		RET
		
;TXT	
		;"Booting at\n"
		;DB 42, 6f, 6f, 74
		;DB 69, 6e, 67, 20 
		;DB 61, 74, 00, 00
		
		DW 0,0,0,0,0,0,0,0,0,0,0,0
BUFF 
		`DS 80
	

