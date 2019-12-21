<?php
/******************************************
**** CUSTOM CODED SCRIPTS BY FLAPPI282 ****
******************************************/
if($_GET['do'] == "fm"){
?>
<html>
<head>
<title>File Manager</title>
<script>
</script>
</head>
<body>
<table width=100%>
<?php
$defdir = dirname(__FILE__) . "/";
$dir = $defdir;
if(isset($_GET['directory'])){
$dir = $_GET['directory'] . "/";
}
function cmp($a, $b){
    return strcmp($a["name"], $b["name"]);
}
$handle = @opendir($dir);
if($handle == false){
$dir = $defdir;
$handle = @opendir($dir);
echo "<tr><td colspan=\"2\" style=\"background-color: #FF0000; color: #FFFFFF;\">Error finding directory! Showing default</td></tr>";
}
$files = array();
$dirs = array();
while (($file = readdir($handle)) !== false){
if(is_file($dir . $file)){
$files[] = array("name" => $file, "size" => filesize($dir . $file));
} else {
if(basename($file) != "." && basename($file) != "..")
$dirs[] = array("name" => $file, "size" => filesize($dir . $file));
}
usort($files, "cmp");
usort($dirs, "cmp");
}
$updir = explode("/", $dir);
foreach ($updir as $key => $value){
  if ($value == "") {
    unset($updir[$key]);
  }
}
array_pop($updir);
var_dump($updir);
$updir = "/" . implode("/", $updir);
echo '<tr style="background-color: #CCC;"><td><a href="javascript: void(0)" onclick="window.location.href = \'' . $_SERVER['php_self'] . '?do=fm&directory=' . $updir . '/\'">&gt;&gt;Up one directory</a></td><td width=1>Directory</td></tr>' . "\n";
foreach($dirs as $file){
echo '<tr style="background-color: #CCC;"><td><a href="javascript: void(0)" onclick="window.location.href = \'' . $_SERVER['php_self'] . '?do=fm&directory=' . $dir . $file['name'] . '/\'">' . basename($file['name']) . '</a></td><td width=1>Directory</td></tr>' . "\n";
}
foreach($files as $file){
echo '<tr style="background-color: #DDD;"><td><a href="javascript: void(0)" onclick="top.location.href = \'' . $_SERVER['php_self'] . '?file=' . $dir . basename($file['name']) . '\'">' . basename($file['name']) . '</a></td><td width=1>' . round($file['size'] / 1024) . ' kb</td></tr>' . "\n";
}
closedir($handle);
?>
</table>
</body>
</html>
<?php
} else {
if(isset($_GET['save'])){
$content = stripslashes($_POST['content']);
$file = $_POST['file'];
$error = false;
$fh = fopen($file, 'w') or $error = "Could not write to file. Do you have the appropriate permissions?";
fwrite($fh, $content);
fclose($fh);
$error = "File Saved!";
$_REQUEST['file'] = $_POST['file'];
}
?>
<html>
<head>
<title>File Edit <?php if(isset($_REQUEST['file'])){ echo " - " . basename($_REQUEST['file']); } ?></title>
<style>
html, body {
padding: 0;
margin: 0;
}
.fm {
width: 910px;
height: 450px;
background-color: #EEE;
position: absolute;
left: 50px;
top: 50px;
z-index: 2;
display: none;
}

#edit {
z-index: 1;
padding: 0;
margin: 0;
}
</style>
<script>
var changed=0;
function leave(){
if(changed != 1){
return true;
} else {
return confirm("You have unsaved changes. Continue?");
}
}
</script>
</head>
<body onunload="return leave();">
<div id="filemanager" class="fm">
<span style="text-align:left">File Manager</span><a href="javascript: void(0);" onclick="document.getElementById('filemanager').style.display='none';" style="text-align: right;">(X)</a>
<iframe width=900 height=450 src="<?= $_SERVER['PHP_SELF']; ?>?do=fm">
You need to enable iFrames to use the file manager.
</iframe>
</div>
<div id="edit">
<table border="0" id="maintbl" cellpadding="0" cellspacing="0" width=100% height=100%>
<tr style="background: #CCC; height: 1px;"><td>
<form action="<?= $_SERVER['PHP_SELF']; ?>">
Open File: <input type="text" name="file" value="<?= $_REQUEST['file']; ?>" onclick=""/><input type="button" onclick="document.getElementById('filemanager').style.display='block';" value="Browse"/><input type="submit" value="Open" />
</form></td>
<td><span style="color: #FF0000;"><?= $error; ?></span>
<td>
<form action="<?= $_SERVER['PHP_SELF']; ?>?save=true" method="post">
<input type="hidden" name="file" value="<?= $_REQUEST['file']; ?>" />
<input type="submit" value="Save File" />
</td></tr>
<tr><td colspan=2>
<textarea onChange="changed=1" wrap="virtual" style="padding: 0 0 0 3px; margin:0; width:100%; border:0; height:100%;" name="content"><?php
if(isset($_REQUEST['file'])){
echo @htmlspecialchars(file_get_contents($_REQUEST['file'], TRUE));
}
?></textarea>
</td></tr>
</table></form>
</div>
</body>
</html>
<?php
}
?>