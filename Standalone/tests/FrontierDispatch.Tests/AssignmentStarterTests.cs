using System;
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

        [Test, Ignore("Phase 4: enable when implementing Dijkstra route finding.")]
        public void Dijkstra_StartEqualsGoal_ReturnsZeroCostRoute()
        {
            var finder = new DijkstraRouteFinder(WorldDatasetFactory.CreateSmall());
            RouteResult result = finder.FindRoute(0, 0);
            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0 }));
        }

        [Test, Ignore("Phase 5: enable when implementing bounded reachability.")]
        public void Reachability_ZeroBudget_IncludesStartAtZeroCost()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, 0);
            Assert.That(result.ContainsKey(0), Is.True);
            Assert.That(result[0], Is.EqualTo(0));
        }
    }
}
