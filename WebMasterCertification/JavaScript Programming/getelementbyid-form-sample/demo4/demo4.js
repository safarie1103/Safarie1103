function processFormData(name_attr) 
{

  var mail_format_elements = document.getElementsByName(name_attr);
  
  var message = "The form has the following elements with the 'name' attribute = '" + name_attr + "': \n\n";
 
  for (i=0; i<mail_format_elements.length; i++) {
	  message += mail_format_elements[i].tagName + ' element with "id" attribute = "' + mail_format_elements[i].getAttribute("id") + '"\n';
  }
   
  alert(message);
  
}
