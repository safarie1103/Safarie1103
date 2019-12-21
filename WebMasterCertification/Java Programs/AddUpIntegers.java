
/**
 * Write a description of class AddUpIntegers here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import  java.util.Scanner;

// User enters a value N
// Add up odd integers,  
// even  integers, and all integers 1 to N
//
class AddUpIntegers
{
  public static void main (String[] args ) 
  {
    Scanner scan = new Scanner( System.in );
    int N, sumAll = 0, sumEven = 0, sumOdd = 0;

    System.out.print( "Enter limit value: " );
    N = scan.nextInt();

    int count = 1;
    while (  count <= N )    
    {
      sumAll = sumAll + count ;
      
      if ( count % 2 == 0  )
        sumEven = sumEven + count ;

      else
        sumOdd  = sumOdd  + count ;

      count = count + 1 ;
    }

    System.out.print  ( "Sum of all : " + sumAll  );
    System.out.print  ( "\tSum of even: " + sumEven );
    System.out.println( "\tSum of odd : " + sumOdd  );
  }
}