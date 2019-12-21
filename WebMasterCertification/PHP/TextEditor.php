<body>
<h2>Text Editor</h2>
    <form action="/~esafari/PHP/FileManagement.php" method="POST">
<p><br>
<textarea name="text" cols="100" rows="14"><?php echo htmlspecialchars($text) ?></textarea>
</p>
<br /><br />
    <!--input name="myBtn" type="button" value="Save"/-->
    <input type="submit" value="Save"/>

</form>
</body>
