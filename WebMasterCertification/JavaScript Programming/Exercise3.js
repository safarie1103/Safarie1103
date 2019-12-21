
function EvaluateAge(){
var repeat = false;
var NumberEvaluated = 0;
 do{
	var Age = 0;
	Age = prompt("Please Enter Your Age", "");
 
	if (Age == null || isNaN(Age)) {
		alert ("Please enter numbers!");
	}
	else if(Age == 0){
		alert ("You can't be Zero years old!:-)");
		}
	else if(Age < 0){
		alert ("Really? " + Age + " years old? Come on:-)");
	}
	else {
		NumberEvaluated += 1;
		if(Age <= 18){
			alert("Wow you are young at just " + Age + " years old!");
		}
		else if (Age > 18 && Age <=23){
			alert("You are getting on in years at " + Age + " years old!");
		}
		else{
			alert("Wow you are really old at " + Age + " years old!");
		}
	}
	
	
	repeat = confirm("Would you like to repeat?" );
	} while (repeat)
	if (NumberEvaluated > 1){
			alert("Evaluated " + NumberEvaluated + " Ages.");
		}
		else if (NumberEvaluated == 0){
			alert("Did not evaluate anything!");
		}
		else{
			alert("Evaluated " + NumberEvaluated + " Age.");
		}

}