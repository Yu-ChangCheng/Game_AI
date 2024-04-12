using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;


namespace GameAICourse
{


    public class AStarPathSearchImpl
    {

        // Please change this string to your name
        public const string StudentAuthorName = "Yu-Chang Cheng";


        // Null Heuristic for Dijkstra
        public static float HeuristicNull(Vector2 nodeA, Vector2 nodeB)
        {
            return 0f;
        }

        // Null Cost for Greedy Best First
        public static float CostNull(Vector2 nodeA, Vector2 nodeB)
        {
            return 0f;
        }



        // Heuristic distance fuction implemented with manhattan distance
        public static float HeuristicManhattan(Vector2 nodeA, Vector2 nodeB)
        {
            //STUDENT CODE HERE

            // The following code is just a placeholder so that the method has a valid return
            // You will replace it with the correct implementation
            return Mathf.Abs(nodeA.x - nodeB.x) + Mathf.Abs(nodeA.y - nodeB.y);

            //END CODE 
        }

        // Heuristic distance function implemented with Euclidean distance
        public static float HeuristicEuclidean(Vector2 nodeA, Vector2 nodeB)
        {
            //STUDENT CODE HERE

            // The following code is just a placeholder so that the method has a valid return
            // You will replace it with the correct implementation
            return Vector2.Distance(nodeA, nodeB);

            //END CODE 
        }


        // Cost is only ever called on adjacent nodes. So we will always use Euclidean distance.
        // We could use Manhattan dist for 4-way connected grids and avoid sqrroot and mults.
        // But we will avoid that for simplicity.
        public static float Cost(Vector2 nodeA, Vector2 nodeB)
        {
            //STUDENT CODE HERE

            // The following code is just a placeholder so that the method has a valid return
            // You will replace it with the correct implementation
            return HeuristicEuclidean(nodeA, nodeB);

            //END STUDENT CODE
        }



        public static PathSearchResultType FindPathIncremental(
            GetNodeCount getNodeCount,
            GetNode getNode,
            GetNodeAdjacencies getAdjacencies,
            CostCallback G,
            CostCallback H,
            int startNodeIndex, int goalNodeIndex,
            int maxNumNodesToExplore, bool doInitialization,
            ref int currentNodeIndex,
            ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
            ref SimplePriorityQueue<int, float> openNodes, ref HashSet<int> closedNodes, ref List<int> returnPath)
        {
            PathSearchResultType pathResult = PathSearchResultType.InProgress;

            var nodeCount = getNodeCount();

            if (startNodeIndex >= nodeCount || goalNodeIndex >= nodeCount ||
                startNodeIndex < 0 || goalNodeIndex < 0 ||
                maxNumNodesToExplore <= 0 ||
                (!doInitialization &&
                 (openNodes == null || closedNodes == null || currentNodeIndex < 0 ||
                  currentNodeIndex >= nodeCount )))

                return PathSearchResultType.InitializationError;


            // STUDENT CODE HERE

            // The following code is just a placeholder so that the method has a valid return
            // You will replace it with the correct implementation

            if (doInitialization)
            {
                InitializeSearch(getNodeCount, startNodeIndex, goalNodeIndex, H, getNode, ref currentNodeIndex, ref searchNodeRecords, ref openNodes, ref closedNodes, ref returnPath);
            }

            return ContinueSearch(getNode, getAdjacencies, G, H, goalNodeIndex, maxNumNodesToExplore, ref currentNodeIndex, ref searchNodeRecords, ref openNodes, ref closedNodes, ref returnPath);

        }

        private static void InitializeSearch(
            GetNodeCount getNodeCount,
            int startNodeIndex,
            int goalNodeIndex,
            CostCallback H,
            GetNode getNode,
            ref int currentNodeIndex,
            ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
            ref SimplePriorityQueue<int, float> openNodes,
            ref HashSet<int> closedNodes,
            ref List<int> returnPath)
        {
            currentNodeIndex = startNodeIndex;
            searchNodeRecords = new Dictionary<int, PathSearchNodeRecord>();
            openNodes = new SimplePriorityQueue<int, float>();
            closedNodes = new HashSet<int>();
            returnPath = new List<int>();

            var startNode = getNode(startNodeIndex);
            var goalNode = getNode(goalNodeIndex);
            var startRecord = new PathSearchNodeRecord(startNodeIndex, -1, 0, H(startNode, goalNode));

            openNodes.Enqueue(startNodeIndex, startRecord.EstimatedTotalCost);
            searchNodeRecords[startNodeIndex] = startRecord;
        }

        private static PathSearchResultType ContinueSearch(
            GetNode getNode,
            GetNodeAdjacencies getAdjacencies,
            CostCallback G,
            CostCallback H,
            int goalNodeIndex,
            int maxNumNodesToExplore,
            ref int currentNodeIndex,
            ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
            ref SimplePriorityQueue<int, float> openNodes,
            ref HashSet<int> closedNodes,
            ref List<int> returnPath)
        {
            int nodesExplored = 0;
            while (openNodes.Count > 0 && nodesExplored < maxNumNodesToExplore)
            {
                currentNodeIndex = openNodes.Dequeue();
                PathSearchNodeRecord currentNodeRecord = searchNodeRecords[currentNodeIndex];

                if (currentNodeIndex == goalNodeIndex)
                {
                    returnPath = ReconstructPath(currentNodeIndex, searchNodeRecords);
                    return PathSearchResultType.Complete;
                }

                foreach (var neighborIndex in getAdjacencies(currentNodeIndex))
                {
                    Vector2 neighbor = getNode(neighborIndex);
                    float tentativeGScore = currentNodeRecord.CostSoFar + G(getNode(currentNodeIndex), neighbor);
                    float estimatedTotalCost = tentativeGScore + H(neighbor, getNode(goalNodeIndex));

                    if (!searchNodeRecords.ContainsKey(neighborIndex) || tentativeGScore < searchNodeRecords[neighborIndex].CostSoFar)
                    {
                        searchNodeRecords[neighborIndex] = new PathSearchNodeRecord(neighborIndex, currentNodeIndex, tentativeGScore, estimatedTotalCost);

                        if (!openNodes.Contains(neighborIndex))
                            openNodes.Enqueue(neighborIndex, estimatedTotalCost);
                        else
                            openNodes.UpdatePriority(neighborIndex, estimatedTotalCost);
                    }
                }

                closedNodes.Add(currentNodeIndex);
                nodesExplored++;
            }

            if (openNodes.Count == 0)
            {
                // If goal is not reached, find the nearest node to the goal based on heuristic cost
                PathSearchNodeRecord nearestNode = FindNearestNodeToGoal(searchNodeRecords, H, getNode(goalNodeIndex), getNode);
                if (nearestNode != null)
                {
                    returnPath = ReconstructPath(nearestNode.NodeIndex, searchNodeRecords);
                    return PathSearchResultType.Partial;
                }
            }

            return PathSearchResultType.InProgress;
        }

        private static PathSearchNodeRecord FindNearestNodeToGoal(
            Dictionary<int, PathSearchNodeRecord> records,
            CostCallback H,
            Vector2 goalNode,
            GetNode getNode) 
        {
            PathSearchNodeRecord nearestNode = null;
            float nearestDistance = float.MaxValue;

            foreach (var record in records.Values)
            {
                Vector2 nodePosition = getNode(record.NodeIndex); 
                var distance = H(nodePosition, goalNode); 

                if (distance < nearestDistance)
                {
                    nearestNode = record;
                    nearestDistance = distance;
                }
            }

            return nearestNode;
        }

        private static List<int> ReconstructPath(int currentNodeIndex, Dictionary<int, PathSearchNodeRecord> searchNodeRecords)
        {
            var path = new List<int>();
            while (currentNodeIndex != -1)
            {
                path.Add(currentNodeIndex);
                currentNodeIndex = searchNodeRecords.TryGetValue(currentNodeIndex, out var record) ? record.FromNodeIndex : -1;
            }
            path.Reverse();
            return path;
        }
    }

}