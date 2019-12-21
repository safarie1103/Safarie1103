
<!doctype html>
<html><head lang="en">
<title>Array Example</title>
</head>

<body>
Hello. Here's what we got:
    <br>
    
<?php
//We initialize the array using the array() function. 
//Note that for readability one can spread the argument over several lines.
$flower_shop = array ( 
"rose" => "5.00", 
"daisy" => "4.00", 
"orchid" => "2.00", 
); 
//let's print out the headers to our table
echo "<table border='1' cellpadding='5'>";
echo("<tr><th>Flower</th><th>Price</th></tr>");
//Now we start the foreach loop using the variable $flower to hold our key 
//and $price to hold our cost.
foreach($flower_shop as $Flower=>$Price)
{
echo ("<tr><td>$Flower </td><td>$Price</td></tr> ");
//print the values into a table cell for each iteration
}
//finally close the table
echo ("</table>");
?> 

Goodbye!
</body>
</html>
