
var Player : Transform;
var CloudsSpeed: float;
private var distance = 10.0;

private var height = 0;
private var heightDamping = 0;
private var rotationDamping = 0;



function Update() {
  transform.Rotate(0,Time.deltaTime*CloudsSpeed ,0); 
    // rotate 90 degrees around the object's local Y axis:
    }

function LateUpdate () {



	if (!Player)
		return;

	var wantedHeight = Player.position.y + height;
		
	var currentHeight = transform.position.y;

	currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);

	transform.position = Player.position;

	transform.position.y = currentHeight;

}