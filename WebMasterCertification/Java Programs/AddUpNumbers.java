
/**
 * Write a description of class AddUpNumbers here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;

// Add up integers entered by the user.
// After the last integer, the user enters a 0.
//
import java.io.*;

// Add up integers entered by the user.
// After the last integer, the user enters a 0.
//
class AddUpNumbers
{
  public static void main (String[] args )  
  {
    Scanner scan = new Scanner( System.in );
    String inputData;
    String suffix;
    int value;             // integer entered by the user
    int count = 0;         // how many integers added so far
    int sum   = 0;         // initialize the sum

    // get the first value
    System.out.println( "Enter first integer (enter 0 to quit):" );
    value = scan.nextInt();

    while ( value != 0 )    
    {
      //add value to sum
      sum   = sum + value;     // add current value to the sum
      count = count + 1;       // one more integer added

      // prompt for the next value 
      if ( count+1  == 2  )
        suffix = "nd"; 
      else
        if ( count+1 == 3  )
          suffix = "rd";
        else
          suffix = "th"; 
          
      System.out.println( "Enter the " + (count+1) + suffix + 
                          " integer (enter 0 to quit):" );

      //get the next value from the user 
      value = scan.nextInt();
      
    }

    System.out.println( "Sum of the integers: " + sum );
  }
}