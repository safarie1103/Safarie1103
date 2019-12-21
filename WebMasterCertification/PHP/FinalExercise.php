<?php
error_reporting(E_ALL); ini_set("display_errors", "On");

$file = "MyDocument.txt";


if (isset($_POST['save']))
{
    if (!file_put_contents($file, $_POST['text']))
    {
            $text = "failed writting to file";
    }

}
if (isset($_POST['exit']))
    exit();

$text = file_get_contents($file);
?>

<!-- HTML form -->
<body>
<h2>Text Editor</h2>

<form action="" method="post">
<textarea name="text" cols="100" rows="14"><?php echo stripslashes($text) ?></textarea>
<br/>
<input type="submit" name="save"  value="Save"/>
<input type="submit" name="exit"  value="Exit"/>
</form>
</body>