using System.Collections.Concurrent;

namespace AonFreelancing.Services
{
    public class InMemorySignalRUserConnectionService
    {
        readonly ConcurrentDictionary<long, HashSet<string>> _connections = [];

        public void Add(long userId, string connectionId)
        {
            if (!_connections.ContainsKey(userId))
                _connections[userId] = [];
            _connections[userId].Add(connectionId);
        }
        public void Remove(long userId)
        {
            if (_connections.ContainsKey(userId))
                _connections.Remove(userId, out _);
        }
        public void Remove(long userId, string connectionId)
        {
            if (_connections.ContainsKey(userId))
                _connections[userId].Remove(connectionId);
        }

        public HashSet<string>? GetConnections(long userId)
        {
            if (_connections.ContainsKey(userId))
                return _connections[userId];
            return null;
        }
    }
}
