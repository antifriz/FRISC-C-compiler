#include <stdio.h>
#include <string.h>

int nadi(char *txt,FILE *cit)
{
	char com[12],prvi,p;
	while(EOF!=(prvi=fgetc(cit)))
	{	
		if(prvi==txt[0])
		{	
			fgets(com,7,cit);
			if(strncmp(com,txt+1,7)==0)
				break;
		}		
	}	
	
	if(EOF!=prvi)
	{
		fgets(com,12,cit);
		fgets(com,12,cit);
		return 1;
	}
	return 0;
}

int main()
{
	int i,j,cnt=0,n;
	long of;
	char c,buffer1[100],buffer2[100],buffer3[100],buffer4[100],buffer5[4],buffer6[4],buffer7[4],buffer8[4];
	FILE *cit1,*cit2,*cit3,*cit4,*pot,*pis;
	cit1=fopen("rezultat.txt","r");
	cit2=fopen("rezultat.txt","r");
	cit3=fopen("rezultat.txt","r");
	cit4=fopen("rezultat.txt","r");
	pot=fopen("vj0.p","r");
	pis=fopen("SENDVIC.txt","w");	
		
	if((nadi("RAMB_00",cit1)*nadi("RAMB_01",cit2)*nadi("RAMB_02",cit3)*nadi("RAMB_03",cit4))==0)//
	{	
		puts("ERROR!!!\n");
		return 0;
	}
	
	for(i=0;i<64;i++)
	{
		fgets(buffer1,13,cit1);
		fgets(buffer1,13,cit2);
		fgets(buffer1,13,cit3);
		fgets(buffer1,13,cit4);
		for(j=0;j<32;j++)
		{
			while((c=fgetc(pot))!=EOF)
			{
				if(c!='0')
					continue;
				fscanf(pot,"%7x",&n);			
				if(((i*32+j)*4)<=n)
				{
					if(((i*32+j)*4)<n)
					{
						of=ftell(pot);
						fseek(pot,of-100,SEEK_SET);
						break;
					}
					fscanf(pot,"%c%3s%3s%3s%3s",&c,buffer5,buffer6,buffer7,buffer8);
					fgets(buffer1,90,pot);
					break;
				}
				fgets(buffer1,90,pot);
			}
			fscanf(cit1,"%3s",buffer1);
			fscanf(cit2,"%3s",buffer2);
			fscanf(cit3,"%3s",buffer3);
			fscanf(cit4,"%3s",buffer4);	
			cnt=1;
			if(strncmp(buffer1,buffer5,3)||strncmp(buffer2,buffer6,3)||strncmp(buffer3,buffer7,3)||strncmp(buffer4,buffer8,3)&&(n==(i*32+j)))
				cnt=0;	
			fprintf(pis,"%04x:	%s%s%s%s%s%s",(i*32+j)*4,buffer1,buffer2,buffer3,buffer4,(cnt==1)?"    ":" !!!","\n");	
		}
		fgets(buffer1,90,cit1);
		fgets(buffer1,90,cit2);
		fgets(buffer1,90,cit3);
		fgets(buffer1,90,cit4);
	}
	puts("OK!\n");
	return 0;
}
