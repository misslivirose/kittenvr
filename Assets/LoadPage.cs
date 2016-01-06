using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections;

public class LoadPage : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(Pause(3.0f));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator Pause(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);
        EditorSceneManager.LoadScene(1);

    }
}
