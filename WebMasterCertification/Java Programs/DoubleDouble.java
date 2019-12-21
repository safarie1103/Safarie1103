
/**
 * Write a description of class DoubleDouble here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;

class DoubleDouble
{
  public static void main (String[] args)
  {
    double value;
    Scanner scan = new Scanner( System.in );
 
    System.out.print("Enter a double:");
    value = scan.nextDouble();

    System.out.println("value: " + value +" twice value: " + 2.0*value );
  }
}