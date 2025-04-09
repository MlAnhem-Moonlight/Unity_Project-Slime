using BehaviorTree;
using UnityEngine;

public class CheckEnemyInRange : Nodes
{
    private Transform _transform;
    private float _range;

    public CheckEnemyInRange(Transform transform, float range)
    {
        _transform = transform;
        _range = range;
    }

    public override NodeState Evaluate()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(_transform.position, _range);
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Human"))
            {
                float distance = Vector3.Distance(_transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestTarget = hitCollider.transform;
                    closestDistance = distance;
                }
            }
        }

        if (closestTarget != null)
        {
            parent.SetData("target", closestTarget);
            state = NodeState.SUCCESS;
        }
        else
        {
            state = NodeState.FAILURE;
        }

        return state;
    }
}
