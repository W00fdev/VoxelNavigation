using System;
using System.Collections.Generic;
using System.Linq;
using Octrees;
using Pathfinding;
using Tools;
using UnityEngine;

namespace DefaultNamespace.Statistics
{
    public class AStarBenchmark : MonoBehaviour
    {
        [SerializeField] Transform _source;
        [SerializeField] List<Transform> _benchmarkTests;
        [SerializeField] OctreeGenerator _octreeGenerator;
        [SerializeField] Mover _mover;

        Vector3 _destination;
        AStarGraph _aStarGraph;
        AStarGrid _aStarGrid;

        public void GoCheckWithMover(int index)
        {
            _mover.Go(_source.position, _benchmarkTests[index].position);
        }

        public void ExploreBenchmarkAt(int index)
        {
            _aStarGrid = _octreeGenerator.aStarGrid;

            Vector3 benchmarkPosition = _benchmarkTests[index].position;

            GridNode currentNode = GetClosestGridNode(_source.position);
            GridNode destinationNode = GetClosestGridNode(benchmarkPosition);

            UnityEngine.Debug.Log($"#{Time.frameCount}: destination node pos: {destinationNode.position}");

            _aStarGrid.AStar(currentNode, destinationNode);
        }

        public void GoGrid()
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
        }

        public void GoGraph()
        {
            _aStarGraph = _octreeGenerator.aStarGraph;

            OctreeNode currentNode = GetClosestNode(_source.position);
            OctreeNode prevNode = null;

            var performanceLogger = new PerformanceLogger($"performance_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            UnityEngine.Debug.Log($"#{Time.frameCount}: start benchmark AStar");
            foreach (Transform benchmarkTest in _benchmarkTests)
            {
                OctreeNode destinationNode = GetClosestNode(benchmarkTest.position);
                performanceLogger.Start();

                if (!_aStarGraph.AStar(currentNode, destinationNode))
                {
                    throw new Exception($"Can't find a benchmark path to: {destinationNode.bounds.center}");
                }

                performanceLogger.Stop();

                float pathLength;
                if (prevNode == null)
                {
                    pathLength = Vector3.Distance(_source.position, destinationNode.bounds.center);
                }
                else
                {
                    pathLength = 0f;
                    for (int i = 1; i < _aStarGraph.GetPathLength(); i++)
                    {
                        OctreeNode octreeNode = _aStarGraph.GetPathNode(i);
                        OctreeNode prevOctreeNode = _aStarGraph.GetPathNode(i - 1);

                        // g in AStar is squaredDistance
                        pathLength += (octreeNode.bounds.center - prevOctreeNode.bounds.center).magnitude;
                    }
//                    pathLength = _aStarGraph.pathList.Last().g;
                }

                performanceLogger.LogPerformance(pathLength, _aStarGraph.GetPathLength(), _aStarGraph.actionsTaken);

                prevNode = destinationNode;

                UnityEngine.Debug.Log(
                    $"#{Time.frameCount}: ends pathfinding with: {performanceLogger.lastExecutionTimeMs} ms");
            }


            performanceLogger.SaveToFile();
        }

        GridNode GetClosestGridNode(Vector3 position)
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

            foreach (var kvNode in _aStarGrid.nodes)
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
        }

        OctreeNode GetClosestNode(Vector3 position)
        {
            OctreeNode closestNode = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var kvNode in _aStarGraph.nodes)
            {
                OctreeNode node = kvNode.Key;
                float distanceSqr = (node.bounds.center - position).sqrMagnitude;

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