using UnityEngine;
using System.Collections;

public class BoxScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnCollisionEnter(Collision other)
	{
		Color newColor = new Color (Random.value*10, Random.value*10, Random.value*10);
		this.gameObject.GetComponent<MeshRenderer> ().material.color = newColor;
	}

}
