using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Kratos.Data.Contexts;
using Kratos.Data.Models;

namespace Kratos.Services
{
    public class TagService
    {
        private TagContext _db;

        public async Task<bool> TryAddTagAsync(TagValue entity)
        {
            if (_db == null)
                _db = new TagContext();
            await _db.Database.EnsureCreatedAsync();
            if (await _db.Tags.AnyAsync(x => x.Tag == entity.Tag)) return false;
            await _db.Tags.AddAsync(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TryRemoveTagAsync(string tag)
        {
            if (_db == null)
                _db = new TagContext();
            await _db.Database.EnsureCreatedAsync();
            var toRemove = await _db.Tags.FirstOrDefaultAsync(x => x.Tag == tag);
            if (toRemove == null) return false;
            _db.Remove(toRemove);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<TagValue> GetTagAsync(string tag, bool invoke)
        {
            if (_db == null)
                _db = new TagContext();
            await _db.Database.EnsureCreatedAsync();
            var toReturn = _db.Tags.FirstOrDefault(x => x.Tag == tag);
            if (!invoke) return toReturn;
            toReturn.TimesInvoked++;
            await _db.SaveChangesAsync();
            return toReturn;
        }

        public async Task<IEnumerable<TagValue>> GetTagsAsync()
        {
            if (_db == null)
                _db = new TagContext();
            await _db.Database.EnsureCreatedAsync();
            return _db.Tags;
        }

        public void DisposeContext()
        {
            _db.Dispose();
            _db = null;
        }
    }
}
