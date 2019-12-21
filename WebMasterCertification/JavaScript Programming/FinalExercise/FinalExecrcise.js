function newWindow() {
wnd=window.open("Exercise3.htm","displayWindow","toolbar=no,scrollbars=yes,width=1000,height=300");

}


function trim(str) {
  return str.replace(/^\s+|\s+$/g, '');
}
function isEmpty(str) {
  str = trim(str);
  return ((str == null) || (str.length == 0));
}

function toggleFormVisibility()
{
  var frm_element = document.getElementById('PersonalInfo_frm'); 
  var sub_link_element = document.getElementById('EnterInfoLink');
  var nosub_link_element = document.getElementById('NoThanks');

  var vis = frm_element.style;
  
  if(vis.display=='' || vis.display=='none') {
	  vis.display = 'block';
	  sub_link_element.style.display='none';
	  nosub_link_element.style.display='';
  } else {
	  vis.display = 'none';
	  sub_link_element.style.display='block';
	  nosub_link_element.style.display='none';
	  document.getElementById("PersonalInfo_frm").reset();
	  delWindow();
  }
}
var wnd;
function delWindow() {
	if (wnd != undefined) {
		wnd.close();
	}
}

function processFormData() 
{
	var missingData = [];
	missingData.push("Missing data:");

	var FistName_element = document.getElementById('txt_FirstName');
	var LastName_element = document.getElementById('txt_LastName');
	var StreetAddress_element = document.getElementById('txt_StreetAddress');
	var City_element = document.getElementById('txt_City');
	var ZipCode_element = document.getElementById('txt_ZipCode');
	var State_element = document.getElementById('txt_State');
	var Country_element = document.getElementById('txt_Country');
	var Occupation_element = document.getElementById('txt_Occupation');
	var EducationLevel_element = document.getElementById('select_EducationLevel');
	var HobbiesAndInterests_element = document.getElementById('txt_HobbiesAndInterests');

	var FirstName = (isEmpty(FistName_element.value)? missingData.push("FirstName") : trim(FistName_element.value));
	var LastName = (isEmpty(LastName_element.value) ? missingData.push("LastName"): trim(LastName_element.value));
	var StreetAddress = (isEmpty(StreetAddress_element.value) ? missingData.push("StreetAddress") : trim(StreetAddress_element.value));
	var City = (isEmpty(City_element.value) ? missingData.push("City"): trim(City_element.value));
	var ZipCode = (isEmpty(ZipCode_element.value) || isNaN(trim(ZipCode_element.value)) || trim(ZipCode_element.value).length != 5 ? missingData.push("ZipCode"): trim(ZipCode_element.value));
	var State = (isEmpty(State_element.value) ? missingData.push("State") : trim(State_element.value));
	var Country = (isEmpty(Country_element.value) ? missingData.push("Country"): trim(Country_element.value));
	var Occupation = (isEmpty(Occupation_element.value) ? missingData.push("Occupation") : trim(Occupation_element.value));
	var EducationLevel = (isEmpty(EducationLevel_element.value) || trim(EducationLevel_element.value) == "--Select one--" ? missingData.push("EducationLevel") : trim(EducationLevel_element.value));
	var HobbiesAndInterests = (isEmpty(HobbiesAndInterests_element.value) ? missingData.push("HobbiesAndInterests") : trim(HobbiesAndInterests_element.value));
	
	if(missingData.length > 1){		
		alert(missingData.join('\n\n'));
	}
	else{
		delWindow();
		wnd=window.open("","","toolbar=no,scrollbars=yes,width=undefined,height=undefined",true);
		wnd.document.write("<style> p {font:italic; color:blue;font-size:20px; background-color: yellow ; padding: 10px; border: black 2px solid} body {background-color:powderblue} </style>");
		wnd.document.write("<h1>First Name: " + FirstName + "</h1>");
		wnd.document.write("<h1>Last Name: " + LastName + "</h1>");
		wnd.document.write("<h2>" + StreetAddress + "</h2>");
		wnd.document.write("<h2>" + City + ", " + State.toUpperCase() + "   " + ZipCode + "</h2>");
		wnd.document.write("<h2>" + Country.toUpperCase() + "</h2>");
		wnd.document.write("<h2>Current Occupation:" + Occupation + "</h2>");
		wnd.document.write("<h2>Education Level:" + EducationLevel + "</h2>");
		wnd.document.write("<h2>Hobbies and Interests:</h2>");
		wnd.document.write("<p>" + HobbiesAndInterests + "</p>");			
	} 
}