using System.Linq;
using NUnit.Framework;
using Pathfinding.Base;
using Pathfinding.Configs;
using Pathfinding.Enums;
using Pathfinding.MonoBehaviours;
using Pathfinding.MonoBehaviours.Agent;
using Pathfinding.Tests;
using UnityEngine;

namespace Pathfinding.Editor.Tests
{
    public class PathfindingMechanicsTests
    {
        private AStarPathfindingMechanics _mechanics;
        private AStarPathfindingSettingsConfig _pathfindingSettings;
        private ScanSettings _scanSettings;

        private const float NodeSize = 12;
        
        private static readonly AStarNode[,] Grid1 = GridTestHelper.CreateGrid(
            new[,]
            {
                { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 2, 2, 2, 2, 0 },
                { 2, 1, 2, 2, 1, 2, 0, 0, 0, 0 },
                { 0, 0, 0, 2, 0, 2, 0, 0, 0, 0 },
                { 0, 0, 0, 2, 0, 2, 1, 0, 0, 0 },
                { 0, 0, 0, 2, 0, 2, 2, 2, 2, 0 },
                { 0, 0, 0, 2, 0, 1, 2, 0, 0, 0 },
                { 0, 0, 0, 2, 0, 0, 2, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            40,
            NodeSize);
        
        
        [SetUp]
        public void Init()
        {
            _scanSettings = new ScanSettings()
            {
                CheckingMode = CheckingMode.Circle,
                NodeSize = NodeSize,
                ObstaclesLayerMask = 2,
                ScanBounds = new Bounds(),
                WeightEnvironmentLayerMask = 1
            };

            _pathfindingSettings = ConfigsTestHelper.CreateConfig<AStarPathfindingSettingsConfig>();
            _pathfindingSettings.HeuristicMultiplier = 1;
            _pathfindingSettings.DefaultWeightInfluenceRatio = 0.5f;

            _mechanics = new AStarPathfindingMechanics(_pathfindingSettings, _scanSettings);
        }

        [Test]
        public void TestGridByLength()
        {
            var agentFake = new AStarAgentFake();
            _mechanics.SetGrid(Grid1, NodeSize);
            var path = _mechanics.GetMinimumPath(
                new Vector2(1 * NodeSize, 7 * NodeSize),
                new Vector2(1 * NodeSize, 1 * NodeSize),
                agentFake);
            
            //TODO: check test
            Assert.IsTrue(path.Count == 9);
            //check path for weightables nodes count == 0
            Assert.IsFalse(path.Any(node => node.Weightable));
            
            path = _mechanics.GetMinimumPath(
                new Vector2(4 * NodeSize, 1 * NodeSize),
                new Vector2(1 * NodeSize, 1 * NodeSize),
                agentFake);
        }

        [Test]
        public void TestGridByWeightables()
        {
            var agentFake = new AStarAgentFake();
            _mechanics.SetGrid(Grid1, NodeSize);
            var path = _mechanics.GetMinimumPath(
                new Vector2(1 * NodeSize, 7 * NodeSize),
                new Vector2(1 * NodeSize, 1 * NodeSize),
                agentFake);
            
            //check path for weightables nodes count == 0
            Assert.IsFalse(path.Any(node => node.Weightable));
            
            path = _mechanics.GetMinimumPath(
                new Vector2(4 * NodeSize, 1 * NodeSize),
                new Vector2(1 * NodeSize, 1 * NodeSize),
                agentFake);
        }

        [TestCase]
        public void TestGetNearestNode()
        {
            var grid = GridTestHelper.CreateGrid(new[,]
                {
                    {0, 0, 2, 0, 0},
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 1, 0},
                    {0, 0, 0, 2, 0},
                }, 20, 10);
            _mechanics.SetGrid(grid, 10);

            var desireNode1 = grid[0, 2];
            var node1 = _mechanics.GetNearestNode(new Vector2(2, 17));
            var node4 = _mechanics.GetNearestNode(new Vector2(-2, 21));
            Assert.IsTrue(node1 == desireNode1);
            Assert.IsTrue(node4 == desireNode1);

            var desireNode2 = grid[2, 3];
            var node2 = _mechanics.GetNearestNode(new Vector2(24, 32));
            Assert.IsTrue(node2 == desireNode2);
            
            var desireNode3 = grid[3, 3];
            var node3 = _mechanics.GetNearestNode(new Vector2(36, 29));
            var node5 = _mechanics.GetNearestNode(new Vector2(31, 29));
            Assert.IsFalse(node3 == desireNode3);
            Assert.IsTrue(node5 == desireNode3);
        }
    }
}