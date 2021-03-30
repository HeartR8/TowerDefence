using Field;
using Runtime;
using Turret;
using UnityEngine;
using Grid = Field.Grid;

namespace TurretSpawn
{
    public class TurretSpawnController: IController
    {
        private Grid m_Grid;
        private TurretMarket m_Market;

        public TurretSpawnController(Grid grid, TurretMarket turretMarket)
        {
            m_Grid = grid;
            m_Market = turretMarket;
        }
        public void OnStart()
        {
        }

        public void OnStop()
        {
        }

        public void Tick()
        {
            if (m_Grid.HasSelectedNode() && Input.GetMouseButtonDown(0))
            {
                Node selectedNode = m_Grid.GetSelectedNode();
                bool canOccupy = false;
                m_Grid.TryOccupyNode(selectedNode, ref canOccupy);
                if (canOccupy)
                {
                    SpawnTurret(m_Market.ChosenTurret, selectedNode);
                    m_Grid.UpdatePathfinding();
                }
            }
        }

        public void SpawnTurret(TurretAsset asset, Node node)
        {
            TurretView view = Object.Instantiate(asset.ViewPrefab);
            TurretData data = new TurretData(asset, node);
            
            data.AttachView(view);

            node.IsOccupied = true; 
            m_Grid.UpdatePathfinding();
        }
    }
}