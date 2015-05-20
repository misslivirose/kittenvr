using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public int kittens_collected = 0;


	/**
	 * Use this for initialization
	 **/
	void Start () {

	}
	
	/**
	 * Update is called once per frame.
	 * Check for various game play components. 
	 **/
	void Update () {
		MouseClickListener ();
		CheckPosition ();
	}

	/**
	 * Check and see if the user has jumped off of the level.
	 * If so, reset the level. 
	 **/
	void CheckPosition()
	{
		if (this.gameObject.transform.position.y < 0) {
			Application.LoadLevel(Application.loadedLevel);
		}
	}

	/**
	 * A function to check if the player has clicked on a kitten.
	 * Eventually update this with a different, more interactive input type.
	 **/
	void MouseClickListener()
	{
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Debug.Log ("Clicked");
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			if (hit.transform != null) {
				Debug.Log (hit.transform.position);
				if(hit.transform.gameObject.tag == "kitten")
				{
					kittens_collected++;
					Destroy(hit.transform.transform.gameObject);
					Debug.Log("You've collected " + kittens_collected + " kittens!");
				}
				if(hit.transform.gameObject.tag.Equals ("leader"))
				{
					Debug.Log ("Hit Leader");
				}
			}
		}
	}
}