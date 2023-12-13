using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoomRental.Data;
using RoomRental.Models;

namespace RoomRental.Services
{
    public abstract class CachedService<T>
    {
        protected RoomRentalsContext _context;
        protected static IMemoryCache _cache;
        protected string _name;
        protected readonly User _user;
        public CachedService(IMemoryCache cache, RoomRentalsContext context, string name, User user)
        {
            _cache = cache;
            _context = context;
            _name = name;
            _user = user;
        }

        public async virtual Task<List<T>> GetAll()
        {
            if (!_cache.TryGetValue(_name, out List<T>? entities))
            {
                entities = await UpdateCache();
            }
            else
            {
                Console.WriteLine($"Список извлечен из кэша");
            }
            return entities;
        }
        public abstract Task<T> Get(int? id);
        public async virtual Task Add(T entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            await UpdateCache();
        }
        public async virtual Task Update(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
            await UpdateCache();
        }
        public abstract Task Delete(T entity);
        public abstract Task<List<T>> UpdateCache();
    }
}
