import java.util.Scanner;
import Car;
public class MilesPerGallon {
	public static void main( String[] args ) 
	  {
	    Scanner scan = new Scanner(System.in);

	    double startMiles, endMiles, gallons;

	    System.out.print("Enter first reading: " ); 
	    startMiles = scan.nextDouble();

	    System.out.print("Enter second reading: " ); 
	    endMiles = scan.nextDouble();

	    System.out.print("Enter gallons: " ); 
	    gallons  = scan.nextDouble();
	    
	    Car car = new Car(startMiles, endMiles,gallons);
	    System.out.println( "Miles per gallon is "  + car.calculateMPG() );
	    if (car.gasHog())
		    System.out.println( "Gas Hog");
	    else if(car.economyCar())
		    System.out.println( "Economy car!");

	  }

}
