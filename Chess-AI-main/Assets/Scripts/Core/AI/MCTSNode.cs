namespace Chess
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class MCTSNode
    {
        public MCTSNode Parent { get; private set; } // There is null if this node is root.
        public List<MCTSNode> Offsprings { get; private set; } = new List<MCTSNode>();
        public Board State { get; private set; } // Game state represented by this node
        public Move ParentMove { get; private set; }  // Move that was applied to the parent to reach this node.
        public int PlayerID { get; private set; } // Stores ID of player which plays (Black/White) (Sorry for languange torture :D)
        public int UseCounter { get; private set; }
        public double Reward { get; private set; } // Rewards obtained from simulations passing through (just a suim).

        public bool IsRoot => Parent == null;
        public bool IsLeaf => Offsprings.Count == 0;

        public MCTSNode(Board state, int playerID)
        {
            Parent = null;
            State = state;
            PlayerID = playerID;
            ParentMove = Move.InvalidMove; //I would like to set it to null, which is unfortunatelly invalid. Maybe boxing to object in the future?
            UseCounter = 0;
            Reward = 0.0;
        }


        //Offspring node contructor.
        public MCTSNode(MCTSNode parent, Board state, Move parentMove, int playerID)
        {
            Parent = parent;
            State = state;
            ParentMove = parentMove;
            PlayerID = playerID;
            UseCounter = 0;
            Reward = 0.0;

            if (parent != null)
                parent.AddOffspring(this);
        }


        /// Adds a child node to this node's children list.
        public void AddOffspring(MCTSNode offspring)
        {
            if (offspring != null && !Offsprings.Contains(offspring))
                Offsprings.Add(offspring);
        }


        /// Updates usage count and total reward with a new simulation result.
        public void Update(double reward)
        {
            UseCounter++;
            Reward += reward;
        }

        //public double GetAverageReward()
        //{ 
        //    return UseCounter > 0 ? Reward / UseCounter : 0.0; 
        //}
    }
}