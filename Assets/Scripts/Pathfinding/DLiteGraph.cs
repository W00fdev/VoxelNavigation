using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public static class NumberExtensions
    {
        public static bool Approx(this float f1, float f2) => Mathf.Approximately(f1, f2);
    }

    public class DLiteGraph
    {
        public class Node
        {
            public Vector3 Data { get; set; }

            public Func<Node, Node, float> Cost { get; set; }
            public Func<Node, Node, float> Heuristic { get; set; }

            public float G { get; set; }
            public float RHS { get; set; }
            public bool GEqualRHS => G.Approx(RHS);

            public List<Node> Neighbours { get; set; } = new();

            public Node(Vector3 data, Func<Node, Node, float> cost, Func<Node, Node, float> heuristic)
            {
                Data = data;
                Cost = cost;
                Heuristic = heuristic;

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
        readonly List<Node> _allNodes;
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
        private readonly Dictionary<Node, Key> _lookups;

        public DLiteGraph(Node startNode, Node goalNode, List<Node> allNodes)
        {
            _startNode = startNode;
            _goalNode = goalNode;
            _allNodes = allNodes;
        }

        public void ComputeShortestPath()
        {
            int maxSteps = 1000000;
            while (_openSet.Count > 0 &&
                   (_openSet.Min.Item1 < CalculateKey(_startNode) || _startNode.RHS > _startNode.G))
            {
                if (maxSteps-- <= 0)
                {
                    throw new Exception("Can't find a path");
                }

                (Key, Node) smallest = _openSet.Min;
                _openSet.Remove(smallest);
                _lookups.Remove(smallest.Item2);

                Node node = smallest.Item2;

                Key newKey = CalculateKey(node);
                if (smallest.Item1 < newKey)
                {
                    _openSet.Add((newKey, node));
                    _lookups[node] = newKey;
                }
                else if (node.G > node.RHS)
                {
                    node.G = node.RHS;
                    foreach (Node predecessor in Predecessors(node))
                    {
                        if (predecessor != _goalNode)
                        {
                            predecessor.RHS = Mathf.Min(predecessor.RHS, predecessor.Cost(predecessor, node) + node.G);
                        }

                        UpdateVertex(predecessor);
                    }
                }
                else
                {
                    var gOld = node.G;
                    node.G = float.MaxValue;

                    foreach (Node predecessor in Predecessors(node).Concat(new[] { node }))
                    {
                        if (predecessor.RHS.Approx(predecessor.Cost(predecessor, node) + gOld))
                        {
                            if (predecessor != _goalNode)
                            {
                                predecessor.RHS = float.MaxValue;
                            }

                            foreach (Node successor in Successors(predecessor))
                            {
                                predecessor.RHS = Mathf.Min(predecessor.RHS,
                                    predecessor.Cost(predecessor, successor) + successor.G);
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
            return node.Neighbours;
        }

        private IEnumerable<Node> Successors(Node node)
        {
            return node.Neighbours;
        }

        void UpdateVertex(Node node)
        {
            Key key = CalculateKey(node);
            if (!node.GEqualRHS && !_lookups.ContainsKey(node))
            {
                _openSet.Add((key, node));
                _lookups[node] = key;
            }
            else if (node.GEqualRHS && _lookups.ContainsKey(node))
            {
                _openSet.Remove((key, node));
                _lookups.Remove(node);
            }
            else if (_lookups.ContainsKey(node))
            {
                _openSet.Remove((_lookups[node], node));
                _openSet.Add((key, node));
                _lookups[node] = key;
            }
        }

        Key CalculateKey(Node node)
        {
            var minK2 = Mathf.Min(node.G, node.RHS);

            return new Key(
                minK2 + node.Heuristic(node, _startNode) + _km,
                minK2
            );
        }

        public void Initialize()
        {
            _openSet.Clear();
            _lookups.Clear();
            _km = 0;

            foreach (Node node in _allNodes)
            {
                node.G = float.MaxValue;
                node.RHS = float.MaxValue;
            }

            _goalNode.RHS = 0;
            var key = CalculateKey(_goalNode);
        }
    }
}