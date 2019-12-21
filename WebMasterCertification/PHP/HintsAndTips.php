<?php
//If we submitted the form
if(isset($_POST['submitMe']))
{
echo("Hello, " . $_POST['name'] . ", we submitted your form!");
}
//If we haven't submitted the form
else
{
?>
<form action="<?=$_SERVER['PHP_SELF']?>" method="POST">
<input type="text" name="name"><br>
<input type="submit" value="submit" name="submitMe">
</form>
<?
}
?>