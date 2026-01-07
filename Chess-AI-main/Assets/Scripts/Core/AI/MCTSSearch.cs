namespace Chess
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using static System.Math;
    class MCTSSearch : ISearch
    {
        public event System.Action<Move> onSearchComplete;

        MoveGenerator moveGenerator;

        Move bestMove;
        int bestEval;
        bool abortSearch;

        MCTSSettings settings;
        Board board;
        Evaluation evaluation;

        System.Random rand;

        // Diagnostics
        public SearchDiagnostics Diagnostics { get; set; }
        System.Diagnostics.Stopwatch searchStopwatch;

        public MCTSSearch(Board board, MCTSSettings settings)
        {
            this.board = board;
            this.settings = settings;
            evaluation = new Evaluation();
            moveGenerator = new MoveGenerator();
            rand = new System.Random();
        }

        public void StartSearch()
        {
            InitDebugInfo();

            // Initialize search settings
            bestEval = 0;
            bestMove = Move.InvalidMove;

            moveGenerator.promotionsToGenerate = settings.promotionsToSearch;
            abortSearch = false;
            Diagnostics = new SearchDiagnostics();

            SearchMoves();

            onSearchComplete?.Invoke(bestMove);

            if (!settings.useThreading)
            {
                LogDebugInfo();
            }
        }

        public void EndSearch()
        {
            if (settings.useTimeLimit)
            {
                abortSearch = true;
            }
        }

        void SearchMoves()
        {
            // ROOT
            var rootBoard = board.Clone();
            var root = new MCTSNode(rootBoard, rootBoard.WhiteToMove ? 0 : 1);

            // For this task, only one iteration (I hope it is OK)
            if (!abortSearch)
            {
                var selected = SelectNode(root);
                var expanded = ExpandNode(selected, true);
                double reward = Simulate(expanded);
                Backpropagate(expanded, reward);
            }

            // TODO: Redo in next assignment
            var legalMoves = moveGenerator.GenerateMoves(board, true, true);
            if (legalMoves.Count > 0)
                bestMove = legalMoves[0];
        }

        MCTSNode SelectNode(MCTSNode root)
        {
            var c = 1.0;
            MCTSNode node = root;
            while (!node.IsLeaf)
            {
                List<MCTSNode> unused = null;
                foreach (var child in node.Offsprings)
                {
                    if (child.UseCounter == 0)
                    {
                        if (unused == null)
                            unused = new List<MCTSNode>();

                        unused.Add(child);
                    }
                }

                if (unused != null && unused.Count > 0)
                {
                    node = unused[rand.Next(unused.Count)];
                    continue;
                }

                MCTSNode bestChild = null;
                var bestVal = double.NegativeInfinity;
                var parentUsages = node.UseCounter > 0 ? node.UseCounter : 1.0;
                foreach (var child in node.Offsprings)
                {
                    var ucb = child.Reward / child.UseCounter + c * Sqrt(Log(parentUsages) / child.UseCounter);
                    if (ucb > bestVal)
                    {
                        bestVal = ucb;
                        bestChild = child;
                    }
                }

                if (bestChild == null)
                    break;
                node = bestChild;
            }
            return node;
        }

        MCTSNode ExpandNode(MCTSNode node, bool root)
        {
            var moves = moveGenerator.GenerateMoves(node.State, true, root); // Expand state (generate all moves)
            if (moves.Count == 0)
                return node; // No legal moves

            int moveIdx = moves.Count - 1 - node.Offsprings.Count;
            if (moveIdx < 0)
                return node; // All moves expanded

            Move toExpand = moves[moveIdx];

            Board nBoard = node.State.Clone();
            nBoard.MakeMove(toExpand, true);

            return new MCTSNode(node, nBoard, toExpand, nBoard.WhiteToMove ? 0 : 1);
        }

        double Simulate(MCTSNode node)
        {     
            var simBoard = node.State.GetLightweightClone();
            int currentPlayerId = node.PlayerID;
            int opponentId = currentPlayerId == 1 ? 0 : 1;

            int steps = 0;
            int maxSteps = settings.playoutDepthLimit;
            while (steps < maxSteps && !abortSearch)
            {
                var terminalInfo = CheckTerminal(simBoard);
                if (terminalInfo.isTerminal)
                {
                    if (terminalInfo.winnerPlayerId == currentPlayerId)
                    {
                        return 1.0; //Based on the given instr.
                    }
                    if (terminalInfo.winnerPlayerId == opponentId)
                    {
                        return 0.0;
                    }
                    break;
                }

                steps++;
                break; // stop after one step
            }

            return evaluation.EvaluateSimBoard(simBoard, currentPlayerId == 1 ? true : false);
        }

        void Backpropagate(MCTSNode node, double reward)
        {
            MCTSNode curr = node;
            while (curr != null)
            {
                curr.Update(reward);
                curr = curr.Parent;
            }
        }

        (bool isTerminal, int winnerPlayerId) CheckTerminal(object simBoard)
        {

            //TODO: This is just a prep for full sim. 
            return (false, -1);
        }

        void LogDebugInfo()
        {
            // Optional
        }

        void InitDebugInfo()
        {
            searchStopwatch = System.Diagnostics.Stopwatch.StartNew();
            // Optional
        }
    }
}