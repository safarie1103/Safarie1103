import java.util.Scanner;

public class ScannerDemo {
    public static void main(String[] arguments){
        Scanner input = new Scanner(System.in);

        String username;
        double age;
        String gender;
        String marital_status;
        int telephone_number;

        // Allows a person to enter his/her name   
        Scanner one = new Scanner(System.in);
        System.out.println("Enter Name:" );  
        username = one.next();
        System.out.println("Name accepted " + username);

        // Allows a person to enter his/her age   
        Scanner two = new Scanner(System.in);
        System.out.println("Enter Age:" );  
        age = two.nextDouble();
        System.out.println("Age accepted " + age);

        // Allows a person to enter his/her gender  
        Scanner three = new Scanner(System.in);
        System.out.println("Enter Gender:" );  
        gender = three.next();
        System.out.println("Gender accepted " + gender);

        // Allows a person to enter his/her marital status
        Scanner four = new Scanner(System.in);
        System.out.println("Enter Marital status:" );  
        marital_status = four.next();
        System.out.println("Marital status accepted " + marital_status);

        // Allows a person to enter his/her telephone number
        Scanner five = new Scanner(System.in);
        System.out.println("Enter Telephone number:" );  
        telephone_number = five.nextInt();
        System.out.println("Telephone number accepted " + telephone_number);
    }
}