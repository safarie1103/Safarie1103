
/**
 * Write a description of class SquareRoot here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;
class  SquareRoot
{

  public static void main( String[] args ) 
  {
    final double smallValue = 1.0E-14 ;
    double N  ;               // the user enters a value for N
    double guess = 1.00 ;     // the same first guess works for any N

    // get the number from the user
    Scanner scan = new Scanner( System.in );
    System.out.print("Enter the number: "); 
    N = scan.nextDouble();
    if (N >= 0.0)
    {
        while ( Math.abs( N/(guess*guess) - 1.0 ) > smallValue )
        {      
           guess =  N/(2*guess) + guess/2 ; // calculate a new guess
        }
        System.out.println("The square root of " + N + " is " + guess ) ;
    }
    else
    {
        System.out.println("Invalid value" ) ;
    }
  }

}