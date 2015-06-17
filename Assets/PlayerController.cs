using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	private int kittens_collected = 0;



	private string message;
	private bool hasDisplayedFinal = false;
	private bool isInVRMode;

	/**
	 * Use this for initialization
	 **/
	void Start ()
	{
		if (GameObject.FindGameObjectWithTag ("NoVRPlayer") != null) {
			isInVRMode = false;
		}
		else
		{
			isInVRMode = true;
		}
		Debug.Log (isInVRMode);

	}
	
	/**
	 * Update is called once per frame.
	 * Check for various game play components. 
	 **/
	void Update ()
	{
		ResetOnFall();
	}


	/**
	 * Check if character has fallen off level (FPSController only currently) 
	 **/
	void ResetOnFall()
	{
		if (this.tag != "anchor") {
			if (this.transform.position.y < 0) {
				Application.LoadLevel (Application.loadedLevel);
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
		Vector3 scale = new Vector3(0f, 0f, (-5f * kittens_collected));
		kitten.transform.localPosition = scale;
		//Destroy (kitten);
	}
}