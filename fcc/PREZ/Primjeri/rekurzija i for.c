#define MAX 10
int funkcija(int n)
{
	if (n <= 0)
		return 0;
	return (n + funkcija( n - 1 ));
}

int main()
{
	int i,j,suma=0;
	for(i=0;i<MAX;i++)
		for(j=i;j<MAX;j++)
			suma+=funkcija(i+j);
	return 0;
}
