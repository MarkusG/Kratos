using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Kratos.Data;

namespace Kratos.Services
{
    public class UsernoteService
    {
        private UsernoteContext _db;

        public async Task AddNoteAsync(ulong userId, ulong authorId, ulong timestamp, string content)
        {
            if (_db == null)
                _db = new UsernoteContext();
            await _db.Database.EnsureCreatedAsync();
            await _db.Notes.AddAsync(new Usernote
            {
                SubjectId = userId,
                AuthorId = authorId,
                UnixTimestamp = timestamp,
                Content = content
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Usernote>> GetNotesForUserAsync(ulong userId)
        {
            if (_db == null)
                _db = new UsernoteContext();
            await _db.Database.EnsureCreatedAsync();
            return _db.Notes.Where(x => x.SubjectId == userId);
        }

        public async Task<Usernote> GetNoteAsync(int key)
        {
            if (_db == null)
                _db = new UsernoteContext();
            await _db.Database.EnsureCreatedAsync();
            return await _db.Notes.FirstOrDefaultAsync(x => x.Key == key);
        }

        public async Task<bool> RemoveNoteAsync(int key)
        {
            if (_db == null)
                _db = new UsernoteContext();
            await _db.Database.EnsureCreatedAsync();
            if (!await _db.Notes.AnyAsync(x => x.Key == key))
                return false;
            _db.Notes.Remove(await _db.Notes.FirstOrDefaultAsync(x => x.Key == key));
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ClearNotesAsync(ulong userId)
        {
            if (_db == null)
                _db = new UsernoteContext();
            await _db.Database.EnsureCreatedAsync();
            var toRemove = _db.Notes.Where(x => x.SubjectId == userId);
            _db.Notes.RemoveRange(toRemove);
            await _db.SaveChangesAsync();
        }

        public void DisposeContext()
        {
            _db.Dispose();
            _db = null;
        }
    }
}
