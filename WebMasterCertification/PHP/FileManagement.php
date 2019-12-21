<?php
// configuration
$file = 'http://webtrain.austincc.edu/~esafari/PHP/MyDocument.txt';


if(isset($_POST['Save']))
{
 file_put_contents($file, $_POST['text']);

}
else
{
 file_put_contents($file, $_POST['text']);

}
exit();
    $text = file_get_contents($file);
?>
