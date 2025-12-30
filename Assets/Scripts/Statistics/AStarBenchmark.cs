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

        public void GoCheckWithMover(int index)
        {
            _mover.Go(_source.position, _benchmarkTests[index].position);
        }

        public void Go()
        {
            _aStarGraph = _octreeGenerator.waypoints;
            OctreeNode currentNode = GetClosestNode(_source.position);
            OctreeNode prevNode = null;

            var performanceLogger = new PerformanceLogger($"performance_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            float pathLength = 0f;
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