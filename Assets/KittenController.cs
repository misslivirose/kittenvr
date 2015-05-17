using UnityEngine;
using System.Collections;

public class KittenController : MonoBehaviour {

	public bool isVisible = false;
	private float timer = 0.0f;
	public GameController playerStats;
	private string message;
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

					if(playerStats.kittens_collected == 0)
					{
						message = "Help! My kittens have gone missing! Can you help collect all 7 of them?";
						isVisible = true;
					}
					else if(playerStats.kittens_collected < 7)
					{
						message = "You've found " + playerStats.kittens_collected + " kittens so far! Just " + (7 - playerStats.kittens_collected) + " left to go!";
						isVisible = true;
					}
					else
					{
						message = "You've found them all! Thank you so meouch!";
						isVisible = true;
						System.Threading.Thread.Sleep(2000);
						Application.LoadLevel(Application.loadedLevel);
					}
				}
			}
		}
		if (isVisible) {
			timer++;
		}

		if (timer > 300f) {
			isVisible = false;
			timer = 0f;
		}
	
	}

	void OnGUI()
	{
		if (isVisible) {
			GUI.Box (new Rect (Screen.width / 2 - 290, Screen.height / 1 - 100, 600, 80), message);
		}
	}
}
