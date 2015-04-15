#include <string.h>
#include <rs232.h>

int main()
{
	int a,b,rez;
	char operacija;
	char buffer[80];
	rs_init();
	rs_writeln("Unesi prvi broj:");
	rs_readln(buffer);
	a=atoi(buffer);
	rs_writeln("Unesi drugi broj:");
	rs_readln(buffer);
	b=atoi(buffer);
	rs_writeln("Unesi operaciju (+, -, *, /)");
	operacija = rs_getc();
	rs_puts("\n");
	rs_puts(itoa(a,buffer,10));
	rs_putc(operacija);
	rs_puts(itoa(b,buffer,10));
	rs_puts("=");
	if(operacija == '+')
		rs_writeln(itoa(a+b,buffer,10));
	else if(operacija == '-')
		rs_writeln(itoa(a-b,buffer,10));
	else if(operacija == '*')
		rs_writeln(itoa(a*b,buffer,10));
	else if(operacija == '/')
		rs_writeln(itoa(a/b,buffer,10));
	
	return 0;
}
