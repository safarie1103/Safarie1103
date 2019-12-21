
/**
 * Write a description of class RestaurantBill here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;

class RestaurantBill
{
  public static void main (String[] args)  

  {  
    double basicCost,tipPercent;

    System.out.print("Enter the basic cost: ");

    Scanner scan = new Scanner( System.in );

    basicCost = scan.nextDouble();
    
    System.out.print("Enter the tipPercent: ");
    tipPercent = scan.nextDouble();

    System.out.println("basic cost: " +  basicCost + " total cost: " + (basicCost + basicCost*0.06 + basicCost*tipPercent));
  }
}