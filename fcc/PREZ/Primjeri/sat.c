#include <string.h>
#include <LCD.h>
#define SECOND 1017
#define HOUR_FORMAT 24
#define NUM_BASE 10

int hours = 10;
int minutes=21;
int seconds=15;

char c[20];

void advance()
{
	seconds++;
	if(seconds!=59)
		return;
	seconds=0;
	minutes++;
	if(minutes!=59)
		return;
	minutes=0;
	hours++;
	if(hours!=HOUR_FORMAT-1)
		return;
	hours=0;
}

void setval(char * s,int n)
{
	char m[2];	
	itoa(n,m,NUM_BASE);
	if(m[1]=='\0')
	{
		m[1]=m[0];
		m[0]='0';
	}
	s[0]=m[0];
	s[1]=m[1];
}

int main()
{
	char *s;
	int i;
	while(1)
	{
		advance();
		s=c;
		setval(s,hours);
		s+=3;
		setval(s,minutes);
		s+=3;
		setval(s,seconds);
		c[2]=':';
		c[5]=':';
		LCDprintA(c);
		
		for(i=0;i<1500<<5;i++);
	}
}

