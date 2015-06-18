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

    // Collection
    public void CollectMe()
    {
        Destroy(this);
    }

}
