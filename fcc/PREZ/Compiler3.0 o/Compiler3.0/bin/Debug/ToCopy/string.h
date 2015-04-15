#ifndef STRING_H_INCLUDED
#define STRING_H_INCLUDED
void inttostring(int integer,char * string);
//
//
int strlen(char *str)
{
	int i=0;
	while(str[i]!=0)
		i++;
	return i;
}

char *strrev(char *str)
{
    char c, *front, *back;

    for(front=str,back=str+strlen(str)-1;front < back;front++,back--)
	{
        c=*front;*front=*back;*back=c;
    }
    return str;
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
#endif
