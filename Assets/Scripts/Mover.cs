using System;
using System.Linq;
using Octrees;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class Mover : MonoBehaviour
    {
        private float speed = 5f;
        private float accuracy = 1f;
        private float turnSpeed = 5f;

        private int currentWaypoint;
        private OctreeNode currentNode;
        private Vector3 destination;

        public OctreeGenerator octreeGenerator;
        private Graph graph;
        [SerializeField] private int _pathLength;

        private void Start()
        {
            graph = octreeGenerator.waypoints;
            currentNode = GetClosestNode(transform.position);
            GetRandomDestination();
        }

        private void Update()
        {
            if (graph == null) return;

            if (graph.GetPathLength() == 0 || currentWaypoint >= graph.GetPathLength())
            {
                GetRandomDestination();
                return;
            }

            if (Vector3.Distance(graph.GetPathNode(currentWaypoint).bounds.center, transform.position) < accuracy)
            {
                currentWaypoint++;
                _pathLength = currentWaypoint;
            }

            if (currentWaypoint < graph.GetPathLength())
            {
                currentNode = graph.GetPathNode(currentWaypoint);
                destination = currentNode.bounds.center;

                Vector3 direction = destination - transform.position;
                direction.Normalize();

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
                    turnSpeed * Time.deltaTime);
                transform.Translate(0, 0, speed * Time.deltaTime);
            }
            else
            {
                GetRandomDestination();
            }
        }

        private OctreeNode GetClosestNode(Vector3 position)
        {
            OctreeNode closestNode = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var kvNode in graph.nodes)
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

        private void GetRandomDestination()
        {
            OctreeNode destinationNode;
            do
            {
                destinationNode = graph.nodes.ElementAt(Random.Range(0, graph.nodes.Count)).Key;
            } while (!graph.AStar(currentNode, destinationNode));

            currentWaypoint = 0;
            _pathLength = currentWaypoint;
        }

        private void OnDrawGizmos()
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
        }
    }
}