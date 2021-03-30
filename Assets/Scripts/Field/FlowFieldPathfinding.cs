using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Field
{
    public class FlowFieldPathfinding
    {
        private Grid m_Grid;
        private Vector2Int m_Target;
        private Vector2Int m_Start;
        
        public struct Connection
        {
            public Vector2Int Coordinate;
            public float Weight;

            public Connection(Vector2Int coordinate, float weight)
            {
                Coordinate = coordinate;
                Weight = weight;
            }
        }
        
        public FlowFieldPathfinding(Grid grid, Vector2Int target, Vector2Int start)
        {
            m_Grid = grid;
            m_Target = target;
            m_Start = start;
        }

        public void UpdateField()
        {
            foreach (Node node in m_Grid.EnumerateAllNodes())
            {
                node.ResetWeight();
            }
            
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            queue.Enqueue(m_Target);
            m_Grid.GetNode(m_Target).PathWeight = 0f;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                Node currentNode = m_Grid.GetNode(current);
                currentNode.m_OccupationAvailability = Node.OccupationAvailability.CanOccupy;
                
                foreach (Connection neighbour in GetNeighbours(current))
                {
                    Node neighbourNode = m_Grid.GetNode(neighbour.Coordinate);
                    if (currentNode.PathWeight + neighbour.Weight < neighbourNode.PathWeight)
                    {
                        neighbourNode.NextNode = currentNode;
                        neighbourNode.PathWeight = currentNode.PathWeight + neighbour.Weight;
                        queue.Enqueue(neighbour.Coordinate);
                    }
                }
            }

            m_Grid.GetNode(m_Start).m_OccupationAvailability = Node.OccupationAvailability.CanNotOccupy;
            m_Grid.GetNode(m_Target).m_OccupationAvailability = Node.OccupationAvailability.CanNotOccupy;
            
            Node currentPathNode = m_Grid.GetNode(m_Start);

            while (currentPathNode.NextNode != m_Grid.GetNode(m_Target))
            {
                currentPathNode.m_OccupationAvailability = Node.OccupationAvailability.Undefined;
                currentPathNode = currentPathNode.NextNode;
            }
        }

        private IEnumerable<Connection> GetNeighbours(Vector2Int coordinate)
        {
            Vector2Int rightCoordinate = coordinate + Vector2Int.right;
            Vector2Int leftCoordinate = coordinate + Vector2Int.left;
            Vector2Int upCoordinate = coordinate + Vector2Int.up;
            Vector2Int downCoordinate = coordinate + Vector2Int.down;
            
            Vector2Int rightUpCoordinate = coordinate + Vector2Int.right + Vector2Int.up;
            Vector2Int leftUpCoordinate = coordinate + Vector2Int.left + Vector2Int.up;
            Vector2Int rightDownCoordinate = coordinate + Vector2Int.down + Vector2Int.right;
            Vector2Int leftDownCoordinate = coordinate + Vector2Int.down + Vector2Int.left;
            
            bool hasRightNode = rightCoordinate.x < m_Grid.Width && !m_Grid.GetNode(rightCoordinate).IsOccupied;
            bool hasLeftNode = leftCoordinate.x >= 0 && !m_Grid.GetNode(leftCoordinate).IsOccupied;
            bool hasUpNode = upCoordinate.y < m_Grid.Height && !m_Grid.GetNode(upCoordinate).IsOccupied;
            bool hasDownNode = downCoordinate.y >= 0 && !m_Grid.GetNode(downCoordinate).IsOccupied;
            
            bool hasRightUpNode = rightUpCoordinate.x < m_Grid.Width && rightUpCoordinate.y < m_Grid.Height && !m_Grid.GetNode(rightUpCoordinate).IsOccupied &&
                                  hasRightNode && hasUpNode;
            bool hasLeftUpNode = leftUpCoordinate.x >= 0 && leftUpCoordinate.y < m_Grid.Height && !m_Grid.GetNode(leftUpCoordinate).IsOccupied &&
                                 hasLeftNode && hasUpNode;
            bool hasRightDownNode = rightDownCoordinate.y >= 0 && rightDownCoordinate.x < m_Grid.Width && !m_Grid.GetNode(rightDownCoordinate).IsOccupied &&
                                    hasRightNode && hasDownNode;
            bool hasLeftDownNode = leftDownCoordinate.y >= 0 && leftDownCoordinate.x >= 0 && !m_Grid.GetNode(leftDownCoordinate).IsOccupied &&
                                   hasDownNode && hasLeftNode;
            
            if (hasRightNode)
            {
                yield return new Connection(rightCoordinate, 1f);
            }
            
            if (hasLeftNode)
            {
                yield return new Connection(leftCoordinate, 1f);
            }
            
            if (hasUpNode)
            {
                yield return new Connection(upCoordinate, 1f);
            }
            
            if (hasDownNode)
            {
                yield return new Connection(downCoordinate, 1f);
            }

            if (hasRightUpNode)
            {
                yield return new Connection(rightUpCoordinate, (float)Math.Sqrt(2));
            }
            
            if (hasLeftUpNode)
            {
                yield return new Connection(leftUpCoordinate, (float)Math.Sqrt(2));
            }
            
            if (hasRightDownNode)
            {
                yield return new Connection(rightDownCoordinate, (float)Math.Sqrt(2));
            }
            
            if (hasLeftDownNode)
            {
                yield return new Connection(leftDownCoordinate, (float)Math.Sqrt(2));
            }
            
        }

        public bool CanOccupy(Node node)
        {
            Node currentNode = node;
            if (currentNode.m_OccupationAvailability == Node.OccupationAvailability.CanOccupy)
            {
                return true;
            }
            if (currentNode.m_OccupationAvailability == Node.OccupationAvailability.CanNotOccupy)
            {
                return false;
            }
            
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(m_Target);
            
            bool[][] nodes = new bool[m_Grid.Height][];
            
            for (int i = 0; i < m_Grid.Height; i++)
            {
                nodes[i] = new bool[m_Grid.Width];
            }

            for (int i = 0; i < m_Grid.Height; i++)
            {
                for (int j = 0; j < m_Grid.Width; j++)
                {
                    nodes[i][j] = true;
                }
            }

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                foreach (Connection neighbour in GetNeighbours(current))
                {
                    if (nodes[neighbour.Coordinate.x][neighbour.Coordinate.y])
                    {
                        if (m_Start == neighbour.Coordinate)
                        {
                            currentNode.m_OccupationAvailability = Node.OccupationAvailability.CanOccupy;
                            return true;
                        }
                        nodes[neighbour.Coordinate.x][neighbour.Coordinate.y] = false;
                        queue.Enqueue(neighbour.Coordinate);
                    }
                }
            }
            
            currentNode.m_OccupationAvailability = Node.OccupationAvailability.CanNotOccupy;
            return false;
        }
    }
}