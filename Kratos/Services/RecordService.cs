using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Kratos.Data;

namespace Kratos.Services
{
    public class RecordService
    {
        private RecordContext _db;

        public async Task<Mute> AddMuteAsync(Mute mute)
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            var entry = await _db.Mutes.AddAsync(mute);

            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<PermaBan> AddPermaBanAsync(PermaBan ban)
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            var entry = await _db.PermaBans.AddAsync(ban);

            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<TempBan> AddTempBanAsync(TempBan ban)
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            var entry = await _db.TempBans.AddAsync(ban);

            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<SoftBan> AddSoftBanAsync(SoftBan ban)
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            var entry = await _db.SoftBans.AddAsync(ban);

            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task DeactivateBanAsync(int key)
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            var ban = await _db.TempBans.FirstOrDefaultAsync(x => x.Key == key);
            ban.Active = false;
            await _db.SaveChangesAsync();
        }

        public async Task DeactivateMuteAsync(int key)
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            var mute = await _db.Mutes.FirstOrDefaultAsync(x => x.Key == key);
            mute.Active = false;
            await _db.SaveChangesAsync();
        }

        public async Task DeactivateMutesForUserAsync(ulong id)
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            var mutes = _db.Mutes.OrderByDescending(x => x.Timestamp); // Oldest first
            var userMutes = mutes.Where(x => x.SubjectId == id);
            foreach (var mute in userMutes)
                mute.Active = false;
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<TempBan>> GetActiveTempBansAsync()
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            return _db.TempBans.Where(x => DateTime.Compare(DateTime.UtcNow, x.UnbanAt) > 0 && x.Active);
        }

        public async Task<IEnumerable<Mute>> GetActiveMutesAsync()
        {
            if (_db == null)
                _db = new RecordContext();
            await _db.Database.EnsureCreatedAsync();
            return _db.Mutes.Where(x => DateTime.Compare(DateTime.UtcNow, x.UnmuteAt) > 0 && x.Active);
        }

        public void DisposeContext()
        {
            _db.Dispose();
            _db = null;
        }
    }
}
