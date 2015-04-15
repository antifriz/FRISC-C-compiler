;using btn_sw_led
sw_status
		PUSH R0
		LOAD R0, (IO_DIPSW)
		STORE R0, (SP+8)
		POP R0
		RET
