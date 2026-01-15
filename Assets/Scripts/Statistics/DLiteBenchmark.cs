using System;
using System.Collections.Generic;
using System.Linq;
using Octrees;
using Pathfinding;
using Tools;
using UnityEngine;

namespace Root.Statistics
{
    public class DLiteBenchmark : MonoBehaviour
    {
        [SerializeField] Transform _source;
        [SerializeField] List<Transform> _benchmarkTests;
        [SerializeField] GraphMover _mover;

        Vector3 _destination;
        Octree _octree;
        //AStarGrid _aStarGrid;

        public void Initialize(Octree octree)
        {
            _octree = octree;
        }

        public void GoCheckWithMover(int index)
        {
            _mover.Go(_source.position, _benchmarkTests[index].position);
        }

/*        public void ExploreBenchmarkAt(int index)
        {
            _aStarGrid = _octreeGenerator.aStarGrid;

            Vector3 benchmarkPosition = _benchmarkTests[index].position;

            GridNode currentNode = GetClosestGridNode(_source.position);
            GridNode destinationNode = GetClosestGridNode(benchmarkPosition);

            UnityEngine.Debug.Log($"#{Time.frameCount}: destination node pos: {destinationNode.position}");

            _aStarGrid.AStar(currentNode, destinationNode);
        }*/

/*        public void GoGrid()
        {
            _aStarGrid = _octreeGenerator.aStarGrid;

            GridNode currentNode = GetClosestGridNode(_source.position);
            GridNode prevNode = null;

            var performanceLogger = new PerformanceLogger($"performance_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            float pathLength = 0f;
            UnityEngine.Debug.Log($"#{Time.frameCount}: start benchmark AStar");
            foreach (Transform benchmarkTest in _benchmarkTests)
            {
                GridNode destinationNode = GetClosestGridNode(benchmarkTest.position);
                performanceLogger.Start();

                if (!_aStarGrid.AStar(currentNode, destinationNode))
                {
                    throw new Exception($"Can't find a benchmark path to: {destinationNode.position}");
                }

                performanceLogger.Stop();

                if (prevNode == null)
                {
                    pathLength = Vector3.Distance(_source.position, destinationNode.position);
                }
                else
                {
                    pathLength = 0f;
                    for (int i = 1; i < _aStarGrid.GetPathLength(); i++)
                    {
                        GridNode octreeNode = _aStarGrid.GetPathNode(i);
                        GridNode prevOctreeNode = _aStarGrid.GetPathNode(i - 1);

                        // g in AStar is squaredDistance
                        pathLength += (octreeNode.position - prevOctreeNode.position).magnitude;
                    }
                }

                performanceLogger.LogPerformance(pathLength, _aStarGrid.GetPathLength(), _aStarGrid.actionsTaken);

                prevNode = destinationNode;

                UnityEngine.Debug.Log(
                    $"#{Time.frameCount}: ends pathfinding with: {performanceLogger.lastExecutionTimeMs} ms");
            }


            performanceLogger.SaveToFile();
        }*/

        public void GoGraph()
        {
            DLiteGraph.Node startNode = GetClosestNode(_source.position);
            DLiteGraph.Node prevNode = null;

            var performanceLogger = new PerformanceLogger($"performance_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            UnityEngine.Debug.Log($"#{Time.frameCount}: start benchmark D-Lite");
            foreach (Transform benchmarkTest in _benchmarkTests)
            {
                DLiteGraph.Node destinationNode = GetClosestNode(benchmarkTest.position);
                DLiteGraph dLiteGraph = new(startNode, destinationNode);

                performanceLogger.Start();

                dLiteGraph.Initialize();
                dLiteGraph.ComputeShortestPath();
                var path = dLiteGraph.GetPath();

                performanceLogger.Stop();

                float pathLength;
                if (prevNode == null)
                {
                    pathLength = Vector3.Distance(_source.position, destinationNode.position);
                }
                else
                {
                    pathLength = 0f;
                    for (int i = 1; i < path.Count; i++)
                    {
                        DLiteGraph.Node octreeNode = path[i];
                        DLiteGraph.Node prevOctreeNode = path[i - 1];

                        // g in AStar is squaredDistance
                        pathLength += (octreeNode.position - prevOctreeNode.position).magnitude;
                    }
//                    pathLength = _aStarGraph.pathList.Last().g;
                }

                performanceLogger.LogPerformance(pathLength, path.Count, dLiteGraph.actionsTaken);

                prevNode = destinationNode;

                UnityEngine.Debug.Log(
                    $"#{Time.frameCount}: ends pathfinding with: {performanceLogger.lastExecutionTimeMs} ms");
            }

            performanceLogger.SaveToFile();
        }

/*        GridNode GetClosestGridNode(Vector3 position)
        {
            // change to normal finding
            var intPosition = new Vector3Int(Mathf.CeilToInt(position.x), Mathf.CeilToInt(position.y),
                Mathf.CeilToInt(position.z));

            if (_aStarGrid.nodes.TryGetValue(intPosition, out GridNode gridNode))
            {
                return gridNode;
            }

            GridNode closestNode = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var kvNode in _dLiteGraph.allNodes)
            {
                GridNode node = kvNode.Value;
                float distanceSqr = (node.position - position).sqrMagnitude;

                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestNode = node;
                }
            }

            return closestNode;
        }*/

        DLiteGraph.Node GetClosestNode(Vector3 position)
        {
            DLiteGraph.Node closestNode = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (DLiteGraph.Node node in DLiteGraph.octreeNodesMapper.Values)
            {
                float distanceSqr = (node.position - position).sqrMagnitude;

                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestNode = node;
                }
            }

            return closestNode;
        }
    }
}