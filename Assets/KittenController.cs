using UnityEngine;
using System.Collections;

public class KittenController : MonoBehaviour {

	public bool isVisible = false;
	private float timer = 0.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float turnSpeed = 45.0f;
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit))
			if (hit.transform != null) {
				if (hit.transform.gameObject == this.gameObject) {
					Debug.Log ("Hit " + hit.transform.gameObject.name + "at" + this.tag);
					isVisible = true;
					hit.transform.Rotate (Vector3.up * turnSpeed);
				}
			}
		}
		if (isVisible) {
			timer++;
		}

		if (timer > 300f) {
			isVisible = false;
		}
	
	}

	void OnGUI()
	{
		if (isVisible) {
			GUI.Box (new Rect (Screen.width / 2 - 290, Screen.height / 1 - 100, 600, 80), "\nHelp! My kittens have gone missing! \n Can you help me collect all 7 of them?");
		}
	}
}
