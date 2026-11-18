using System;
using System.Collections.Generic;
using NUnit.Framework;
using FrontierDispatch.Core;

namespace FrontierDispatch.Tests
{
    /// <summary>
    /// Small contract examples for later assignment phases.
    ///
    /// These are not an exhaustive assignment test suite.
    /// Students are expected to enable the relevant tests as they reach each
    /// phase and add meaningful tests of their own, including edge cases.
    /// </summary>
    public sealed class AssignmentStarterTests
    {
        // These tests document later assignment contracts without making the
        // untouched baseline red. Remove Ignore when you reach the named phase.

        [Test]
        public void PriorityQueue_EnqueueAndDequeue_ReturnsLowestPriorityFirst()
        {
            var queue = new MinPriorityQueue<string>();
            queue.Enqueue("expensive", 9);
            queue.Enqueue("cheap", 2);
            queue.Enqueue("medium", 5);
            Assert.That(queue.Dequeue(), Is.EqualTo("cheap"));
        }

        [Test]
        public void PriorityQueue_DequeueAll_ReturnsItemsInAscendingPriorityOrder()
        {
            var queue = new MinPriorityQueue<int>();
            int[] priorities = { 7, 3, 9, 1, 5, 3, 8, 2, 6, 4, 0, 7 };

            foreach (int priority in priorities)
            {
                queue.Enqueue(priority, priority);
            }

            var result = new int[priorities.Length];
            for (int i = 0; i < result.Length; i++) 
                result[i] = queue.Dequeue();
            
            int[] expected = { 0, 1, 2, 3, 3, 4, 5, 6, 7, 7, 8, 9 };
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(queue.Count, Is.EqualTo(0));
        }

        [Test]
        public void Dijkstra_StartEqualsGoal_ReturnsZeroCostRoute()
        {
            var finder = new DijkstraRouteFinder(WorldDatasetFactory.CreateSmall());
            RouteResult result = finder.FindRoute(0, 0);
            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void Dijkstra_PrefersCheaperRouteWithMoreSteps()
        {
            // 0 -> 2 directly costs 10, but 0 -> 1 -> 2 costs 2 + 3 = 5.
            var locations = new[]
            {
                new Location(0, "A", new WorldPosition(0, 0), "Town"),
                new Location(1, "B", new WorldPosition(1, 0), "Town"),
                new Location(2, "C", new WorldPosition(2, 0), "Town"),
            };
            var connections = new[]
            {
                new Connection(0, 2, 10),
                new Connection(0, 1, 2),
                new Connection(1, 2, 3),
            };
            var finder = new DijkstraRouteFinder(new WorldData(locations, connections));

            RouteResult result = finder.FindRoute(0, 2);

            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(5));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void Dijkstra_UnreachableGoal_ReturnsNotFound()
        {
            // 0 -> 1 exists, but nothing leads to 2. Connections are directed,
            // so the edge 2 -> 0 does not make 2 reachable from 0.
            var locations = new[]
            {
                new Location(0, "A", new WorldPosition(0, 0), "Town"),
                new Location(1, "B", new WorldPosition(1, 0), "Town"),
                new Location(2, "C", new WorldPosition(2, 0), "Town"),
            };
            var connections = new[]
            {
                new Connection(0, 1, 1),
                new Connection(2, 0, 1),
            };
            var finder = new DijkstraRouteFinder(new WorldData(locations, connections));

            RouteResult result = finder.FindRoute(0, 2);

            Assert.That(result.Found, Is.False);
            Assert.That(result.LocationIds, Is.Empty);
        }

        [Test]
        public void Reachability_ZeroBudget_IncludesStartAtZeroCost()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, 0);
            Assert.That(result.ContainsKey(0), Is.True);
            Assert.That(result[0], Is.EqualTo(0));
        }

        // Shared world for the reachability tests below.
        //
        //   0 --2--> 1 --2--> 2        Cheapest costs from 0:
        //   0 --------10----> 2          0: 0
        //            1 --3--> 3          1: 2
        //   4 --1--> 0                   2: 4 (via 1, not the direct 10 edge)
        //                                3: 5
        //                                4: unreachable (only has an edge *out*)
        private static ReachabilityQuery CreateReachabilityWorld()
        {
            var locations = new[]
            {
                new Location(0, "A", new WorldPosition(0, 0), "Town"),
                new Location(1, "B", new WorldPosition(1, 0), "Town"),
                new Location(2, "C", new WorldPosition(2, 0), "Town"),
                new Location(3, "D", new WorldPosition(1, 1), "Town"),
                new Location(4, "E", new WorldPosition(5, 5), "Town"),
            };
            var connections = new[]
            {
                new Connection(0, 1, 2),
                new Connection(1, 2, 2),
                new Connection(0, 2, 10),
                new Connection(1, 3, 3),
                new Connection(4, 0, 1),
            };
            return new ReachabilityQuery(new WorldData(locations, connections));
        }

        [Test]
        public void Reachability_ZeroBudget_ReturnsOnlyStart()
        {
            var result = CreateReachabilityWorld().FindReachable(0, 0);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(0));
        }

        [Test]
        public void Reachability_MultipleRoutes_UsesCheapestCost()
        {
            // Location 2 can be reached directly (10) or via 1 (2 + 2 = 4).
            var result = CreateReachabilityWorld().FindReachable(0, 100);

            Assert.That(result[2], Is.EqualTo(4));
        }

        [Test]
        public void Reachability_IsBasedOnTotalCost_NotEdgeCount()
        {
            // Budget 3 reaches 1 (cost 2) but not 2, even though 2 is only one edge away.
            var result = CreateReachabilityWorld().FindReachable(0, 3);

            Assert.That(result.ContainsKey(1), Is.True);
            Assert.That(result.ContainsKey(2), Is.False);
        }

        [Test]
        public void Reachability_LocationCostingExactlyBudget_IsIncluded()
        {
            // 2 costs exactly 4 and must be included; 3 costs 5 and must not.
            var result = CreateReachabilityWorld().FindReachable(0, 4);

            Assert.That(result, Is.EquivalentTo(new Dictionary<int, int>
            {
                [0] = 0,
                [1] = 2,
                [2] = 4,
            }));
        }

        [Test]
        public void Reachability_DisconnectedLocation_IsNeverIncluded()
        {
            // 4 has an edge into 0, but no edge leads to 4, so even a huge budget cannot reach it.
            var result = CreateReachabilityWorld().FindReachable(0, 1000);

            Assert.That(result.ContainsKey(4), Is.False);
            Assert.That(result.Count, Is.EqualTo(4));
        }

        [Test]
        public void Dijkstra_SharedWorldQueries_GivesSameRoutesAsOwnWorldQueries()
        {
            // Several finders share one prepared WorldQueries. Each must return
            // exactly the same route as a finder that builds its own.
            WorldData world = WorldDatasetFactory.CreateMedium();
            var sharedQueries = new WorldQueries(world);
            int count = world.Locations.Count;

            for (int i = 0; i < 20; ++i)
            {
                int start = (i * 997) % count;
                int goal = (i * 7919 + 1234) % count;

                RouteResult expected = new DijkstraRouteFinder(world).FindRoute(start, goal);
                RouteResult actual = new DijkstraRouteFinder(sharedQueries).FindRoute(start, goal);

                Assert.That(actual.Found, Is.EqualTo(expected.Found));
                Assert.That(actual.TotalCost, Is.EqualTo(expected.TotalCost));
                Assert.That(actual.LocationIds, Is.EqualTo(expected.LocationIds));
            }
        }

        [Test]
        public void Reachability_SharedWorldQueries_GivesSameResultAsOwnWorldQueries()
        {
            WorldData world = WorldDatasetFactory.CreateMedium();
            var sharedQueries = new WorldQueries(world);

            var expected = new ReachabilityQuery(world).FindReachable(0, 30);
            var actual = new ReachabilityQuery(sharedQueries).FindReachable(0, 30);

            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void SharedWorldQueriesConstructors_RejectNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DijkstraRouteFinder((WorldQueries)null!));
            Assert.Throws<ArgumentNullException>(() => new ReachabilityQuery((WorldQueries)null!));
        }
    }
}
