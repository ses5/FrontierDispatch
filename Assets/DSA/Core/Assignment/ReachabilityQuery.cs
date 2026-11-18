using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    // Assignment Phase 5. Adapt shortest-path reasoning to a travel-cost budget.

    /// <summary>
    /// Finds locations that can be reached from a starting location without
    /// exceeding a maximum total travel cost.
    ///
    /// This is an adaptation of shortest-path reasoning: instead of searching
    /// for one goal, the query determines the cheapest known cost to every
    /// location that fits within the supplied budget.
    ///
    /// What do we mean by "query"? In this context, a query is simply a one-time search for information.
    /// </summary>
    public sealed class ReachabilityQuery
    {
        private readonly WorldQueries _worldQueries;

        /// <summary>
        /// Creates a reachability query operating on the supplied world.
        /// </summary>
        public ReachabilityQuery(WorldData world)
        {
            _worldQueries = new WorldQueries(world);
        }

        public ReachabilityQuery(WorldQueries worldQueries)
        {
            _worldQueries = worldQueries ?? throw new ArgumentNullException(nameof(worldQueries));
        }

        /// <summary>
        /// Returns reachable location IDs mapped to their cheapest known
        /// travel cost from the starting location.
        ///
        /// Only locations whose cheapest cost is within maxTravelCost should
        /// be included. The starting location has cost 0.
        /// </summary>
        public IReadOnlyDictionary<int, int> FindReachable(int startLocationId, int maxTravelCost)
        {
            var bestCost = new Dictionary<int, int>();
            var frontier = new MinPriorityQueue<(int Id, int Cost)>();

            bestCost[startLocationId] = 0;
            frontier.Enqueue((startLocationId, 0), 0);

            while (frontier.Count > 0)
            {
                var node = frontier.Dequeue();
                
                var current = node.Id;
                var priority = node.Cost;

                if (priority > bestCost[current])
                    continue;
                
                foreach (Connection connection in _worldQueries.FindOutgoingConnections(current))
                {
                    var candidate = bestCost[current] + connection.TravelCost;

                    if (!bestCost.ContainsKey(connection.ToLocationId) || candidate < bestCost[connection.ToLocationId])
                    {
                        if (candidate <= maxTravelCost)
                        {
                            bestCost[connection.ToLocationId] = candidate;
                            frontier.Enqueue((connection.ToLocationId, candidate), candidate);
                        }
                    }
                }

            }

            return bestCost;
        }
    }
}
