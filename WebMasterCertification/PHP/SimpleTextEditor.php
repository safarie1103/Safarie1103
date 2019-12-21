<h2>content1</h2>
<? 
if($_POST['Submit']){ 
$open = fopen("~/esafari/PHP/content1.txt","w+"); 
$text = $_POST['update']; 
fwrite($open, $text); 
fclose($open); 
echo "File updated.<br />";  
echo "File:<br />"; 
$file = file("~/esafari/PHP/content1.txt"); 
foreach($file as $text) { 
echo $text."<br />"; 
} 
}else{ 
$file = file("~/esafari/PHP/content1.txt"); 
echo "<form action=\"".$PHP_SELF."\" method=\"post\">"; 
echo "<textarea Name=\"update\" cols=\"50\" rows=\"10\">"; 
foreach($file as $text) { 
echo $text; 
}  
echo "</textarea>"; 
echo "<input name=\"Submit\" type=\"submit\" value=\"Update\" />\n 
</form>"; 
} 
?> 
<br />
<h2>content2</h2>
<? 
if($_POST['Submit2']){ 
$open = fopen("~/esafari/PHP/content2.txt","w+"); 
$text = $_POST['update2']; 
fwrite($open, $text); 
fclose($open); 
echo "File updated.<br />";  
echo "File:<br />"; 
$file = file("~/esafari/PHP/content2.txt"); 
foreach($file as $text) { 
echo $text."<br />"; 
} 
}else{ 
$file = file("~/esafari/PHP/content2.txt"); 
echo "<form action=\"".$PHP_SELF."\" method=\"post\">"; 
echo "<textarea Name=\"update2\" cols=\"50\" rows=\"10\">"; 
foreach($file as $text) { 
echo $text; 
}  
echo "</textarea>"; 
echo "<input name=\"Submit2\" type=\"submit\" value=\"Update\" />\n 
</form>"; 
} 
?> 
