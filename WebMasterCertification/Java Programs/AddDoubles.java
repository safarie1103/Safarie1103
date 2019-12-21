
/**
 * Write a description of class AddDoubles here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;

class AddDoubles
{
  public static void main (String[] args)
  {
    double first, second, sum;
    Scanner scan = new Scanner( System.in );
 
    // Read in the first double
    System.out.print("Enter the first double: ");
    first = scan.nextDouble();

    // Read in the second double
    System.out.print("Enter the second double: ");
    second = scan.nextDouble();

    // Compute the sum and write it out
    sum = first + second;
    System.out.print("Sum: " + sum );
  }
}