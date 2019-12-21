
/**
 * Write a description of class Factorial here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import  java.util.Scanner;

// User enters integer N.  
// The program calculates N factorial.
//
class Factorial
{
  public static void main (String[] args ) 
  {
    Scanner scan = new Scanner( System.in );
    long N, fact = 1; 

    System.out.print( "Enter N: " );
    N = scan.nextLong();

    if ( N >= 0 && N <= 20 )
    {
      while ( N > 1 )    
      {
        fact = fact * N;
        N    = N - 1;
      }
      System.out.println( "factorial is " + fact );
    }
    else
    {
      System.out.println("N must be between 0 and 20");
      System.out.println("Factorial for N less than 0 is not defined.");
      System.out.println("Factorial for N greater than 20 causes overflow.");
    }
  }
}