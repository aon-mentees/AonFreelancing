using System.Collections.Concurrent;


namespace AonFreelancing.Services
{
    public class InMemoryUserConnectionService
    {
         readonly ConcurrentDictionary<string, List<string>> _connectionIds = [];

        public void Add(string userId, string connectionId)
        {
            if (!_connectionIds.ContainsKey(userId))
                _connectionIds[userId] = [];
            _connectionIds[userId].Add(connectionId);
        }
        public void Remove(string userId) 
        {
            if (_connectionIds.ContainsKey(userId))
                _connectionIds.Remove(userId,out _);
        }
        public void Remove(string userId,string connectionId) 
        {
            if (_connectionIds.ContainsKey(userId))
                _connectionIds[userId].Remove(connectionId);
        }

        public List<string>GetConnections(string userId)
        {
            if (_connectionIds.ContainsKey(userId))
                return _connectionIds[userId];
            return null;
        }
    }
}
