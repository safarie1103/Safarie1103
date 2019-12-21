
import java.awt.*;
import java.util.*;
class HelloObject
{
  // method definition
	String Message;
	HelloObject(String Msg)
	{
		Message = Msg;
	}
  void speak()
  {
    System.out.println(Message);
  }
}
class HelloTester
{
  public static void main (String arg[])
  {
	System.out.println("Enter Greeting: ");
	Scanner scanner = new Scanner(System.in);
	String Greeting = scanner.nextLine();
    HelloObject anObject = new HelloObject(Greeting);
    anObject.speak();
  }
}
