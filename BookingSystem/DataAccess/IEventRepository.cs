using BookingSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess
{
    public interface IEventRepository
    {
        Task<Event> GetByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task<IEnumerable<Event>> GetFutureEventsAsync();
        Task<int> AddAsync(Event eventItem);
        Task UpdateAsync(Event eventItem);
        Task DeleteAsync(int id);
    }
}
