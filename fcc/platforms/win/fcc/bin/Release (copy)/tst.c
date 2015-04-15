#include <rs232.h>

int main()
{
	char buffer[80];
	rs_init();
	rs_writeln("Koliko imas godina pitah te?");
	rs_readln(buffer);
	rs_writeln("");
	rs_puts("Imas ");
	rs_puts(buffer);
	rs_writeln(" godina");
	return 0;
}
