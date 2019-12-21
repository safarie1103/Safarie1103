
/**
 * Write a description of class StandardDeviation here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
import java.util.Scanner;
public class StandardDeviation
{
    // instance variables - replace the example below with your own
    
    public static void main (String[] args)  
    {  
        int NumberOfValues = 0;
        int Count = 1;
        double Value = 0.0;
        double ValueSquared = 0.0;
        double SD = 0.0;
        Scanner scan = new Scanner(System.in);
        System.out.print("Please Enter number of values: ");
        NumberOfValues = scan.nextInt();
        
        while(Count <= NumberOfValues)
        {
            
             
            System.out.print("Please Enter X(" + Count++ + "):");
            
            double EnteredValue = scan.nextDouble();
            
            Value += EnteredValue;
            ValueSquared += Math.pow(EnteredValue,2);
            System.out.print("Value = " + Value + "\n");
            System.out.print("ValueSquared = " + ValueSquared + "\n");
            
            
        }
        
        
        System.out.print("Sum = " + Value + "\n");
        
        SD= Math.sqrt(ValueSquared/NumberOfValues - Math.pow(Value/NumberOfValues,2));
        
        System.out.print("Standard Deviation is :" + SD);
    }
    
}
