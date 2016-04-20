using UnityEngine;
using System.Collections;

public class Teleport : MonoBehaviour {

    Collider active_telepad = null;
    GameObject _activeKitten = null;
    public GameController _gameController;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetAxis("Fire1") >= .8)
        {
            // First check for a teleport collison
            if(! (active_telepad == null))
            {
                MoveToLocation(active_telepad);
            }
            // Next check for a kitten collision
            else if(!(_activeKitten == null))
            {
                // We have the leader kitten
                if(_activeKitten.tag.Equals("leader"))
                {
                    _activeKitten.SendMessage("KittenLeaderClick");
                }
                // We have a regular kitten
                else
                {
                    _gameController.SendMessage("AddKitten");
                    Destroy(_activeKitten);
                    _activeKitten = null;
                }
            }
        }
	}

    // Handle collisions
    void OnTriggerEnter(Collider _object)
    {
        if(_object.gameObject.tag.Equals("teleporter"))
        {
            // Confirm the selection & update visually
            _object.GetComponentInChildren<ParticleSystem>().Play();
            active_telepad = _object;
        }
   
        else if(_object.gameObject.tag.Equals("kitten")||
                _object.gameObject.tag.Equals("leader"))
        {
            _activeKitten = _object.gameObject;
           
        }

    }

    // Break connection to teleporter or kitten
    void OnTriggerExit(Collider _object)
    {
        if(_object.gameObject.tag.Equals("teleporter"))
        {
            _object.GetComponentInChildren<ParticleSystem>().Stop();
            _object.GetComponentInChildren<ParticleSystem>().Clear();

            active_telepad = null;
        }
        else if (_object.gameObject.tag.Equals("kitten") ||
                 _object.gameObject.tag.Equals("leader"))
        {
            _activeKitten = null;
        }
    }

    // Move to the location of the teleport pad, reset active pad
    void MoveToLocation(Collider _targetLocation)
    {
        Vector3 _targetPosition = _targetLocation.gameObject.transform.position;
        _targetLocation.GetComponentInChildren<ParticleSystem>().Stop();
        _targetLocation.GetComponentInChildren<ParticleSystem>().Clear();

        GameObject.FindGameObjectWithTag("Player").transform.position = _targetPosition;
        active_telepad = null;
    }

}
