using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/**
 * The GameController class helps clean up the behavior of the game.
 * Will be working with the game's EventSystem to handle multiple input types.
 * 6/11/2015
 * @author: Liv Erickson (@misslivirose)
 */
public class GameController : MonoBehaviour {
	bool isInVRMode;
	int kittens_collected;

	// Use this for initialization
	void Start () {
		kittens_collected = 0;
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown("Submit"))
        {

            Debug.Log("Registered press");
            ExecuteEvents.Execute<IPointerClickHandler>(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

        }

	}

    // For the click method
    public void OnClick()
    {

    }

	// Return kittens collected
	public int NumberKittens()
	{
		return kittens_collected;
	}

	// Increase kittens collected
	public void AddKitten()
	{
		kittens_collected++;
	}
}
