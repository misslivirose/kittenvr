using UnityEngine;
using System.Collections;

public class RingRotate : MonoBehaviour {

    Vector3 originVector;

	// Use this for initialization
	void Start () {
        float originY = transform.position.y;
        originVector = new Vector3(0.0f, originY, 0.0f);
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.Rotate(originVector, 0.3f);
	}
}
