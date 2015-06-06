using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	public int kittens_collected = 0;
	public bool isVisible = false;
	public CanvasGroup _guiCanvas;
	private float timer = 0.0f;
	private string message;
	private bool hasDisplayedFinal = false;


	/**
	 * Use this for initialization
	 **/
	void Start ()
	{

	}
	
	/**
	 * Update is called once per frame.
	 * Check for various game play components. 
	 **/
	void Update ()
	{
		MouseClickListener ();
		ResetOnFall();
		ListenForReset ();
	}

	/**
	 * A function to check if the player has clicked on a kitten.
	 * Eventually update this with a different, more interactive input type.
	 **/
	void MouseClickListener ()
	{
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			/** 
			 * Logic for OVR Player with floating trigger box
			 * If the tag is not 'NoVRPlayer', the trigger is from
			 * the 'anchor' object on an OVR Controller. If the tag is 'NoVRPlayer',
			 * we are using a typical FPS controller and need to do a little extra.
			 **/ 
			if (this.tag != "NoVRPlayer") {
				if (Physics.Raycast (transform.position, transform.forward, out hit, 10)) {
					KittenClick (hit);
				}
			} else {
				Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if(Physics.Raycast(_ray, out hit))
				{
					KittenClick(hit);
				}
			}
		}
		GUIFunction ();
	}
	/**
	 * When we detect a hit on our input method, call this method.
	 * This will be the same regardless of input device; the calling
	 * check for raycast collisions will vary depending on the character
	 * player type.
	 **/
	void KittenClick (RaycastHit hit)
	{
		GameObject _VICTIM = hit.collider.gameObject;
		if (_VICTIM.tag.Equals ("kitten")) {
			kittens_collected++;
			FollowPlayer (_VICTIM);
			Debug.Log ("You've collected " + kittens_collected + " kittens!");
		} else if (_VICTIM.tag.Equals ("leader")) {
			Debug.Log ("Hit Leader");
			//Call function to orient text facing user
			OrientTextToUser(_VICTIM);
			if (kittens_collected == 0) {
				message = "Help! My kittens have gone missing! Can you help collect all 7 of them?";
				isVisible = true;
			} else if (kittens_collected == 1) {
				message = "You've found one kitten so far! Just 6 more to go!";
				isVisible = true;
			} else if (kittens_collected < 7) {
				message = "You've found " + kittens_collected + " kittens so far! Just " + (7 - kittens_collected) + " left to go!";
				isVisible = true;
			} else {
			
				StartCoroutine(DisplayEndText(5f));	
			}
		}
	}
	/**
	 * Display GUI when Leader Kitten is clicked
	 **/
	void GUIFunction ()
	{
		if (isVisible) {
			timer++;
		}
		
		if (timer > 300f) {
			isVisible = false;
			timer = 0f;
		}
		Display ();
	}
	/**
	 * Helper method for GUI Function 
	 **/
	void Display ()
	{
		//We are using a regular camera, nothing special
		if (this.tag == "NoVRPlayer") {
			if (isVisible) {
				_guiCanvas.alpha = 1;
				Text t = _guiCanvas.GetComponentInChildren<Text> ();
				t.text = message;
			} else {
				_guiCanvas.alpha = 0;
			}
		} 
		//We are playing with the Oculus headset, need to adjust canvas positioning
		else {
			if (isVisible) {
				_guiCanvas.alpha = 1;
				Text t = _guiCanvas.GetComponentInChildren<Text> ();
				t.text = message;
			} else {
				_guiCanvas.alpha = 0;
			}

		}
		Debug.Log ("Canvas Coordinates: " + _guiCanvas.transform.position);

	}
	/**
	 * Check if character has falled off level (FPSController only currently) 
	 **/
	void ResetOnFall()
	{
		if (this.transform.position.y < 0) {
			Application.LoadLevel (Application.loadedLevel);
		}
	}
	/**
	 * Helper methods for winning the game 
	 **/
	IEnumerator DisplayEndText(float wait)
	{
		message = "You've found them all! Thank you so meouch! \n Press 'J' to play again.";
		isVisible = true;
		yield return new WaitForSeconds (wait);
		hasDisplayedFinal = true;
	}
	void ListenForReset()
	{
		if (hasDisplayedFinal == true) {

			if (Input.GetKeyDown (KeyCode.J)) {
				Application.LoadLevel(Application.loadedLevel);
			}

		}
	}
	/**
	 * Orient canvas & kitten to face user 
	 * The canvas is part of the kitten so we just need to orient the gameObject passed in
	 * Helpful overview link for calculating the rotation: 
	 * http://bit.ly/1Fvb8am
	 **/
	void OrientTextToUser(GameObject baseObject)
	{
		float speed = 360f;
		// Note: Player (this) is the target of the Kitten (baseObject)
		Vector3 dist = this.transform.position - baseObject.transform.position;
		Quaternion rot = Quaternion.Slerp (baseObject.transform.rotation, Quaternion.LookRotation (dist), speed * Time.deltaTime);
		baseObject.transform.rotation = rot;
		baseObject.transform.eulerAngles = new Vector3 (0, baseObject.transform.eulerAngles.y, 0);	
	}
	/**
	 * "Collect a kitten by having it follow the player on click. 
	 *  Kitten needs to : reparent to player
	 * 					  orient behind player
	 * 					  set animation to walk
	 **/
	void FollowPlayer(GameObject kitten)
	{
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		kitten.transform.parent = player.transform; 
		Vector3 scale = new Vector3(1f, 1f, -5f * kittens_collected);
		kitten.transform.position = scale;



		//Destroy (kitten);
	}
}