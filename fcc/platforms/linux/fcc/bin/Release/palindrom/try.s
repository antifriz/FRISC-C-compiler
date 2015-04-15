	.file	"try.c"
	.text
	.globl	main
	.type	main, @function
main:
.LFB0:
	.cfi_startproc
	pushl	%ebp
	.cfi_def_cfa_offset 8
	.cfi_offset 5, -8
	movl	%esp, %ebp
	.cfi_def_cfa_register 5
	andl	$-16, %esp
	subl	$48, %esp
	movl	%gs:20, %edx
	movl	%edx, 44(%esp)
	xorl	%edx, %edx
	call	v
	movl	%eax, %edx
	movb	%dl, 6(%esp)
	movl	$0, %edx
	movl	%edx, %eax
	movl	44(%esp), %edx
	xorl	%gs:20, %edx
	je	.L2
	call	__stack_chk_fail
.L2:
	leave
	.cfi_restore 5
	.cfi_def_cfa 4, 4
	ret
	.cfi_endproc
.LFE0:
	.size	main, .-main
	.ident	"GCC: (Ubuntu/Linaro 4.6.3-1ubuntu5) 4.6.3"
	.section	.note.GNU-stack,"",@progbits
