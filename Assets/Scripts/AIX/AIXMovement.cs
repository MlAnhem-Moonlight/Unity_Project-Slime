using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIXMovement : Nodes
{
    private Transform _transform;
    private Transform[] _waypoints;

    private int _currentWaypointIndex = 0;

    private float _waitTime = 0.5f, _waitCounter = 0f;
    private bool _waiting = false;

    public AIXMovement(Transform transform, Transform[] waypoints)
    {
        _transform = transform;
        _waypoints = waypoints;
    }

    public override NodeState Evaluate()
    {
        if (_waiting)
        {
            _waitCounter += Time.deltaTime;
            if (_waitCounter >= _waitTime) _waiting = false;
            
        }
        else
        {

            
            Transform wp = _waypoints[_currentWaypointIndex];
            if (Vector2.Distance(_transform.position, wp.position) < 0.01f)
            {
                _transform.position = wp.position;
                _waitCounter = 0f;
                _waiting = true;
                _waitTime = Random.Range(0.5f, 1f);
                _currentWaypointIndex = GetRandomWaypointIndex();
            }
            else
            {
                _transform.position = Vector2.MoveTowards(_transform.position, wp.position, AIXBehaviorTree.speed * Time.deltaTime);
                
            }
        }

        state = NodeState.RUNNING;
        return state;
    }

    private int GetRandomWaypointIndex()
    {
        if (_waypoints.Length <= 1)
        {
            return 0;
        }

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, _waypoints.Length);
        } while (randomIndex == _currentWaypointIndex);

        return randomIndex;
    }
}
