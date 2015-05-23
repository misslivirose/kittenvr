using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public int kittens_collected = 0;
	public bool isVisible = false;
	private float timer = 0.0f;
	private string message;

	/**
	 * Use this for initialization
	 **/
	void Start () {

	}
	
	/**
	 * Update is called once per frame.
	 * Check for various game play components. 
	 **/
	void Update () {
		MouseClickListener ();
	}

	/**
	 * A function to check if the player has clicked on a kitten.
	 * Eventually update this with a different, more interactive input type.
	 **/
	void MouseClickListener()
	{
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, 10))
			{
				Collider _hit = hit.collider;
				GameObject _VICTIM = hit.collider.gameObject;
				if(_VICTIM.tag.Equals("kitten"))
				{
					kittens_collected++;
					Destroy(hit.transform.transform.gameObject);
					Debug.Log("You've collected " + kittens_collected + " kittens!");
				}
				else if (_VICTIM.tag.Equals("leader")) {
					Debug.Log("Hit Leader");
					if(kittens_collected == 0)
					{
						message = "Help! My kittens have gone missing! Can you help collect all 7 of them?";
						isVisible = true;
					}
					else if(kittens_collected == 1)
					{
						message = "You've found one kitten so far! Just 6 more to go!";
						isVisible = true;
					}
					else if(kittens_collected < 7)
					{
						message = "You've found " + kittens_collected + " kittens so far! Just " + (7 - kittens_collected) + " left to go!";
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
		GUIFunction();
	}

	void GUIFunction()
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

	void Display()
	{
		if (isVisible) {

		}
	}
}