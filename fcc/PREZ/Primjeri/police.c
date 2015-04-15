#include <LCD.h>
#include <DELAY.h>
char *a="!!! POLICIJA !!!";
char *b=">> SLIJEDI ME <<";
int i;

int main ()
{
	while(1)
	{
		LCDprintA(a);
		for(i=0;i<40000;i++);
		LCDprintA(b);
		for(i=0;i<40000;i++);
	}
	return 0;
}


