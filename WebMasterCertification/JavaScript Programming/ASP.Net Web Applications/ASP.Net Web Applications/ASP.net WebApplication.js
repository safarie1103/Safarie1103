function JSClock() {
    var time = new Date();
    var hour = time.getHours();
    var minute = time.getMinutes();
    var second = time.getSeconds();
    var temp = "" + ((hour > 12) ? hour - 12 : hour);
    if (hour == 0)
        temp = "12";
    temp += ((minute < 10) ? ":0" : ":") + minute;
    temp += ((second < 10) ? ":0" : ":") + second;
    temp += (hour >= 12) ? " P.M." : " A.M.";
    return temp;
}

function calc(numA, numB) {
    return numA + numB;
}

// JScript source code
DowhileExample()
{
var a = ['a', 'b', 'c', 'd', 'a', 'b'];
var correct = false;
 var x = prompt("Enter 1st number:", "");
 var y = prompt("Enter 2nd number:", "");
 var z = prompt("Enter 1st number multiplied by 2nd number:", "");
 a.c

 if (isNaN(x) || isNaN(y)  || isNaN(z)) {
 alert ("Please enter numbers!");
 }
 else {
 correct = confirm("Are you sure that it is " + z + " ?");
 }
} while (!correct)

if (z == x*y) {
 alert ("Right !");
 corrects += 1;
}
else {
 alert ("Wrong!");
 incorrects += 1;
}
 wanttoplaymore= confirm("want to play more? " );
} while (wanttoplaymore)
alert("Corrects:" + corrects + " incorrects: " + incorrects);
}