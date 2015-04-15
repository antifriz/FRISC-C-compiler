#include <rs232.h>
#include <string.h>

int main()
{
	char buffer[80];
	int i,l;
	rs_init();
	rs_writeln("Unesi tekst");
	rs_readln(buffer);
	l=strlen(buffer);
	for(i=0;i<l/2;i++)
		if(buffer[i]!=buffer[l-1-i])
		{
			rs_writeln("Nije palindrom");
			return;
		}
	rs_writeln("Palindrom je");
	return 0;
}
