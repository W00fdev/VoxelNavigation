using System;
using System.Collections.Generic;
using System.Linq;
using Octrees;
using Pathfinding;
using Tools;
using UnityEngine;

namespace Root.Statistics
{
    public class FlowFieldsBenchmark : MonoBehaviour
    {
        [SerializeField] Transform _source;
        [SerializeField] List<Transform> _benchmarkTests;
        [SerializeField] GraphMover _mover;

        Vector3 _destination;
        FlowFieldGraph _flowFieldGraph;

        public void GoCheckWithMover(int index)
        {
            _mover.Go(_source.position, _benchmarkTests[index].position);
        }

        public void ExploreBenchmarkAt(FlowFieldGraph flowFieldGraph, int index)
        {
            _flowFieldGraph = flowFieldGraph;

            Vector3 benchmarkPosition = _benchmarkTests[index].position;

            OctreeNode currentNode = GetClosestNode(_source.position);
            OctreeNode destinationNode = GetClosestNode(benchmarkPosition);

            UnityEngine.Debug.Log($"#{Time.frameCount}: destination node pos: {destinationNode.bounds.center}");

            flowFieldGraph.FindPath(currentNode, destinationNode);
        }

        public void GoGraph(FlowFieldGraph flowFieldGraph)
        {
            _flowFieldGraph = flowFieldGraph;

            OctreeNode currentNode = GetClosestNode(_source.position);
            OctreeNode prevNode = null;

            var performanceLogger = new PerformanceLogger($"performance_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            UnityEngine.Debug.Log($"#{Time.frameCount}: start benchmark AStar");
            foreach (Transform benchmarkTest in _benchmarkTests)
            {
                OctreeNode destinationNode = GetClosestNode(benchmarkTest.position);
                performanceLogger.Start();

                if (!_flowFieldGraph.FindPath(currentNode, destinationNode))
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
                    for (int i = 1; i < _flowFieldGraph.GetPathLength(); i++)
                    {
                        OctreeNode octreeNode = _flowFieldGraph.GetPathNode(i);
                        OctreeNode prevOctreeNode = _flowFieldGraph.GetPathNode(i - 1);

                        // g in AStar can be squaredDistance
                        pathLength += (octreeNode.bounds.center - prevOctreeNode.bounds.center).magnitude;
                    }
                    //                    pathLength = _aStarGraph.pathList.Last().g;
                }

                performanceLogger.LogPerformance(pathLength, _flowFieldGraph.GetPathLength(), _flowFieldGraph.actionsTaken);

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

            foreach (var kvNode in _flowFieldGraph.nodes)
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