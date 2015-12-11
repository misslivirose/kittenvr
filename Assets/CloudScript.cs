using UnityEngine;
using System.Collections;

public class CloudScript : MonoBehaviour {
	private float lastHit;
	// Use this for initialization
	void Start () {
		lastHit = -50;
	}
	
	// Update is called once per frame
	void Update () {
		if (lastHit == -50) {
			Vector3 newPos = new Vector3 (this.transform.position.x, 
			                             this.transform.position.y, 
			                             this.transform.position.z + .001f);
			this.transform.position = newPos;
		} else if (lastHit == 50) {
			Vector3 newPos = new Vector3 (this.transform.position.x, 
			                              this.transform.position.y, 
			                              this.transform.position.z - .001f);
			this.transform.position = newPos;
		}
		if (this.transform.position.z > 50) {
			lastHit = 50;
		} else if (this.transform.position.z < -50) {
			lastHit = -50;
		}
	}
}
