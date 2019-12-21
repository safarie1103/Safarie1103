
public class Car {

	double MPG;
	Car(Double StartOdo,Double EndOdo, Double Gallons)
	{
		MPG = (EndOdo - StartOdo)/Gallons;
	}
}
