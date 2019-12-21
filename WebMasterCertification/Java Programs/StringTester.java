
/**
 * Write a description of class StringTester here.
 * 
 * @author (your name) 
 * @version (a version number or a date)
 */
public class StringTester
{

  public static void main ( String[] args )
  {
    String str1;   // str1 is a variable that may refer to an object. 
                   // The object does not exist unit the "new" is executed.
    int    len;    // len is a primitive variable of type int

    str1 = new String("Random Jottings");  // create an object of type String
                                                           
    len  = str1.length();  // invoke the object's method length()

    System.out.println("The string is " + len + " characters long");
  }
}