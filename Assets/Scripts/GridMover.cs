using System;
using System.Linq;
using Octrees;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Root
{
    public class GridMover : MonoBehaviour
    {
        [SerializeField] float _speed = 10;
        [SerializeField] float _accuracy = 3f;
        [SerializeField] float _turnSpeed = 10;
        [field: SerializeField] float _pathLength { get; set; }

        int _currentWaypoint;
        GridNode _currentNode;
        Vector3 _destination;
        AStarGrid _aStarGrid;
        Vector3Int _mapSize;

        public void Initialize(AStarGrid aStarGrid, Vector3Int mapSize)
        {
            _aStarGrid = aStarGrid;
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
            GridNode destinationNode = GetClosestNode(to);
            if (!_aStarGrid.AStar(_currentNode, destinationNode))
            {
                throw new Exception("Can't find a path");
            }

            for (var index = 0; index < _aStarGrid.pathList.Count; index++)
            {
                GridNode pathNode = _aStarGrid.pathList[index];
                UnityEngine.Debug.Log($"#{Time.frameCount}: i{index} -- g: {pathNode.g}");
            }

            _currentWaypoint = 0;
            _pathLength = _currentWaypoint;
        }

        void Update()
        {
            if (_aStarGrid == null)
                return;

            if (_aStarGrid.GetPathLength() == 0
                || _currentWaypoint >= _aStarGrid.GetPathLength())
            {
                _currentNode = GetClosestNode(transform.position);
                GetRandomDestination();
                return;
            }

            if (Vector3.Distance(
                    _aStarGrid.GetPathNode(_currentWaypoint)
                        .position,
                    transform.position
                )
                < _accuracy)
            {
                _currentWaypoint++;
                _pathLength = _currentWaypoint;
            }

            if (_currentWaypoint < _aStarGrid.GetPathLength())
            {
                _currentNode = _aStarGrid.GetPathNode(_currentWaypoint);
                _destination = _currentNode.position;

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

        GridNode GetClosestNode(Vector3 position)
        {
            // change to normal finding
            var intPosition = new Vector3Int(Mathf.CeilToInt(position.x), Mathf.CeilToInt(position.y),
                Mathf.CeilToInt(position.z));

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

        void GetRandomDestination()
        {
            GridNode destinationNode;

            do
            {
                /*
                Vector3 RandomPosition = new Vector3(Random.Range(0, _octreeGenerator.MapSize.x),
                    Random.Range(0, _octreeGenerator.MapSize.y), Random.Range(0, _octreeGenerator.MapSize.z));
                destinationNode = GetClosestNode(RandomPosition);
                */

                destinationNode = _aStarGrid.nodes.ElementAt(Random.Range(0, _aStarGrid.nodes.Count))
                    .Value;
            } while (!_aStarGrid.AStar(_currentNode, destinationNode));

            _currentWaypoint = 0;
            _pathLength = _currentWaypoint;
        }

        private void OnDrawGizmos()
        {
            if (_aStarGrid == null || _aStarGrid.GetPathLength() == 0) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_aStarGrid.GetPathNode(0).position, 0.7f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_aStarGrid.GetPathNode(_aStarGrid.GetPathLength() - 1).position, 0.85f);

            Gizmos.color = Color.magenta;
            for (int i = 0; i < _aStarGrid.GetPathLength(); i++)
            {
                Gizmos.DrawWireSphere(_aStarGrid.GetPathNode(i).position, 0.5f);
                if (i < _aStarGrid.GetPathLength() - 1)
                {
                    Vector3 start = _aStarGrid.GetPathNode(i).position;
                    Vector3 end = _aStarGrid.GetPathNode(i + 1).position;
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}