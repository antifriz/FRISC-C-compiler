;using btn_sw_led
btn_center
		PUSH R0
		LOAD R0, (IO_PUSHBTN)
		AND R0,BTN_CENTER,R0		
		STORE R0, (SP+8)
		POP R0
		RET
