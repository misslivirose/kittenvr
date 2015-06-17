using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KittenScript : MonoBehaviour {

	public GameController _gameController;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void CollectMe()
	{
        Debug.Log("Hit kitten");
		_gameController.AddKitten ();
        GameObject _body = gameObject;
		GameObject.Destroy (_body); //Replace with follow function in future
	}
}
