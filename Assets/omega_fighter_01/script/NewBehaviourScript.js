  #pragma strict

  var speed=10;
  function Start () {
  }
  function Update () {
     var move=speed*Time.deltaTime;
     var ver=Input.GetAxis("Vertical");
     var hor=Input.GetAxis("Horizontal");
 
     transform.Translate(Vector3.forward*ver*move);
     transform.Translate(Vector3.right*hor*move); 
  } 