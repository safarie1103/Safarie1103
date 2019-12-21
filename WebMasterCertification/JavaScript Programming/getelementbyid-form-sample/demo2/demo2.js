function processFormData() 
{

  var inputs = document.getElementsByTagName('input');

  var message = 'The form has the following input elements: \n\n';

  for (var i = 0; i < inputs.length; i++) {
	message += inputs[i].tagName + " element with 'type' attribute = '" + inputs[i].getAttribute('type') + "' and 'name' attribute = '" + inputs[i].getAttribute('name') + "'\n";	
  }

  alert(message);
  
}
