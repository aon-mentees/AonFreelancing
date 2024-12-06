using System.Collections.Concurrent;

namespace AonFreelancing.Services
{
    public class InMemoryUserConnectionService
    {
        readonly ConcurrentDictionary<long, HashSet<string>> _connectionIds = [];

        public void Add(long userId, string connectionId)
        {
            if (!_connectionIds.ContainsKey(userId))
                _connectionIds[userId] = [];
            _connectionIds[userId].Add(connectionId);
        }
        public void Remove(long userId)
        {
            if (_connectionIds.ContainsKey(userId))
                _connectionIds.Remove(userId, out _);
        }
        public void Remove(long userId, string connectionId)
        {
            if (_connectionIds.ContainsKey(userId))
                _connectionIds[userId].Remove(connectionId);
        }

        public HashSet<string>? GetConnections(long userId)
        {
            if (_connectionIds.ContainsKey(userId))
                return _connectionIds[userId];
            return null;
        }
    }
}
