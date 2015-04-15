#ifndef DELAY_H_INCLUDED
#define DELAY_H_INCLUDED
void delay(int clocks);
//
//
void delay(int clocks)
{
	clocks=clocks<<5;
	for(;clocks>0;clocks--);
}
#endif
