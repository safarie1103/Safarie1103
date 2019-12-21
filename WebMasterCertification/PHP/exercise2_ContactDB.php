<?php
$ContactList = array ( 
"Jane" => "28", 
"John" => "52", 
"Michel" => "18",
"Issac" => "72",
"Sara" => "34"   
); 
echo "<table border='1' cellpadding='5'>";
echo("<tr><th>Name</th><th>Age</th></tr>");

ksort($ContactList);

foreach($ContactList as $key => $value)
{
    echo ("<tr><td>$key</td><td>$value</td></tr>");
}
?>