#pragma strict

function Start () {
	GetComponent.<Animation>()["kittAll_Meow"].layer  = 1;
	GetComponent.<Animation>()["kittAll_Meow"].wrapMode = WrapMode.Once;
}

function Update () {
	if(Input.GetKey ("m")){
		GetComponent.<Animation>().Play("kittAll_Meow");
	}
}