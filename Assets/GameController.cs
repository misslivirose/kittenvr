using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public int kittens_collected = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			if (hit.transform != null) {
				if(hit.transform.gameObject.tag == "kitten")
				{
					kittens_collected++;
					Destroy(hit.transform.transform.gameObject);
					Debug.Log("You've collected " + kittens_collected + " kittens!");
				}
			}
		}
	}
}