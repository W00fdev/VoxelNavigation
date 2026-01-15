using System;
using Octrees;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Root
{
    public class GraphMover : MonoBehaviour
    {
        [SerializeField] float _speed = 10;
        [SerializeField] float _accuracy = 3f;
        [SerializeField] float _turnSpeed = 10;
        [field: SerializeField] float _pathLength { get; set; }

        int _currentWaypoint;
        OctreeNode _currentNode;
        Vector3 _destination;
        AStarGraph _aStarGraph;
        Vector3Int _mapSize;

        public void Initialize(AStarGraph aStarGraph, Vector3Int mapSize)
        {
            _aStarGraph = aStarGraph;
            _mapSize = mapSize;
        }

        public void GoRandom()
        {
            _currentNode = GetClosestNode(transform.position);

            GetRandomDestination();
        }
        
        public void Go(Vector3 from, Vector3 to)
        {
            transform.position = from;

            _currentNode = GetClosestNode(from);
            OctreeNode destinationNode = GetClosestNode(to);
            if (!_aStarGraph.AStar(_currentNode, destinationNode))
            {
                throw new Exception("Can't find a path");
            }

            for (var index = 0; index < _aStarGraph.pathList.Count; index++)
            {
                AStarNode pathNode = _aStarGraph.pathList[index];
                UnityEngine.Debug.Log($"#{Time.frameCount}: i{index} -- g: {pathNode.g}");
            }

            _currentWaypoint = 0;
            _pathLength = _currentWaypoint;
        }

        void Update()
        {
            if (_aStarGraph == null)
                return;

            if (_aStarGraph.GetPathLength() == 0
                || _currentWaypoint >= _aStarGraph.GetPathLength())
            {
                _currentNode = GetClosestNode(transform.position);
                GetRandomDestination();
                return;
            }

            if (Vector3.Distance(
                    _aStarGraph.GetPathNode(_currentWaypoint)
                        .bounds.center,
                    transform.position
                )
                < _accuracy)
            {
                _currentWaypoint++;
                _pathLength = _currentWaypoint;
            }

            if (_currentWaypoint < _aStarGraph.GetPathLength())
            {
                _currentNode = _aStarGraph.GetPathNode(_currentWaypoint);
                _destination = _currentNode.bounds.center;

                Vector3 direction = _destination - transform.position;
                direction.Normalize();

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    _turnSpeed * Time.deltaTime
                );

                transform.Translate(0, 0, _speed * Time.deltaTime);
            }
            else
            {
                GetRandomDestination();
            }
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

        void GetRandomDestination()
        {
            OctreeNode destinationNode;

            do
            {
                Vector3 RandomPosition = new Vector3(Random.Range(0, _mapSize.x),
                    Random.Range(0, _mapSize.y), Random.Range(0, _mapSize.z));
                destinationNode = GetClosestNode(RandomPosition);

/*                    destinationNode = _aStarGraph.nodes.ElementAt(Random.Range(0, _aStarGraph.nodes.Count))
                       .Key;*/
            } while (!_aStarGraph.AStar(_currentNode, destinationNode));

            _currentWaypoint = 0;
            _pathLength = _currentWaypoint;
        }

        private void OnDrawGizmos()
        {
            if (_aStarGraph == null || _aStarGraph.GetPathLength() == 0) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_aStarGraph.GetPathNode(0).bounds.center, 0.7f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_aStarGraph.GetPathNode(_aStarGraph.GetPathLength() - 1).bounds.center, 0.85f);

            Gizmos.color = Color.magenta;
            for (int i = 0; i < _aStarGraph.GetPathLength(); i++)
            {
                Gizmos.DrawWireSphere(_aStarGraph.GetPathNode(i).bounds.center, 0.5f);
                if (i < _aStarGraph.GetPathLength() - 1)
                {
                    Vector3 start = _aStarGraph.GetPathNode(i).bounds.center;
                    Vector3 end = _aStarGraph.GetPathNode(i + 1).bounds.center;
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}