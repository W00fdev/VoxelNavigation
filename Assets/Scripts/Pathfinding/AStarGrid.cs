using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Pathfinding
{
    public class GridNode : IEquatable<GridNode>
    {
        public int id => _id;
        public Vector3Int position => _position;

        public float f;
        public float g;
        public float h;
        public GridNode from;

        readonly int _id;
        readonly Vector3Int _position;

        static int _nextId;

        public GridNode(Vector3Int position)
        {
            _position = position;
            _id = _nextId++;
        }

        public bool Equals(GridNode other)
        {
            return other != null && (id == other.id || position == other.position);
        }

        public override bool Equals(object obj)
        {
            return obj is GridNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }

    public class AStarGrid
    {
        const int MaxIterations = 100000;

        public Dictionary<Vector3Int, GridNode> nodes => _nodes;
        public List<GridNode> pathList => _pathList;
        public long actionsTaken => _actionsTaken;

        readonly List<GridNode> _pathList = new();
        readonly Dictionary<Vector3Int, GridNode> _nodes = new();
        readonly Vector3Int _mapSize;
        HashSet<GridNode> _closedSet;
        SortedSet<GridNode> _openSet;
        Camera _mainCamera;
        long _actionsTaken;

        public AStarGrid(Vector3Int mapSize)
        {
            _mapSize = mapSize;
        }

        public int GetPathLength() => _pathList.Count;

        public GridNode GetPathNode(int index)
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

            return _pathList[index];
        }

        public bool AStar(GridNode start, GridNode end)
        {
            _pathList.Clear();

            if (start == null
                || end == null)
            {
                Debug.LogError("Start or end node not found in the graph");
                return false;
            }

            _openSet = new SortedSet<GridNode>(new NodeComparer());
            _closedSet = new HashSet<GridNode>();
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

                GridNode current = _openSet.First();
                if (!_openSet.Remove(current))
                {
                    throw new Exception($"Can't remove at {current.position} id= {current.id}");
                }

                if (current.Equals(end))
                {
                    ReconstructPath(current);

                    Debug.Log($"#{Time.frameCount}: path size -- {_pathList.Count}; actionsTaken -- {_actionsTaken}");

                    return true;
                }

                _closedSet.Add(current);

                for (int x = current.position.x - 1; x <= current.position.x + 1; x++)
                {
                    if (x < 0 || x >= _mapSize.x)
                        continue;

                    for (int y = current.position.y - 1; y <= current.position.y + 1; y++)
                    {
                        if (y < 0 || y >= _mapSize.y)
                            continue;

                        for (int z = current.position.z - 1; z <= current.position.z + 1; z++)
                        {
                            if (z < 0 || z >= _mapSize.z)
                                continue;

                            if (!_nodes.TryGetValue(new Vector3Int(x, y, z), out GridNode neighbor))
                                continue;

                            if (_closedSet.Contains(neighbor))
                                continue;

                            float tentative_gScore = current.g + Heuristic(current, neighbor);

                            _actionsTaken++;

                            var neighbourInOpenSet = _openSet.Contains(neighbor);
                            if (tentative_gScore < neighbor.g || !neighbourInOpenSet)
                            {
                                _openSet.Remove(neighbor);

                                neighbor.g = tentative_gScore;
                                neighbor.h = Heuristic(neighbor, end);
                                neighbor.f = neighbor.g + neighbor.h;
                                neighbor.from = current;

                                _openSet.Add(neighbor);
                            }
                        }
                    }
                }
            }

            Debug.Log("No path found");
            return false;
        }

        void ReconstructPath(GridNode current)
        {
            while (current != null)
            {
                _pathList.Add(current);
                current = current.from;
            }

            pathList.Reverse();
        }

        float Heuristic(GridNode a, GridNode b)
        {
            // octile distance
            int dx = Mathf.Abs(a.position.x - b.position.x);
            int dy = Mathf.Abs(a.position.y - b.position.y);
            int dz = Mathf.Abs(a.position.z - b.position.z);

            int dMin = Mathf.Min(Mathf.Min(dx, dy), dz);
            int dMax = Mathf.Max(Mathf.Max(dx, dy), dz);
            int dMid = dx + dy + dz - dMin - dMax;

            var heuristic = (1.7f - 1.4f) * dMin + (1.4f - 1f) * dMid + dMax;
            return heuristic * heuristic;
        }
        //float Heuristic(GridNode a, GridNode b) => (a.position - b.position).magnitude;

        public void DrawGraph()
        {
            Gizmos.color = Color.blue;

            _mainCamera ??= Camera.main;

            if (_mainCamera == null)
                return;

            if (GizmosTools.ShowLastPathGraphNodes)
            {
                foreach (GridNode gridNode in _closedSet)
                {
                    float distanceFromCamera = Vector3.Distance(
                        gridNode.position,
                        _mainCamera.transform.position
                    );

                    if (distanceFromCamera <= GizmosTools.DrawDistance)
                    {
                        Gizmos.DrawWireSphere(gridNode.position, GizmosTools.NodesRadius);
                    }
                }

                foreach (GridNode gridNode in _openSet)
                {
                    float distanceFromCamera = Vector3.Distance(
                        gridNode.position,
                        _mainCamera.transform.position
                    );

                    Gizmos.color = Color.green;

                    if (distanceFromCamera <= GizmosTools.DrawDistance)
                    {
                        Gizmos.DrawWireSphere(gridNode.position, GizmosTools.NodesRadius);
                    }
                }
            }
        }

        public class NodeComparer : IComparer<GridNode>
        {
            public int Compare(GridNode x, GridNode y)
            {
                if (ReferenceEquals(x, y))
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                if (x.id.CompareTo(y.id) == 0)
                {
                    return 0;
                }

                int compare = x.f.CompareTo(y.f);

                return compare;
            }
        }

        public void AddNode(GridNode gridNode)
        {
            _nodes.Add(gridNode.position, gridNode);
        }
    }
}