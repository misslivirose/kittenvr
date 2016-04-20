using UnityEngine;
using System.Collections;

public class NoGravity : MonoBehaviour {

    // Public variable to set the distance
    public float _distance;

    // Variables for floating
    private Vector3 _top, _bottom;
    private float _percent = 0.0f;
    private float _speed = 0.1f;
    public Direction _direction;

    // Define direction up and down
    public enum Direction { UP, DOWN};
    
    // Set the direction to up, and the locations
	void Start () {
        _top = new Vector3(transform.position.x,
                           transform.position.y + _distance,
                           transform.position.z);
        _bottom = new Vector3(transform.position.x,
                       transform.position.y - _distance,
                       transform.position.z);
    }
	
	void Update () {
        ApplyFloatingEffect();
        ApplyRotationEffect();
    }

    // Apply the floating effect between the given positions
    void ApplyFloatingEffect()
    {
        if (_direction == Direction.UP && _percent < 1)
        {

            _percent += Time.deltaTime * _speed;
            transform.position = Vector3.Lerp(_top, _bottom, _percent);
        }
        else if (_direction == Direction.DOWN && _percent < 1)
        {
            _percent += Time.deltaTime * _speed;
            transform.position = Vector3.Lerp(_bottom, _top, _percent);
        }

        if (_percent >= 1)
        {
            _percent = 0.0f;
            if (_direction == Direction.UP)
            {
                _direction = Direction.DOWN;
            }
            else
            {
                _direction = Direction.UP;
            }
        }
    }

    // Apply a random rotation effect
    void ApplyRotationEffect()
    {
        transform.Rotate(Vector3.forward, Time.deltaTime * 25f);
    }
}
