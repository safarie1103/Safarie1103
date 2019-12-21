import java.util.Scanner;

public class Furniture2Test  {

    public static void main(String[] args) {

        wood();

    } // end main

    public static void wood() {

        int choice;

        int pine = 1;
        int oak = 2;
        int mahogany = 3;

        int pineCost = 100;
        int oakCost = 225;
        int mahoganyCost = 310;

        Scanner keyboard = new Scanner(System.in);

        System.out.println("What type of table would you like?");
        System.out.println("1. pine");
        System.out.println("2. oak");
        System.out.println("3. mahogany");

        choice = keyboard.nextInt();

        if (choice == 1) {
            choice = pineCost;
        } else if (choice == 2) {
            choice = oakCost;
        } else if (choice == 3) {
            choice = mahoganyCost;
        } else if (choice > 3 || choice < 1) {
            System.out.println("Try again.");
            choice = -1;
            wood();
        }

        System.out.println("That will be $" + choice + ".");

        size(choice);

    } // end wood

    public static void size(int choice) {

        int sizeChoice;
        int large = 35;

        Scanner keyboard = new Scanner(System.in);

        System.out.println("What size will that be?");
        System.out.println("1. large");
        System.out.println("2. small");

        sizeChoice = keyboard.nextInt();

        if (sizeChoice == 1)
            System.out.println("That will be $" + (choice + large) + ".");
        else if (sizeChoice == 2)
            System.out.println("That will be $" + choice);
        else
            System.out.println("Please, enter either a 1 or a 2.");

    } // end size

}