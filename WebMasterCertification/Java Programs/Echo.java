
/**
 * Write a description of class Echo here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;

     class Echo
    {
      public static void main (String[] args) 
      {
        String inData;
        Scanner scan = new Scanner( System.in );
    
        System.out.println("Enter the data:");
        inData = scan.nextLine();
    
        System.out.println("You entered:" + inData );
      }
    
       public Echo()
       {
           // initialise instance variables
         
       }
    
    }  
