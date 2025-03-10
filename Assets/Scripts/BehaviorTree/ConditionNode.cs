using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class ConditionNode : Nodes
    {
        private KeyCode key;

        public ConditionNode(KeyCode key)
        {
            this.key = key;
        }

        public override NodeState Evaluate()
        {
            if (Input.GetKeyDown(key))
            {
                state = NodeState.SUCCESS;
                return state;
            }
            state = NodeState.FAILURE;
            return state;
        }
    }
}
