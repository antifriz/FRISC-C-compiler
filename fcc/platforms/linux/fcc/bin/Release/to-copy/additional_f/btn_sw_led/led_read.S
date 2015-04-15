;using btn_sw_led
led_read
		PUSH R0
		LOAD R0, (IO_LED)
		STORE R0, (SP+8)
		POP R0
		RET
