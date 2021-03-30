using System;
using Field;
using UnityEngine;
using Grid = Field.Grid;

namespace Enemy
{
    public class FlyingMovementAgent  : IMovementAgent
    {
        private float m_Speed;
        private Transform m_Transform;
        private Grid m_Grid;
        private EnemyData m_EnemyData;
        private Node m_currentNode;

        public FlyingMovementAgent (float speed, Transform transform, Grid grid, EnemyData enemyData)
        {
            m_Speed = speed;
            m_Transform = transform;
            m_EnemyData = enemyData;
            m_Grid = grid;

            SetTargetNode(grid.GetTargetNode());
            m_TargetNode.EnemyDatas.Add(m_EnemyData);
            m_currentNode = m_TargetNode;
        }

        private const float TOLERANCE = 0.1f;

        private Node m_TargetNode;

        public void TickMovement()
        {
            if (m_TargetNode == null)
            {
                return;
            }

            Vector3 target = new Vector3(m_TargetNode.Position.x, m_Transform.position.y, m_TargetNode.Position.z);
            
            Node current_node = m_Grid.GetNodeAtPoint(m_Transform.position);
            if (current_node != m_currentNode)
            {
                m_currentNode.EnemyDatas.Remove(m_EnemyData);
                m_currentNode = current_node;
                m_currentNode.EnemyDatas.Add(m_EnemyData);
            }
            
            float distance = (target - m_Transform.position).magnitude;
            if (distance < TOLERANCE)
            {
                m_TargetNode = m_TargetNode.NextNode;
                return;
            }
        
            Vector3 dir = (target - m_Transform.position).normalized;
            Vector3 delta = dir * (m_Speed * Time.deltaTime);
            m_Transform.Translate(delta);
        }

        private void SetTargetNode(Node node)
        {
            m_TargetNode = node;
        }
    }
}