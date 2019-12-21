<!DOCTYPE HTML>
<!-- 
Author: Melissa Reeve: 
5063 PHP 
Home directory: public_html/~mreeve/5063_PHP/Final_Project/final_form_editor_2.php
Created March 6, 2017: 
Notes: Section 3 - This HTML form displays and allows editing of a .txt file via PHP -->
<html>
<html lang="en">

<head>
    <meta charset="utf-8">
	<meta name="ROBOTS" content="noindex,nofollow">
	<meta name="keywords" content="Form, final, text, editing, PHP, 5063">
    <link href="https://fonts.googleapis.com/css?family=Rubik:400,700" rel="stylesheet">
	<title>Form with PHP text editing program</title>
    <style>
        html{margin: 3em;}
        Body{
            font-size: 1em, 400;
            font-family: 'Rubik', sans-serif;
        }
        html {background-color: #90a1c1;}
        h1{color: rgb(0, 0, 138);}
        textarea {
            padding: 2em;
            width: auto;
            font-family: 'Rubik', sans-serif;
            border: solid, 4px, rgb(0, 0, 138);
        }
    </style>
</head>
    
<body>
     <!-- 
    PROJECT: The user is shown a form with an editable text presented in a textarea. Once the user makes edits the text and cliskd submit, the edited text is written to the same text file. Finally a message is displayed informing the user that their edits were saved.
    Text file URL = ../~mreeve/5063_PHP/Final_Project/BirthdayText.txt
    PHP file URL = ../~mreeve/5063_PHP/Final_Project/Message_Editor.txt
     -->
                <!-- The HTML form -->
                <h1>Please create a personalized message to acompany your gift.</h1>
                <p>A suggestion is provided for convenience. Modify as desired.</p>
                <form method="post" action="/~mreeve/5063_PHP/Final_Project/Message_Editor.php">   
                <textarea name="new_text" rows="10" cols="60">Happy 43rd Birthday George!

Cherish your memories and create magical moments as you celebrate a day made just for you.

Love, 
Scooby Doo</textarea>
                <br><br>
                <input type="submit" name="submitted" value="Submit Final">
                </form>
  
</body>
</html>
