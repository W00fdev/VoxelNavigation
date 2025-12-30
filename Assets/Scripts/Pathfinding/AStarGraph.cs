using System.Collections.Generic;
using System.Linq;
using Octrees;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Pathfinding
{
    public class AStarGraph
    {
        const int MaxIterations = 10000;

        public HashSet<Edge> edges => _edges;
        public Dictionary<OctreeNode, Node> nodes => _nodes;
        public List<Node> pathList => _pathList;
        public long actionsTaken => _actionsTaken;

        readonly Dictionary<OctreeNode, Node> _nodes = new();
        readonly List<Node> _pathList = new();
        readonly HashSet<Edge> _edges = new();
        long _actionsTaken;
        Camera _mainCamera;

        public AStarGraph() { }

        public int GetPathLength() => _pathList.Count;

        public OctreeNode GetPathNode(int index)
        {
            if (_pathList == null)
                return null;

            if (index < 0
             || index >= _pathList.Count)
            {
                Debug.LogError(
                    $"#{Time.frameCount}: Index out of bounds. PathLength: {_pathList.Count}, Index: {index}"
                );

                return null;
            }

            return _pathList[index].octreeNode;
        }

        public bool AStar(OctreeNode startNode, OctreeNode endNode)
        {
            _pathList.Clear();

            Node start = FindNode(startNode);
            Node end = FindNode(endNode);

            if (start == null
             || end == null)
            {
                Debug.LogError("Start or end node not found in the graph");
                return false;
            }

            SortedSet<Node> openSet = new(new NodeComparer());
            HashSet<Node> closedSet = new();
            int iterationCount = 0;

            start.g = 0;
            start.h = Heuristic(start, end);
            start.f = start.g + start.h;
            start.from = null;
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                if (++iterationCount > MaxIterations)
                {
                    Debug.LogError("A* exceed maximum iterations");
                    return false;
                }

                Node current = openSet.First();
                openSet.Remove(current);

                if (current.Equals(end))
                {
                    ReconstructPath(current);

                    Debug.Log($"#{Time.frameCount}: path size -- {_pathList.Count}; actionsTaken -- {_actionsTaken}");

                    return true;
                }

                closedSet.Add(current);

                foreach (Edge edge in current.edges)
                {
                    Node neighbor = Equals(edge.a, current)
                        ? edge.b
                        : edge.a;

                    if (closedSet.Contains(neighbor))
                        continue;

                    if (openSet.Contains(neighbor))
                        continue;

                    float tentative_gScore = current.g + Heuristic(current, neighbor);

                    _actionsTaken++;

                    if (tentative_gScore < neighbor.g
                     || !openSet.Contains(neighbor))
                    {
                        neighbor.g = tentative_gScore;
                        neighbor.h = Heuristic(neighbor, end);
                        neighbor.f = neighbor.g + neighbor.h;
                        neighbor.from = current;
                        openSet.Add(neighbor);
                    }
                }
            }

            Debug.Log("No path found");
            return false;
        }

        void ReconstructPath(Node current)
        {
            while (current != null)
            {
                _pathList.Add(current);
                current = current.from;
            }

            pathList.Reverse();
        }

        float Heuristic(Node a, Node b) => (a.octreeNode.bounds.center - b.octreeNode.bounds.center).magnitude;

        public class NodeComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                if (x == null
                 || y == null)
                    return 0;

                int compare = x.f.CompareTo(y.f);

                if (compare == 0)
                {
                    return x.id.CompareTo(y.id);
                }

                return compare;
            }
        }

        public void AddNode(OctreeNode octreeNode)
        {
            if (!_nodes.ContainsKey(octreeNode))
            {
                _nodes.Add(octreeNode, new Node(octreeNode));
            }
        }

        public void AddEdge(OctreeNode a, OctreeNode b)
        {
            Node nodeA = FindNode(a);
            Node nodeB = FindNode(b);

            if (nodeA == null
             || nodeB == null)
                return;

            var edge = new Edge(nodeA, nodeB);

            if (_edges.Add(edge))
            {
                nodeA.edges.Add(edge);
                nodeB.edges.Add(edge);
            }
        }

        public void DrawGraph()
        {
            Gizmos.color = Color.blue;

            _mainCamera ??= Camera.main;

            if (_mainCamera == null)
                return;

            if (GizmosTools.ShowGraphEdges)
            {
                foreach (Edge edge in _edges)
                {
                    if (edge.GetMaxDistance(_mainCamera.transform.position) <= GizmosTools.DrawDistance)
                    {
                        Gizmos.DrawLine(edge.a.octreeNode.bounds.center, edge.b.octreeNode.bounds.center);
                    }
                }
            }

            if (GizmosTools.ShowGraphNodes)
            {
                foreach (Node node in _nodes.Values)
                {
                    float distanceFromCamera = Vector3.Distance(
                        node.octreeNode.bounds.center,
                        _mainCamera.transform.position
                    );

                    if (distanceFromCamera <= GizmosTools.DrawDistance)
                    {
                        Gizmos.DrawWireSphere(node.octreeNode.bounds.center, GizmosTools.NodesRadius);
                    }
                }
            }
        }

        Node FindNode(OctreeNode octreeNode)
        {
            _nodes.TryGetValue(octreeNode, out Node node);
            return node;
        }
    }
}
