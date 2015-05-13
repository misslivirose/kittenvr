using UnityEngine;
using System.Collections;

public class KittenController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float turnSpeed = 45.0f;
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			if (hit.transform != null) {
				if(hit.transform.gameObject == this.gameObject)
				{
					Debug.Log("Hit " + hit.transform.gameObject.name + "at" + this.tag);
					hit.transform.Rotate(Vector3.up * turnSpeed);
				}
			}
		}
	}
}
