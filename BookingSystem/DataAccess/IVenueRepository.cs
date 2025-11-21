using BookingSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.DataAccess
{
    public interface IVenueRepository
    {
        Task<Venue> GetByIdAsync(int id);
        Task<IEnumerable<Venue>> GetAllAsync();
        Task<int> AddAsync(Venue venue);
        Task UpdateAsync(Venue venue);
        Task DeleteAsync(int id);
    }
}
