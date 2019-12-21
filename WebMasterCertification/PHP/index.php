<html>
<head>
<script>
function iFrameOn(){
	richTextField.document.designMode = 'On';
}
function iBold(){
	richTextField.document.execCommand('bold',false,null); 
}
function iUnderline(){
	richTextField.document.execCommand('underline',false,null);
}
function iItalic(){
	richTextField.document.execCommand('italic',false,null); 
}
function iFontSize(){
	var size = prompt('Enter a size 1 - 7', '');
	richTextField.document.execCommand('FontSize',false,size);
}
function iForeColor(){
	var color = prompt('Define a basic color or apply a hexadecimal color code for advanced colors:', '');
	richTextField.document.execCommand('ForeColor',false,color);
}
function iHorizontalRule(){
	richTextField.document.execCommand('inserthorizontalrule',false,null);
}
function iUnorderedList(){
	richTextField.document.execCommand("InsertOrderedList", false,"newOL");
}
function iOrderedList(){
	richTextField.document.execCommand("InsertUnorderedList", false,"newUL");
}
function iLink(){
	var linkURL = prompt("Enter the URL for this link:", "http://"); 
	richTextField.document.execCommand("CreateLink", false, linkURL);
}
function iUnLink(){
	richTextField.document.execCommand("Unlink", false, null);
}
function iImage(){
	var imgSrc = prompt('Enter image location', '');
    if(imgSrc != null){
        richTextField.document.execCommand('insertimage', false, imgSrc); 
    }
}
function submit_form(){
	var theForm = document.getElementById("myform");
	theForm.elements["myTextArea"].value = window.frames['richTextField'].document.body.innerHTML;
	theForm.submit();
}
</script>
</head>
<body onLoad="iFrameOn();">
<h2>My web application that intakes data from users</h2>
<form action="my_parse_file.php" name="myform" id="myform" method="post">
<p>Entry Title: <input name="title" id="title" type="text" size="80" maxlength="80" /></p>
<p>Entry Body:<br>
<div id="wysiwyg_cp" style="padding:8px; width:700px;">
  <input type="button" onClick="iBold()" value="B"> 
  <input type="button" onClick="iUnderline()" value="U">
  <input type="button" onClick="iItalic()" value="I">
  <input type="button" onClick="iFontSize()" value="Text Size">
  <input type="button" onClick="iForeColor()" value="Text Color">
  <input type="button" onClick="iHorizontalRule()" value="HR"> 
  <input type="button" onClick="iUnorderedList()" value="UL">
  <input type="button" onClick="iOrderedList()" value="OL">
  <input type="button" onClick="iLink()" value="Link">
  <input type="button" onClick="iUnLink()" value="UnLink">
  <input type="button" onClick="iImage()" value="Image">
</div>
<!-- Hide(but keep)your normal textarea and place in the iFrame replacement for it -->
<textarea style="display:none;" name="myTextArea" id="myTextArea" cols="100" rows="14"></textarea>
<iframe name="richTextField" id="richTextField" style="border:#000000 1px solid; width:700px; height:300px;"></iframe>
<!-- End replacing your normal textarea -->
</p>
<br /><br /><input name="myBtn" type="button" value="Submit Data" onClick="javascript:submit_form();"/>
</form>