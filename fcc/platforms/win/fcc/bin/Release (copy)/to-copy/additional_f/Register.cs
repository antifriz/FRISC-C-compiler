;using strlen
strstr
				PUSH R0
				PUSH R1
				PUSH R2
				LOAD R0, (SP +0C)
				LOAD R1, (SP +10)
				
				PUSH R0
				CALL strlen
				LOAD R2, (SP)
				ADD SP,4,SP
				ADD R0,R2,R0
strstr1
				LOAD R2, (R1)
				CMP R2,0
				JR_Z strstr2
				STORE R2, (R0)
				ADD R0,1,R0
				ADD R1,1,R1
				JR strstr1
				
				
				STORE R0, (SP+10)
strstr2
				STORE R2,(R0)
				POP R2
				POP R1
				POP R0
				RET
