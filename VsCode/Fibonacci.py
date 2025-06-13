import turtle
import math
from pyvirtualdisplay import Display

# Start a virtual display
display = Display(visible=0, size=(800, 600))
display.start()

# Rest of your code remains the same

# Create a turtle object
my_turtle = turtle.Turtle()

# Move the turtle forward
my_turtle.forward(100)

# Turn the turtle left
my_turtle.left(90)

# Move the turtle forward again
my_turtle.forward(100)

# Keep the window open until manually closed
#turtle.done()


# Close the virtual display after plotting
display.stop()