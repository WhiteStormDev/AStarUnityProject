using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _lerpSpeed;
    [SerializeField] private float _zOffset;
    private void FixedUpdate()
    {
        if (_target == null)
            return;

        transform.position = Vector3.Lerp(transform.position, _target.position, _lerpSpeed * Time.fixedDeltaTime) + new Vector3(0, 0, _zOffset);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
