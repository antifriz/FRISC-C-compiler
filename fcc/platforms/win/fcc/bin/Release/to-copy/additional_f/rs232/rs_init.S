rs_init 
	RET
;lokacije rs232
IO_SIO_BYTE	`EQU	ffffff20	; byte, RW
IO_SIO_STATUS	`EQU	ffffff21	; byte, RD
IO_SIO_BAUD	`EQU	ffffff22	; half, WR

; definicije kojekakvih bitmaska
SIO_TX_BUSY	`EQU	04
SIO_RX_OVERRUN	`EQU	02
SIO_RX_FULL	`EQU	01
	
