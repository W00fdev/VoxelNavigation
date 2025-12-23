using System.Linq;
using Octrees;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] OctreeGenerator _octreeGenerator;
        [SerializeField] float _speed = 5f;
        [SerializeField] float _accuracy = 1f;
        [SerializeField] float _turnSpeed = 5f;
        [field: SerializeField] float _pathLength { get; set; }

        int _currentWaypoint;
        OctreeNode _currentNode;
        Vector3 _destination;
        AStarGraph _aStarGraph;

        void Start()
        {
            _aStarGraph = _octreeGenerator.waypoints;
            _currentNode = GetClosestNode(transform.position);

            GetRandomDestination();
        }

        void Update()
        {
            if (_aStarGraph == null)
                return;

            if (_aStarGraph.GetPathLength() == 0
             || _currentWaypoint >= _aStarGraph.GetPathLength())
            {
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
                destinationNode = _aStarGraph.nodes.ElementAt(Random.Range(0, _aStarGraph.nodes.Count))
                   .Key;
            }
            while (!_aStarGraph.AStar(_currentNode, destinationNode));

            _currentWaypoint = 0;
            _pathLength = _currentWaypoint;
        }

        /*private void OnDrawGizmos()
        {
            if (graph == null || graph.GetPathLength() == 0) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(graph.GetPathNode(0).bounds.center, 0.7f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(graph.GetPathNode(graph.GetPathLength() - 1).bounds.center, 0.85f);

            Gizmos.color = Color.green;
            for (int i = 0; i < graph.GetPathLength(); i++)
            {
                Gizmos.DrawWireSphere(graph.GetPathNode(i).bounds.center, 0.5f);
                if (i < graph.GetPathLength() - 1)
                {
                    Vector3 start = graph.GetPathNode(i).bounds.center;
                    Vector3 end = graph.GetPathNode(i + 1).bounds.center;
                    Gizmos.DrawLine(start, end);
                }
            }
        }*/
    }
}
