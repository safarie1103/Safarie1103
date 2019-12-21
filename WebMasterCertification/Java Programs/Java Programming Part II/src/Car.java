
public class Car {
	  // instance variables
	  double startMiles;   // Stating odometer reading
	  double endMiles;     // Ending odometer reading
	  double gallons;      // Gallons of gas used between the readings


	Car(Double StartOdo,Double EndOdo, Double Gallons)
	{
		startMiles = StartOdo;
		endMiles = EndOdo;
		gallons = Gallons; 
	}
	  // methods
	double calculateMPG()
	{
	   return (endMiles - startMiles)/gallons;
	}
	boolean gasHog()
	{
		if (calculateMPG() < 15.0)
			return true;
		else
			return false;
	}
	boolean economyCar()
	{
		if (calculateMPG() > 30.0)
			return true;
		else
			return false;
		
	}
}
