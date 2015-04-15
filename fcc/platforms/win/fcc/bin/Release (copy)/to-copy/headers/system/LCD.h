#ifndef LCD_H_INCLUDED
#define LCD_H_INCLUDED
#ifndef LCD_SHIFT_2
#define LCD_SHIFT_2 3
#endif
#ifndef LCD_SHIFT_4
#define LCD_SHIFT_4 5
#endif
#ifndef LCD_SHIFT_6
#define LCD_SHIFT_6 7
#endif
void LCDprintA(char * string);
void LCDclearA();
void LCDprintW(char c3,char c2, char c1, char c0);
void LCDshiftA(int shift);
void LCDcaretM(int position);
//
//
//

void LCDprintA(char * string)
{
	char c[4];
	int i=0;
	int j=0;
	LCDclearA();
	for(;*string&&j<8;string++)
	{
		c[i]=*string;
		i++;
		if(i==4)
		{
			LCDprintW(c[0],c[1],c[2],c[3]);
			j++;
			if(j==4)
				LCDcaretM(10*4);
			i=0;
		}
	}
	while (i<4&&i>0)
	{
		c[i]=' ';
		i++;
		if(i==4)
			LCDprintW(c[0],c[1],c[2],c[3]);
	}
	LCDcaretM(6*4);
}
void LCDclearA()
{
}
void LCDprintW(char c3,char c2, char c1, char c0)
{
}
void LCDshiftA(int shift)
{
}
void LCDcaretM(int position)
{
}
#endif
