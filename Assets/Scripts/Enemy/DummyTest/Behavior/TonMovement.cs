using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TonMovement : Nodes
{
    private Transform _transform;
    private Transform _target;
    private float _speed;
    private float _range;

    public TonMovement(Transform transform, float speed, float range, Transform target = null)
    {
        _transform = transform;
        _speed = speed;
        _range = range;
        _target = target; // Initialize with the provided target
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public override NodeState Evaluate()
    {
        if (_target == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        float step = _speed * Time.deltaTime;
        Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
        _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);

        if (Vector3.Distance(_transform.position, targetPosition) < 0.1f)
        {
            state = NodeState.SUCCESS;
        }
        else
        {
            state = NodeState.RUNNING;
        }

        return state;
    }
}
