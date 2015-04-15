#include <string.h>
#include <rs232.h>

int i=4;
int main()
{
	char buffer[]="8";
	char * p = "mate";
	rs_init();
	rs_readln(buffer);
	rs_puts("Imas ");
	rs_puts(buffer);
	//rs_writeln(" godina");
	//rs_puts("Za 10 godina imati ces ");
	i=atoi(buffer);
	rs_puts(itoa(i,buffer,10));
//	rs_writeln(" godina");
	return 0;
}
