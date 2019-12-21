<?php

// configuration
$url = 'http://webtrain.austincc.edu/~esafari/PHP/editor.php';
$file = 'http://webtrain.austincc.edu/~esafari/PHP/MyDocument.txt';

// check if form has been submitted
if (isset($_POST['submitMe']))
{
    // save the text contents
    file_put_contents($file, $_POST['text']);

    ?>
<form action="<?=$_SERVER['PHP_SELF']?>" method="POST">
<input type="text" name="name"><br>
<input type="submit" value="submit" name="submitMe">
</form>
<?
}
}
else
{
$text = file_get_contents($file);
    
}
    exit();

?>
<!-- HTML form -->
<form action="" method="post">
<textarea name="text" cols="100" rows="14"><?php echo htmlspecialchars($text) ?></textarea>
<input type="submit" name="submitMe"/>
</form>