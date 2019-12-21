
/**
 * Write a description of class SquareRoot here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;
class  DrugPotency
{

  public static void main( String[] args ) 
  {
    final double smallValue = 1.0E-14 ;
    double Potency = 100.0, PotencyDegradation = smallValue ;  
    int Month = 0;
  
        while ( PotencyDegradation >= smallValue)
        {      
           System.out.println("Month: " + Month + "\t" + "Potency: " + Potency  + "\t" + "potency degradation: " + PotencyDegradation);
           Potency =  Potency - Potency*.04;
           PotencyDegradation = Potency - 50.0;
           Month += 1;
        }
           System.out.println("Month: " + Month + "\t" + "Potency: " + Potency  + "\t" + "potency degradation: " + PotencyDegradation + "  DISCARDED" ); 
  }
}