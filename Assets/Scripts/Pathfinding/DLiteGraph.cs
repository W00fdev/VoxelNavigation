using Octrees;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;

namespace Pathfinding
{
    public static class NumberExtensions
    {
        public static bool Approx(this float f1, float f2) => Mathf.Approximately(f1, f2);
    }

    public static class NodeExtensions
    {
        public static float Cost(this DLiteGraph.Node from, DLiteGraph.Node to)
        {
            return Vector3.Distance(from.position, to.position);
        }

        public static float Heuristic(this DLiteGraph.Node from, DLiteGraph.Node to)
        {
            return Vector3.Distance(from.position, to.position);
        }
    }

    public class DLiteGraph
    {
        public class Node
        {
            public Vector3 position { get; set; }

            public float G { get; set; }
            public float RHS { get; set; }
            public bool GEqualRHS => G.Approx(RHS);

            public HashSet<Node> neighbours { get; set; } = new();

            public Node(Vector3 data)
            {
                position = data;
                G = float.MaxValue;
                RHS = float.MaxValue;
            }
        }

        public readonly struct Key : IEquatable<Key>
        {
            public readonly float k1;
            public readonly float k2;

            public Key(float k1, float k2)
            {
                this.k1 = k1;
                this.k2 = k2;
            }

            public static bool operator <(Key a, Key b) => a.k1 < b.k1 || a.k1.Approx(b.k1) && a.k2 < b.k2;
            public static bool operator >(Key a, Key b) => a.k1 > b.k1 || a.k1.Approx(b.k1) && a.k2 > b.k2;
            public static bool operator ==(Key a, Key b) => a.k1.Approx(b.k1) && a.k2.Approx(b.k2);
            public static bool operator !=(Key a, Key b) => !(a == b);

            public bool Equals(Key other)
            {
                return this == other;
            }

            public override bool Equals(object other)
            {
                return other is Key key && Equals(key);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(k1, k2);
            }

            public override string ToString()
            {
                return $"({k1}, {k2})";
            }
        }

        readonly Node _startNode;
        readonly Node _goalNode;
        private List<Node> _allNodes;
        public static readonly Dictionary<OctreeNode, Node> octreeNodesMapper = new();
        public int actionsTaken;
        float _km;

        class KeyNodeComparer : IComparer<(Key, Node)>
        {
            public int Compare((Key, Node) x, (Key, Node) y)
            {
                return x.Item1 < y.Item1 ? -1 : x.Item1 > y.Item1 ? 1 : 0;
            }
        }

        // sorted set will add or remove elements in O(log n) and fetch min at O(1)
        private readonly SortedSet<(Key, Node)> _openSet = new(new KeyNodeComparer());
        private readonly Dictionary<Node, Key> _openSetLookups = new();

        public DLiteGraph(Node startNode, Node goalNode)
        {
            _startNode = startNode;
            _goalNode = goalNode;
        }

        public void Initialize()
        {
            _allNodes = octreeNodesMapper.Values.ToList();
            _openSet.Clear();
            _openSetLookups.Clear();
            _km = 0;

            foreach (Node node in _allNodes)
            {
                node.G = float.MaxValue;
                node.RHS = float.MaxValue;
            }

            _goalNode.RHS = 0;
            var key = CalculateKey(_goalNode);

            _openSet.Add((key, _goalNode));
            _openSetLookups.Add(_goalNode, key);
        }

        public static void AddNode(OctreeNode octreeNode)
        {
            if (!octreeNodesMapper.ContainsKey(octreeNode))
            {
                octreeNodesMapper.Add(octreeNode, new Node(octreeNode.bounds.center));
            }
        }

        public static void AddNeighbours(OctreeNode a, OctreeNode b)
        {
            octreeNodesMapper.TryGetValue(a, out Node nodeA);
            octreeNodesMapper.TryGetValue(b, out Node nodeB);

            if (nodeA == null
                || nodeB == null)
                return;

            nodeA.neighbours.Add(nodeB);
            nodeB.neighbours.Add(nodeA);
        }

        public List<Node> GetPath()
        {
            List<Node> path = new();

            if (_goalNode.G == float.MaxValue)
            {
                throw new Exception("Goal G is Inf");
            }

            Node current = _goalNode;
            int maxActions = _allNodes.Count * _allNodes.Count;
            int actionsTaken = 0;

            while (current != null && actionsTaken++ < maxActions)
            {
                path.Add(current);

                if (current == _startNode)
                    break;

                Node minPredecessor = null;
                float minCost = float.MaxValue;

                foreach (Node predecessor in Predecessors(current))
                {
                    float totalCost = predecessor.G + predecessor.Cost(current);

                    if (totalCost < minCost)
                    {
                        minCost = totalCost;
                        minPredecessor = predecessor;
                    }
                }

                if (minPredecessor == null)
                {
                    Debug.Log($"Can't find predecessor for position: {current.position}");
                    break;
                }

                if (path.Contains(current))
                {
                    throw new Exception($"Cyclic route in path at: {current}");
                }

                current = minPredecessor;
            }

            path.Reverse();

            return path;
        }

        public void RecalculateNode(Node node)
        {
            _km += _startNode.Heuristic(node);

            // successors + predecessors

            foreach (Node neighbour in node.neighbours)
            {
                if (neighbour != _startNode)
                {
                    neighbour.RHS = Mathf.Min(neighbour.RHS, neighbour.Cost(node) + node.G);
                }

                UpdateVertex(neighbour);
            }

            UpdateVertex(node);
            ComputeShortestPath();
        }

        public void ComputeShortestPath()
        {
            int maxSteps = 1000000;
            Key startNodeKey = CalculateKey(_startNode);
            while (_openSet.Count > 0 &&
                   (_openSet.Min.Item1 < startNodeKey || _startNode.RHS > _startNode.G))
            {
                if (maxSteps-- <= 0)
                {
                    throw new Exception("Can't find a path");
                }

                (Key, Node) smallest = _openSet.Min;

                Node node = smallest.Item2;

                _openSet.Remove(smallest);
                _openSetLookups.Remove(node);

                Key newKey = CalculateKey(node);
                if (smallest.Item1 < newKey)
                {
                    _openSet.Add((newKey, node));
                    _openSetLookups[node] = newKey;
                }
                // there is more optimal way
                else if (node.G > node.RHS)
                {
                    node.G = node.RHS;
                    foreach (Node predecessor in Predecessors(node))
                    {
                        if (predecessor != _goalNode)
                        {
                            predecessor.RHS = Mathf.Min(predecessor.RHS, predecessor.Cost(node) + node.G);
                        }

                        UpdateVertex(predecessor);
                    }
                }
                // the way to node is much complicated now, rebuild
                else
                {
                    var gOld = node.G;
                    node.G = float.MaxValue;

                    foreach (Node predecessor in Predecessors(node).Concat(new[] { node }))
                    {
                        if (predecessor.RHS.Approx(predecessor.Cost(node) + gOld))
                        {
                            if (predecessor != _goalNode)
                            {
                                predecessor.RHS = float.MaxValue;
                            }

                            foreach (Node successor in Successors(predecessor))
                            {
                                predecessor.RHS = Mathf.Min(predecessor.RHS,
                                    predecessor.Cost(successor) + successor.G);
                            }
                        }

                        UpdateVertex(predecessor);
                    }
                }

                _startNode.G = _startNode.RHS;
                UnityEngine.Debug.Log($"#{Time.frameCount}: shortest path computed in ({1000000 - maxSteps})");
            }
        }

        private IEnumerable<Node> Predecessors(Node node)
        {
            return node.neighbours;
        }

        private IEnumerable<Node> Successors(Node node)
        {
            return node.neighbours;
        }

        void UpdateVertex(Node node)
        {
            Key key = CalculateKey(node);
            if (!node.GEqualRHS && !_openSetLookups.ContainsKey(node))
            {
                _openSet.Add((key, node));
                _openSetLookups[node] = key;
            }
            else if (node.GEqualRHS && _openSetLookups.ContainsKey(node))
            {
                _openSet.Remove((_openSetLookups[node], node));
                _openSetLookups.Remove(node);
            }
            else if (_openSetLookups.ContainsKey(node))
            {
                _openSet.Remove((_openSetLookups[node], node));
                _openSet.Add((key, node));
                _openSetLookups[node] = key;
            }
        }

        Key CalculateKey(Node node)
        {
            var minK2 = Mathf.Min(node.G, node.RHS);

            return new Key(
                minK2 + node.Heuristic(_startNode) + _km,
                minK2
            );
        }

    }
}