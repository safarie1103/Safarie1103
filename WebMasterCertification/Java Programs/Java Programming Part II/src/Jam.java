
public class Jam {
String fruitType;
String dateCanned;
int amountInJar;

	Jam(String  type,String date,int amount)
	{
		fruitType = type;
		dateCanned = date;
		amountInJar = amount;
	}
	boolean isEmpty()
	{
		if (amountInJar == 0)
			return true;
		else
			return false;
	}
	private boolean empty()
	{
		return ( amountInJar == 0);
	}
	public void spread(int amount)
	{
		if ( !empty() )
	     {
	         if ( amount <= amountInJar )
	         {
	             System.out.println("Spreading " + amount + " fluid ounces of " + fruitType );
	                 

	             amountInJar = amountInJar - amount ;
	    
	         }
	         else
	         {
	             System.out.println("Spreading " + amountInJar + " fluid ounces of "
	                 + fruitType );

	             amountInJar = 0 ;
	         }
	     }
	     else
	         System.out.println("No jam in the Jar!");
	}
	public void print()
	{
		System.out.println(fruitType + " " + dateCanned + " " + amountInJar + " fl oz.");
	}
	
}
