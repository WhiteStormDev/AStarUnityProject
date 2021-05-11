using System.Collections.Generic;
using Pathfinding.Base;
using Pathfinding.Configs;
using Pathfinding.MonoBehaviours.Agent;
using UnityEngine;

namespace Pathfinding.MonoBehaviours
{
    public class AStarPathfinding : MonoBehaviour
    {
        [SerializeField] 
        private AStarPathfindingSettingsConfig _settingsConfig;

        [SerializeField] 
        private ScanSettings _scanSettings;
        
        public static AStarPathfinding Instance { get; private set; }

        public List<AStarPathNode> LastPath =>
            _pathfindingMechanics == null ? new List<AStarPathNode>() : _pathfindingMechanics.LastPath;

        public int ClosedSetCount => _pathfindingMechanics?.ClosetSetCount ?? 0;
        public int OpenSetCount => _pathfindingMechanics?.OpenSetCount ?? 0;
        
        public AStarPathfindingSettingsConfig SettingsConfig => _settingsConfig;

        private AStarPathfindingMechanics _pathfindingMechanics;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Scan();
        }
        
        private bool TryGetClampedScanBounds(Bounds bounds, out Bounds clampedBounds)
        {
            clampedBounds = new Bounds();
            
            if (_scanSettings.NodeSize > 0)
            {
                var x = bounds.extents.x - bounds.center.x;
                var y = bounds.extents.y - bounds.center.y;
                clampedBounds.center = bounds.center + transform.position;

                var clampX = x - x % _scanSettings.NodeSize;
                var clampY = y - y % _scanSettings.NodeSize;
                clampedBounds.extents = new Vector3(clampX, clampY);
                return true;
            }

            Debug.LogError("[AStarPathfinding] NodeSize must be positive and not 0");
            return false;
        }

        private void Scan()
        {
            if (_pathfindingMechanics == null)
                _pathfindingMechanics = new AStarPathfindingMechanics(_settingsConfig, _scanSettings);
            
            if (TryGetClampedScanBounds(_scanSettings.ScanBounds, out var clampedBounds));
            {
                _pathfindingMechanics.Scan(clampedBounds);
            }
        }

        public List<AStarNode> GetPath(Vector2 from, Vector2 to, IAStarAgent agent)
        {
            if (_pathfindingMechanics == null)
            {
                Debug.LogError("[AStarPathfinding] Path not found ERROR");
                return new List<AStarNode>();
            }

            return _pathfindingMechanics.GetMinimumPath(from, to, agent);
        }
        
#if UNITY_EDITOR
        [ContextMenu("Scan")]
        private void Test()
        {
            Scan();
        }

        private void DrawGrid()
        {
            if (_pathfindingMechanics == null)
                return;

            var grid = _pathfindingMechanics.Grid;
            var damagers = new List<AStarNode>();
            var walls = new List<AStarNode>();
            var walkables = new List<AStarNode>();

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    var node = grid[i, j];
                    if (node == null)
                        break;

                    if (node.Weightable)
                        damagers.Add(node);
                    if (node.Walkable)
                        walkables.Add(node);
                    else
                        walls.Add(node);
                }
            }

            Gizmos.color = Color.gray;
            walkables.ForEach(w => Gizmos.DrawWireCube(w.Center, new Vector3(_scanSettings.NodeSize, _scanSettings.NodeSize, 0)));

            Gizmos.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
            walls.ForEach(w => Gizmos.DrawCube(w.Center, new Vector3(_scanSettings.NodeSize, _scanSettings.NodeSize, 0)));

            Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.3f);
            damagers.ForEach(d => Gizmos.DrawCube(d.Center, new Vector3(_scanSettings.NodeSize, _scanSettings.NodeSize, 0)));
        }

        private void DrawBounds(Bounds bounds, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        
        // private void OnValidate()
        // {
        //     if (NodeSize > 0)
        //     {
        //         var x = ScanBounds.extents.x - ScanBounds.center.x;
        //         var y = ScanBounds.extents.y - ScanBounds.center.y;
        //         _clampedScanBounds.center = ScanBounds.center + transform.position;
        //
        //         var clampX = x - x % NodeSize;
        //         var clampY = y - y % NodeSize;
        //         _clampedScanBounds.extents = new Vector3(clampX, clampY);
        //     }
        // }

        private void OnDrawGizmosSelected()
        {
            if (TryGetClampedScanBounds(_scanSettings.ScanBounds, out var clamped));
                DrawBounds(clamped, Color.blue);

            if (_pathfindingMechanics == null)
                return;
            
            DrawGrid();
        }
        
        private void OnDrawGizmos()
        {
            if (_pathfindingMechanics == null)
                return;
            
            var grid = _pathfindingMechanics.Grid;
            var closedSet = _pathfindingMechanics.ClosedSet;
            var openSet = _pathfindingMechanics.OpenSet;
            
            Gizmos.color = new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.8f);

            closedSet?.ForEach(cs =>
            {
                if (cs.Position.x < grid.GetLength(0) && cs.Position.y < grid.GetLength(1))
                {
                    var node = grid[cs.Position.x, cs.Position.y];
                    Gizmos.DrawSphere(node.Center, _scanSettings.NodeSize / 4);
                } 
            });
            Gizmos.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.8f);
            openSet?.ForEach(os =>
            {
                if (os.Position.x < grid.GetLength(0) && os.Position.y < grid.GetLength(1))
                {
                    var node = grid[os.Position.x, os.Position.y];
                    Gizmos.DrawSphere(node.Center, _scanSettings.NodeSize / 4);
                }
            });
        }
#endif
    }
}