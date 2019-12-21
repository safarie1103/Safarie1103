function processFormData() 
{

  var inputs = document.getElementsByTagName('input');
  
  var message = "The form has the following input elements with the 'type' attribute = 'text': \n\n";

  for (var i=0; i < inputs.length; i++) {
	if (inputs[i].getAttribute('type') == 'text') {
		message += inputs[i].tagName + " element with the 'name' attribute = '" + inputs[i].getAttribute('name') + "'\n";
	}
  }

  alert(message);
  
}
