using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services
{
    public interface IResearchAreaService
    {
        Task<List<ResearchArea>> GetAllAsync();
        Task<ResearchArea> CreateAsync(string name);
        Task<bool> DeleteAsync(int id);
    }

    public class ResearchAreaService : IResearchAreaService
    {
        private readonly ApplicationDbContext _db;

        public ResearchAreaService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ResearchArea>> GetAllAsync()
            => await _db.ResearchAreas.OrderBy(r => r.Name).ToListAsync();

        public async Task<ResearchArea> CreateAsync(string name)
        {
            var area = new ResearchArea { Name = name };
            _db.ResearchAreas.Add(area);
            await _db.SaveChangesAsync();
            return area;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var area = await _db.ResearchAreas.FindAsync(id);
            if (area == null) return false;
            _db.ResearchAreas.Remove(area);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}