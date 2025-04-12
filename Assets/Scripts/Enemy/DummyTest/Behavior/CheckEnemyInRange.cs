using BehaviorTree;
using UnityEngine;

public class CheckEnemyInRange : Nodes
{
    private Transform _transform;
    private float _range;
    private LayerMask _layerName;

    public CheckEnemyInRange(Transform transform, float range, LayerMask layer)
    {
        _transform = transform;
        _range = range;
        _layerName = layer;
    }

    public override NodeState Evaluate()
    {
        // Lấy danh sách các đối tượng trong bán kính
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(_transform.position, _range);
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            // Kiểm tra nếu layer của đối tượng thuộc LayerMask
            if (((1 << hitCollider.gameObject.layer) & _layerName) != 0)
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
            // Lưu trữ mục tiêu gần nhất trong cây hành vi
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
