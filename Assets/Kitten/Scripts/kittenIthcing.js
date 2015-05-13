#pragma strict

function Start () {
	GetComponent.<Animation>()["kittAll_Ithcing"].layer  = 1;
	GetComponent.<Animation>()["kittAll_Ithcing"].wrapMode = WrapMode.Once;
}

function Update () {
	if(Input.GetKey ("c")){
		GetComponent.<Animation>().Play("kittAll_Ithcing");
	}
}