var ListHandler = new Object();
var CheckboxHandler = new Object();
var RadioHandler = new Object();

Array.prototype.in_array = function (value) {
  for (var i = 0; i < this.length; i++) {
	if (this[i] == value) {
		return true;
	}
  }

  return false;

};

RadioHandler.getCheckedValue = function(radio_name) {
  oRadio = document.forms[0].elements[radio_name];
  for(var i = 0; i < oRadio.length; i++) { 
	if(oRadio[i].checked) {
		return oRadio[i].value;
	}
  }

  return '';
};


ListHandler.getSelectedIndices = function (oList) {
  var indices = [];
  for(var i = 1; i < oList.options.length; i++) {
	  if(oList.options[i].selected == true) {
		  indices.push(i);
	  }
  }

  return indices;

};



ListHandler.getSelectedValues = function (oList) {
  var sValues = [];
  for(var i = 1; i < oList.options.length; i++) {
	  if(oList.options[i].selected == true) {
		sValues.push(oList.options[i].value);
	  }
  }

  return sValues;

};



ListHandler.getSelectedOptionsDisplayText = function (oList) {
  var sdValues = [];
  for(var i = 1; i < oList.options.length; i++) {
	  if(oList.options[i].selected == true) {
		sdValues.push(oList.options[i].text);
	  }
  }

  return sdValues;

};




ListHandler.getAllValues = function (oList) {
  var aValues = [];

  for(var i = 1; i < oList.options.length; i++) {
	  aValues.push(oList.options[i].value);
  }

  return aValues;

};




ListHandler.getAllOptionsDisplayText = function (oList) {
  var aValues = [];

  for(var i = 1; i < oList.options.length; i++) {
	  aValues.push(oList.options[i].text);
  }
  return aValues;

};




ListHandler.addOption = function (oList, optionName, optionValue) {
  var oOption = document.createElement("option");
  oOption.appendChild(document.createTextNode(optionName));
  oOption.setAttribute("value", optionValue);

  oList.appendChild(oOption);

};



ListHandler.removeOption = function (oList, index) {
  oList.remove(index);
};



CheckboxHandler.isChecked = function (checkboxObj) {
  return(checkboxObj.checked == true);
};


function trim(str) {
  return str.replace(/^\s+|\s+$/g, '');
}

function isEmpty(str) {
  str = trim(str);
  return ((str == null) || (str.length == 0));
}


function isDigit(c) {
  return ((c >= "0") && (c <= "9"));
}


function isInteger(str) {  
  for (var i = 0; i < str.length; i++) {
	var c = str.charAt(i);
	if (!isDigit(c)) {
		return false;
	}
  }

  return true;
}


function disableElement(obj) {
  obj.value = ' - N.A. - ';
  obj.disabled = true;
}

function enableElement(obj) {
  obj.value = '';
  obj.disabled = false;
}


function addToList(skillObj, experienceObj, expertiseObj, skillsetObj) {
  var skill = trim(skillObj.value);
  var experience = trim(experienceObj.value);
  
  if(isEmpty(skill)) {
	  alert("Please enter a skill.");
	  return;
  }
  
  if(isEmpty(experience) || !isInteger(experience)) {
	  alert("Please enter a valid integer representing your years of experience in the chosen skill");
	  return;
  }
  
  var expertise_chosen = false;

  for(var i = 1; i < expertiseObj.length; i++) {
	  if(expertiseObj.options[i].selected == true) {
		expertise_chosen = true;
		var optionName = skill + ': ' + experience + ' years experience - ' + expertiseObj.options[i].text + ' level skills'; 
		
		var optionValue = expertiseObj.options.length;

		ListHandler.addOption(skillsetObj, optionName, optionValue);
		expertiseObj.options[i].selected = false;
		expertiseObj.selectedIndex = -1;
		skillObj.value = '';
		experienceObj.value = '';
		
		break;
	  }
  }

  if(!expertise_chosen) {
	  alert("Please select an expertise level from the drop down list");
  }

}


function removeFromList(skillsetObj) {
  for(var i = 1; i < skillsetObj.length; i++) {
	if(skillsetObj.options[i].selected == true) {
		ListHandler.removeOption(skillsetObj, i);		
	}
  }
}


function handleJobExpOption(jobExpObj, currentSalaryObj) {
  if (jobExpObj.options[jobExpObj.selectedIndex].value == 1) {
	  disableElement(currentSalaryObj);
  } else {
	enableElement(currentSalaryObj);
  }
}


function checkEmail(email)
{	
	
  var pattern=/^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+/;
  
  if(pattern.test(email)) {         
	return true;
  } else {   
	return false; 
  }

}


function getFormValues(oForm, skip_elements) {
   
  var elements = oForm.elements; 
  var data = [];
  var element_value = null;

  for(var i = 0; i < elements.length; i++) {
     
	var field_type = elements[i].type.toLowerCase();
	var element_name = elements[i].getAttribute("name");
	
	if(!skip_elements.length ||  !skip_elements.in_array(element_name)) {
	
	switch(field_type) {
	
		case "text": 
		case "password": 
		case "textarea":
		case "hidden":	
			
			element_value = elements[i].value;
			data.push(element_name + ': ' + element_value);
			break;
		
		case "checkbox":
			
			element_value = CheckboxHandler.isChecked(elements[i]);
			data.push(element_name + ': ' + element_value);
			break;


		case "select-one":

			var ind = elements[i].selectedIndex;
			if(ind > 0) { 
				element_value = elements[i].options[ind].text;
			} else {
				element_value = '';
			}
			data.push(element_name + ': ' + element_value);
			break;

		case "select-multiple":

			var elems = ListHandler.getSelectedOptionsDisplayText(elements[i]);
			element_value = elems.join('\n');
			data.push(element_name + ': ' + element_value);
			break;
		
		default: 
			break;
	}

        }
  
  }
		
  return data; 

}

function processFormData(oForm) {
	var skip_elements = ['skillset', 'skill', 'experience', 'expertise', 'work_abroad'];
	
	var data = getFormValues(oForm, skip_elements);
	
	var skillset_arr = ListHandler.getAllOptionsDisplayText(oForm.skillset);
	var work_abroad_option = RadioHandler.getCheckedValue('work_abroad');
	
	data.push('skillset_list: ' + skillset_arr.join('\n'));
	data.push('work_abroad: ' + work_abroad_option);
	
	alert(data.join('\n--------------------\n'));
	 
}
