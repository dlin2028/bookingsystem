using BookingSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess.InMemory
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<int, User> _users;
        private int _nextId;

        public InMemoryUserRepository()
        {
            _users = new Dictionary<int, User>();
            _nextId = 1;
            SeedData();
        }

        private void SeedData()
        {
            var users = new[]
            {
                new User(1, "John", "Doe", "john.doe@example.com"),
                new User(2, "Jane", "Smith", "jane.smith@example.com"),
                new User(3, "Robert", "Johnson", "robert.johnson@example.com"),
                new User(4, "Emily", "Williams", "emily.williams@example.com"),
                new User(5, "Michael", "Brown", "michael.brown@example.com")
            };

            foreach (var user in users)
            {
                _users[user.Id] = user;
                _nextId = Math.Max(_nextId, user.Id + 1);
            }
        }

        public Task<User> GetByIdAsync(int id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return Task.FromResult(_users.Values.AsEnumerable());
        }

        public Task<User> GetByEmailAsync(string email)
        {
            var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<int> AddAsync(User user)
        {
            user.Id = _nextId++;
            _users[user.Id] = user;
            return Task.FromResult(user.Id);
        }

        public Task UpdateAsync(User user)
        {
            if (_users.ContainsKey(user.Id))
            {
                _users[user.Id] = user;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            _users.Remove(id);
            return Task.CompletedTask;
        }
    }
}
