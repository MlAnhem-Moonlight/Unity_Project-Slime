using BehaviorTree;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System;

public class AIXBehaviorTree : Tree
{
    public UnityEngine.Transform[] waypoints;

    public static float speed = 10f;

    protected override Nodes SetupTree()
    {
       
        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {         
                new UsingSkill(),      
            }),
            new AIXMovement(transform,waypoints),
        });
        return root;
    }
}
