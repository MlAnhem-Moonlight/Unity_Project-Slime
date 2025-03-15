using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class UsingSkill : Nodes
{

    public UsingSkill()
    {

    }

    public override NodeState Evaluate()
    {
        try
        {
            GameObject AIX = GameObject.FindGameObjectWithTag("AIX");
            if(AIX.transform.childCount > 0)
            {
                state = NodeState.RUNNING;
                return state;
            }
            else
            {
                state = NodeState.FAILURE;
                return state;
            }

        }
        catch(System.Exception e)
        {
            state = NodeState.FAILURE;
            return state;

        }
    }
}
