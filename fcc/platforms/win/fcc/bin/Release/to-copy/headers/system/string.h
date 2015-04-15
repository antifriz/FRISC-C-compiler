#if !defined(NULL)
    #define NULL ((void*)0)
#endif

int strlen(char * s);
char * strcat (char * d, char * s);
int strcmp(char * a, char * b);
char * strcpy(char * d, char * s);
char * strstr(char * d, char * s);
char * strrev(char * s);
char * itoa(int broj, char *c, int baza);
int atoi(char * c);
//
//
//

int strlen(char * s)
{
}
char * strcat (char * d, char * s)
{
}
int strcmp(char * a, char * b)
{
}
char * strcpy(char * d, char * s)
{
}
char * strstr(char * d, char * s)
{
}
int atoi(char * c)
{
}
char *strrev(char * s)
{
}
char *itoa(int broj, char *c, int baza)
{
	int n=broj;
	char table[16]="0123456789abcdef";
	char * s;
	s=c;
	if(broj<0) 
		broj=-broj;
	while(broj>=baza)
	{
		*s++=table[broj%baza];
		broj/=baza;
	}
	*s++=table[broj];
    if(n < 0) 
		*s++='-';
	*s='\0';
	strrev(c);
	return (c);
}
