using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections;

public class LoadPage : MonoBehaviour {

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
	
         if(Input.GetAxis("Fire1") >= 0.75)
        {
            EditorSceneManager.LoadScene(1);
        }
	}
}
