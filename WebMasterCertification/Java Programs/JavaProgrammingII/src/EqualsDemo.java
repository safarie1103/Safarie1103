import java.awt.*;
class EqualsDemo
{
  public static void main ( String arg[] )
  {
    Point pointA = new Point( 7, 99 );    // first Point
    Point pointB = new Point( 7, 99 );    // second Point with equivalent data

    if ( pointA.equals( pointB ) )
      System.out.println( "The two objects contain the same data: " + pointA );
    else
      System.out.println( "The two objects are not equivalent: " + pointA + 
          " differs from" + pointB);     

  }
}
