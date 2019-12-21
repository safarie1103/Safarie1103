
/**
 * Write a description of class IntDevidetest here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;
class IntDivideTest
{
  public static void main (String[] args) 
  { 
    Scanner scan = new Scanner( System.in );
 
    int dividend, divisor ;                      // int versions of input
    int quotient, remainder ;                    // results of "/" and "%"

    System.out.println("Enter the dividend:");   // read the dividend
    dividend = scan.nextInt();          

    System.out.println("Enter the divisor:");    // read the divisor
    divisor  = scan.nextInt();          

    quotient = dividend / divisor ;              // perform int math
    remainder= dividend % divisor ;

    System.out.println( dividend + " / " + divisor + " is " + quotient );
    System.out.println( dividend + " % " + divisor + " is " + remainder );
    System.out.println( quotient + " * " + divisor + 
        " + " + remainder + " is " + (quotient*divisor+remainder) );
  }
}