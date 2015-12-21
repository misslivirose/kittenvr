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

    void OnTriggerEnter(Collider _player)
    {
        //Debug.Log("Collision!");
        CollectMe();
    }

    // Collection
    public void CollectMe()
    {
        GameObject _EventSys = GameObject.FindGameObjectWithTag("eventsystem");
        _EventSys.SendMessage("AddKitten");
        Destroy(gameObject);
    }

}
