using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LeaderScript : MonoBehaviour{

	public GameController _gameController;
	public CanvasGroup _guiCanvas;

	private string message;
	private float timer = 0.0f;
	private bool isVisible = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		GUIFunction ();
	}

	public void KittenLeaderClick()
	{
		int kittens_collected = _gameController.NumberKittens ();
		//Call function to orient text facing user
		if (kittens_collected == 0) {
			message = "Help! My kittens have gone missing! Can you help collect all 7 of them?";
			isVisible = true;
		} else if (kittens_collected == 1) {
			message = "You've found one kitten so far! Just 6 more to go!";
			isVisible = true;
		} else if (kittens_collected < 7) {
			message = "You've found " + kittens_collected + " kittens so far! Just " + (7 - kittens_collected) + " left to go!";
			isVisible = true;
		} else {
			message = "You've found them all! Thank you so meouch! \n Press 'J' to play again.";
            isVisible = true;
		}
		GUIFunction ();
	}
	/**
	 * Display GUI when Leader Kitten is clicked
	 **/
	void GUIFunction ()
	{
		if (isVisible) {
			timer++;
		}
		
		if (timer > 300f) {
			isVisible = false;
			timer = 0f;
		}
		Display ();
	}
	/**
	 * Helper method for GUI Function 
	 **/
	void Display ()
	{
		Text t = _guiCanvas.GetComponentInChildren<Text> ();
		if (isVisible) {
			t.text = message;
		} 
		else {
			t.text = "Captain Mosby";
		}
	}
}
