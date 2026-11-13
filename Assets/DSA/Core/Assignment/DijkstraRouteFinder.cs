using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    // Assignment Phase 4.

    /// <summary>
    /// Finds minimum-total-cost routes through a Frontier Dispatch world
    /// using Dijkstra's shortest-path algorithm.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    /// It should eventually use the student's prepared world representation
    /// and MinPriorityQueue implementation.
    /// </summary>
    public sealed class DijkstraRouteFinder
    {
        private readonly WorldData _world;
        private readonly WorldQueries _worldQueries;
        
        /// <summary>
        /// Constructor. Creates a route finder operating on the supplied world.
        /// </summary>
        public DijkstraRouteFinder(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _worldQueries = new WorldQueries(world);
        }

        /// <summary>
        /// Finds a minimum-total-travel-cost route from the start location
        /// to the goal location.
        ///
        /// A successful RouteResult should contain the route as location IDs,
        /// including both the start and goal locations.
        /// </summary>
        public RouteResult FindRoute(int startLocationId, int goalLocationId)
        {
            // TODO: implement Dijkstra using your prepared world representation
            // and MinPriorityQueue<T>.

            if (startLocationId == goalLocationId)
            { 
                return new RouteResult(found: true, locationIds: new []{goalLocationId}, totalCost: 0, expandedCount: 0);
            }

            var bestCost = new Dictionary<int, int>();
            var cameFrom = new Dictionary<int, int>();
            var frontier = new MinPriorityQueue<(int Id, int Cost)>();

            bestCost[startLocationId] = 0;
            frontier.Enqueue((startLocationId, 0), 0);

            int expandedNodes = 0;

            while (frontier.Count > 0)
            {
                var node = frontier.Dequeue();
                
                var current = node.Id;
                var priority = node.Cost;

                if (priority > bestCost[current])
                    continue;

                expandedNodes++;

                if (current == goalLocationId)
                {
                    List<int> wayToGoal = new List<int>();

                    wayToGoal.Add(goalLocationId);

                    var step = goalLocationId;

                    while (step != startLocationId)
                    {
                        step = cameFrom[step];

                        wayToGoal.Add(step);
                    }

                    wayToGoal.Reverse();
                    return new RouteResult(found: true, locationIds: wayToGoal, totalCost: priority, expandedCount: expandedNodes);
                }

                foreach (Connection connection in _worldQueries.FindOutgoingConnections(current))
                {
                    var candidate = bestCost[current] + connection.TravelCost;

                    if (!bestCost.ContainsKey(connection.ToLocationId) || candidate < bestCost[connection.ToLocationId])
                    {
                        bestCost[connection.ToLocationId] = candidate;
                        cameFrom[connection.ToLocationId] = current;
                        frontier.Enqueue((connection.ToLocationId, candidate), candidate);
                    }
                }

            }

            return new RouteResult(found: false, locationIds: Array.Empty<int>(), totalCost: 0, expandedCount: expandedNodes);
        }
    }
}