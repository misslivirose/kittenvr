#pragma strict

function Start () {
	GetComponent.<Animation>()["Ithcing"].layer  = 1;
	GetComponent.<Animation>()["Ithcing"].wrapMode = WrapMode.Once;
	GetComponent.<Animation>()["Meow"].layer  = 1;
	GetComponent.<Animation>()["Meow"].wrapMode = WrapMode.Once;
}

function Update () {
	if(Input.GetKey ("c")){
		GetComponent.<Animation>().Play("Ithcing");
}
	if(Input.GetKey ("m")){
		GetComponent.<Animation>().Play("Meow");
	}
}