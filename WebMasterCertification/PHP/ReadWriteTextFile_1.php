//FORM CODE:

<html>
<head></head>
<body>

<form method="post" action="save.php">

	<input type=file name=browse style="display: none;"/>
     
<br/>
<br/>

<textarea rows="40" cols="100">

<?php
$fn = "LookAtMe.txt";
print htmlspecialchars(implode("",file($fn)));
?>

</textarea>
<br/>
<br/>
<input type=text name=file/>
<input type=button  onClick="browse.click();file.value=browse.value;browse.disabled=true" value="Open File" /> 
<input type="submit" value="Save"> 
     
</form>
</body>
</html>

//PHP Code for the save button:
<?php
#save.php
header("Content-type: text/plain");
header("Content-Disposition: attachment; filename=MyDocument.txt");
echo $_REQUEST['user_data'];
?>