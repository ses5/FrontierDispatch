#nullable enable

using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    /// <summary>
    /// Straightforward queries over the supplied WorldData.
    ///
    /// The starter project intentionally performs these queries directly over the stored
    /// collections. The implementation is correct, but students should analyze
    /// how its work scales as the world becomes larger and queries are repeated.
    ///
    /// Do not assume that every method must be replaced. Identify the operations
    /// that matter first, then justify any representation changes.
    /// </summary>
    public sealed class WorldQueries
    {
        private readonly WorldData _world;

        private readonly Dictionary<int, Location> _locationsByID;
        private readonly Dictionary<int, List<Connection>> _connectionsByFromID;

        public WorldQueries(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));

            _locationsByID = new Dictionary<int, Location>();
            _connectionsByFromID = new Dictionary<int, List<Connection>>();

            foreach (Location location in _world.Locations)
            {
                _locationsByID.Add(location.Id, location);
            }

            foreach (Connection connection in _world.Connections)
            {
                if (!_connectionsByFromID.TryGetValue(connection.FromLocationId, out List<Connection>? bucket))
                {
                    bucket = new List<Connection>();
                    _connectionsByFromID.Add(connection.FromLocationId, bucket);
                }
                
                bucket.Add(connection);
            }
        }

        public Location? FindLocationById(int id)
        {
            if (_locationsByID.TryGetValue(id, out Location? location))
            {
                return location;
            }

            return null;
        }

        public IReadOnlyList<Connection> FindOutgoingConnections(int fromLocationId)
        {
            if (_connectionsByFromID.TryGetValue(fromLocationId, out List<Connection>? connections))
            {
                return connections;
            }

            return Array.Empty<Connection>();
        }
    }
}
