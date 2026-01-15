using Octrees;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Pathfinding
{
    public class FlowFieldGraph
    {
        public class Node : IEquatable<Node>
        {
            public int id => _id;
            public OctreeNode octreeNode => _octreeNode;
            public List<Edge> edges => _edges;

            public float f;
            public float g;
            public float h;
            public Node from;
            public Node to;

            readonly int _id;
            readonly OctreeNode _octreeNode;
            readonly List<Edge> _edges = new();

            static int _nextId;

            public Node(OctreeNode octreeNode)
            {
                _id = _nextId++;
                _octreeNode = octreeNode;
            }

            public bool Equals(Node other)
            {
                return other != null && id == other.id;
            }

            public override bool Equals(object obj)
            {
                return obj is Node other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _id;
            }
        }

        public class Edge
        {
            public Node a => _a;
            public Node b => _b;

            readonly Node _a;
            readonly Node _b;

            public Edge(Node a, Node b)
            {
                _a = a;
                _b = b;
            }

            public override bool Equals(object obj)
            {
                return obj is Edge other && ((other.a == a && other.b == b) || (other.a == b && other.b == a));
            }

            public override int GetHashCode()
            {
                return a.GetHashCode() ^ b.GetHashCode();
            }
        }

        const int MaxIterations = 10000;

        public HashSet<Edge> edges => _edges;
        public Dictionary<OctreeNode, Node> nodes => _nodes;
        public List<Node> pathList => _pathList;
        public long actionsTaken => _actionsTaken;

        readonly Dictionary<OctreeNode, Node> _nodes = new();
        readonly List<Node> _pathList = new();
        readonly HashSet<Edge> _edges = new();
        HashSet<Node> _closedSet = new();
        SortedSet<Node> _openSet = new(new NodeComparer());
        long _actionsTaken;
        Camera _mainCamera;

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

        public bool FindPath(OctreeNode startNode, OctreeNode endNode)
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

            // already computed path
            if (_closedSet.Contains(start) || _closedSet.Contains(end))
            {
                ReconstructPath(end);

                Debug.Log($"#{Time.frameCount}: path size -- {_pathList.Count}; actionsTaken -- {_actionsTaken}");

                return true;
            }

            //_openSet.Clear();
            //_closedSet.Clear();
            int iterationCount = 0;

            start.g = 0;
            start.h = Heuristic(start, end);
            start.f = start.g + start.h;
            start.from = null;

            _openSet.Add(start);

            while (_openSet.Count > 0)
            {
                if (++iterationCount > MaxIterations)
                {
                    Debug.LogError("A* exceed maximum iterations");
                    return false;
                }

                Node current = _openSet.First();
                _openSet.Remove(current);

                if (current.Equals(end))
                {
                    ReconstructPath(current);

                    Debug.Log($"#{Time.frameCount}: path size -- {_pathList.Count}; actionsTaken -- {_actionsTaken}");

                    return true;
                }

                _closedSet.Add(current);

                foreach (Edge edge in current.edges)
                {
                    Node neighbor = Equals(edge.a, current)
                        ? edge.b
                        : edge.a;

                    if (_closedSet.Contains(neighbor))
                        continue;

                    float tentative_gScore = current.g + Heuristic(current, neighbor);

                    _actionsTaken++;

                    if (tentative_gScore < neighbor.g || !_openSet.Contains(neighbor))
                    {
                        // update element priority
                        _openSet.Remove(neighbor);

                        neighbor.g = tentative_gScore;
                        neighbor.h = Heuristic(neighbor, end);
                        neighbor.f = neighbor.g + neighbor.h;

                        current.to = neighbor;
                        //neighbor.to = current;
                        neighbor.from = current;

                        _openSet.Add(neighbor);
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

            // already visited and should reuse our closed set path
            while (_openSet.Count > 0)
            {
                Node watched = _openSet.First();

                _openSet.Remove(watched);
                _closedSet.Add(watched);
            }

            foreach (var node in _openSet)
            {
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

                if (x.id.CompareTo(y.id) == 0)
                {
                    return 0;
                }

                int compare = x.f.CompareTo(y.f);

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

        public void RemoveNode(OctreeNode octreeNode)
        {
            Node nodeToDelete = FindNode(octreeNode);

            if (nodeToDelete == null)
                return;

            while (nodeToDelete.edges.Any())
            {
                Edge edge = nodeToDelete.edges.First();
                
                if (edge.a == nodeToDelete)
                {
                    edge.b.edges.Remove(edge);
                }
                else
                {
                    edge.a.edges.Remove(edge);
                }

                _edges.Remove(edge);
                nodeToDelete.edges.Remove(edge);
            }

            _nodes.Remove(octreeNode);
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
            _mainCamera ??= Camera.main;

            if (_mainCamera == null)
                return;

            if (GizmosTools.ShowGraphEdges)
            {
                Gizmos.color = Color.blue;

                foreach (Edge edge in _edges)
                {
                    if (edge.GetMaxDistance(_mainCamera.transform.position) <= GizmosTools.DrawDistance)
                    {
                        Gizmos.DrawLine(edge.a.octreeNode.bounds.center, edge.b.octreeNode.bounds.center);
                    }
                }
            }

            if (GizmosTools.ShowFlowFields)
            {
                Gizmos.color = Color.magenta;

                foreach (Node node in _closedSet.Where(node => node.to != null || node.from != null))
                {
                    float distanceFromCamera = Vector3.Distance(
                        node.octreeNode.bounds.center,
                        _mainCamera.transform.position
                    );

                    if (distanceFromCamera <= GizmosTools.DrawDistance)
                    {
                        Vector3 positionA;
                        Vector3 positionB;

                        if (node.to != null)
                        {
                            positionB = node.to.octreeNode.bounds.center;
                            positionA = node.octreeNode.bounds.center;
                        }
                        else
                        {
                            positionB = node.from.octreeNode.bounds.center;
                            positionA = node.octreeNode.bounds.center;
                        }

                        Vector3 direction = (positionB - positionA).normalized;
                        Gizmos.DrawRay(positionA, direction);

                        Vector3 normal = Vector3.Cross(direction, _mainCamera.transform.forward).normalized;

                        Vector3 right = Quaternion.LookRotation(-direction) * Quaternion.AngleAxis(30, normal) * new Vector3(0, 0, 1);
                        Vector3 left = Quaternion.LookRotation(-direction) * Quaternion.AngleAxis(-30, normal) * new Vector3(0, 0, 1);

                        Gizmos.DrawRay(positionA + direction, right * 0.25f);
                        Gizmos.DrawRay(positionA + direction, left * 0.25f);
                    }
                }

                foreach (Node node in _openSet.Where(node => node.to != null || node.from != null))
                {
                    float distanceFromCamera = Vector3.Distance(
                        node.octreeNode.bounds.center,
                        _mainCamera.transform.position
                    );

                    if (distanceFromCamera <= GizmosTools.DrawDistance)
                    {
                        Vector3 positionA;
                        Vector3 positionB;

                        if (node.to != null)
                        {
                            positionB = node.to.octreeNode.bounds.center;
                            positionA = node.octreeNode.bounds.center;
                        }
                        else
                        {
                            positionB = node.from.octreeNode.bounds.center;
                            positionA = node.octreeNode.bounds.center;
                        }

                        Vector3 direction = (positionB - positionA).normalized;
                        Gizmos.DrawRay(positionA, direction);

                        Vector3 normal = Vector3.Cross(direction, _mainCamera.transform.forward).normalized;

                        Vector3 right = Quaternion.LookRotation(-direction) * Quaternion.AngleAxis(30, normal) * new Vector3(0, 0, 1);
                        Vector3 left = Quaternion.LookRotation(-direction) * Quaternion.AngleAxis(-30, normal) * new Vector3(0, 0, 1);

                        Gizmos.DrawRay(positionA + direction, right * 0.25f);
                        Gizmos.DrawRay(positionA + direction, left * 0.25f);
                    }
                }
            }

            if (GizmosTools.ShowGraphNodes)
            {
                Gizmos.color = Color.blue;

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

            if (GizmosTools.ShowLastPathGraphNodes)
            {
                Gizmos.color = Color.blue;

                foreach (Node node in _closedSet)
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

                Gizmos.color = Color.green;

                foreach (Node node in _openSet)
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