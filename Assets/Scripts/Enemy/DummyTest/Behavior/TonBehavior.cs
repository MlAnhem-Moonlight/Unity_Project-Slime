using BehaviorTree;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System;

public class TonBehavior : Tree
{
    public float speed = 10f;
    public float attackRange = 5f;
    public UnityEngine.LayerMask layerName;
    private TonMovement _tonMovement;

    protected override Nodes SetupTree()
    {
        _tonMovement = new TonMovement(transform, speed, attackRange);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                new CheckEnemyInRange(transform, attackRange,layerName),
                new TonSetTargetNode(_tonMovement),
                new TonAttackNode(_tonMovement),
            }),
            _tonMovement,
        });
        return root;
    }
}
