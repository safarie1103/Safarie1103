
/**
 * Write a description of class EvalPoly here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;

class EvalPoly
{
  public static void main (String[] args )  
  {
    Scanner scan = new Scanner ( System.in );

    double x;                      // a value to use with the polynomial
    double result;                 // result of evaluating the polynomial at x
    String response = "y";         // "y" or "n"

    while ( response.equals( "y" ) )    
    {
       // Get a value for x.
       System.out.println("Enter a value for x:")  ;
       x = scan.nextDouble();

       // Evaluate the polynomial.
       result =7*x*x*x - 3*x*x + 4*x - 12;

       // Print out the result.
       System.out.println("The value of the polynomial at x = " +
           x + " is: " + result + "\n"  ) ;

       // Ask the user if the program should continue.
       System.out.println("continue (y or n)?");
       response = scan.next();      
    }

  }
}