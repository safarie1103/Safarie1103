
/**
 * Write a description of class SquareRoot here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;
class  OneOverB
{

  public static void main( String[] args ) 
  {
    final double smallValue = 1.0E-14 ;
    double B  ;               // the user enters a value for N
    double guess = 0.1 ;     // the same first guess works for any N

    // get the number from the user
    Scanner scan = new Scanner( System.in );
    System.out.print("Enter the number: "); 
    B = scan.nextDouble();
    if (B > 0.0)
    {
        while ( Math.abs( guess - B ) > smallValue )
        {      
           guess =  guess*(2-B*guess) ; // calculate a new guess
           System.out.println("guess" + " is " + guess ) ;
        }
        System.out.println("1/" + B + " is " + guess ) ;
    }
    else
    {
        System.out.println("Invalid value" ) ;
    }
  }

}