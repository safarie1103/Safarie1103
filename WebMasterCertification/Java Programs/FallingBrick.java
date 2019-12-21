
/**
 * Write a description of class FallingBrick here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;

// User picks ending value for time, t.
// The program calculates and prints 
// the distance the brick has fallen for each t.
//
class FallingBrick
{
  public static void main (String[] args )  
  {
    final  double G = 9.80665;  // constant of gravitational acceleration
    int     limit;            // time in seconds; ending value of time
    double distance;            // the distance the brick has fallen
    Scanner scan = new Scanner( System.in );

    System.out.print( "Enter limit value: " );
    limit = scan.nextInt();

    // Print a table heading
    System.out.println( "seconds\tDistance"  );  // '\t' is tab
    System.out.println( "-------\t--------"  ); 

    int tenths = 0 ;
    while (  tenths <= limit*10 )    
    {
      double t = tenths/10.0 ;        // be sure to include "point zero"
      distance = (G * t * t)/2 ;
      System.out.println( t + "\t" + distance );
    
      tenths = tenths + 1 ;
    }
  }
}