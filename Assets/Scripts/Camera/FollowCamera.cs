using System;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    [SerializeField] private Transform target;
    private Vector3 _position = new Vector3(0, 0, -10);

    void Update()
    {
        _position.x = target.position.x;
        _position.y = target.position.y;
        transform.position = _position;
    }

    void ChangeTarget(Transform newTarget)
    {
        target = newTarget;
    }

}
