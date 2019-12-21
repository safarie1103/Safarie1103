
/**
 * Write a description of class MillionDollarYears here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;

class DollarsAfterForty
{

  public static void main( String[] args )  
  {
    double dollars = 1000.00 ;
    double rate;
    int    year =  1 ;     

    // get the interest rate from the user
    Scanner scan = new Scanner( System.in );
    System.out.print("Enter the interest rate in percent: "); 
    rate = scan.nextDouble()/100.0 ;
 
    while (  year <= 40 )
    {
      // add another year's interest
      dollars =  dollars + dollars*rate ; 

      // add in this year's contribution
      dollars = dollars + 1000 ;

      year    =  year + 1 ;
    }

    System.out.println("After 40 years at " + rate*100 
      + " percent interest you will have " + dollars + " dollars" );
  }

}