using System;
using System.IO;
using System.Linq;
using Cubes.Core.Base;
using LiteDB;

namespace Cubes.Core.Utilities
{
    public class LocalStorage : ILocalStorage
    {
        private class LocaStorageItem
        {
            public int ID { get; set; }
            public string Key { get; set; }
            public DateTime AddedAt { get; set; }
            public object Value { get; set; }
        }

        private readonly string dbPath;

        public LocalStorage(string rootFolder)
            => this.dbPath = Path.Combine(rootFolder, CubesConstants.LocalStorage_File);

        public object Get(string key)
        {
            using (var db = new LiteDatabase(this.dbPath))
            {
                var collection = db.GetCollection<LocaStorageItem>();
                var existing = collection
                    .Find(i => i.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                return existing?.Value;
            }
        }

        public T Get<T>(string key) where T : class
            => Get(key) as T;

        public void Save<T>(string key, T value)
        {
            using (var db = new LiteDatabase(this.dbPath))
            {
                var collection = db.GetCollection<LocaStorageItem>();
                var existing = collection
                    .Find(i => i.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (existing != null)
                    collection.Delete(existing.ID);

                var item = new LocaStorageItem
                {
                    AddedAt = DateTime.Now,
                    Key = key,
                    Value = value
                };
                collection.Insert(item);
                collection.EnsureIndex(nameof(key), true);
            }
        }

        public void Clear(string key)
        {
            using (var db = new LiteDatabase(this.dbPath))
            {
                var collection = db.GetCollection<LocaStorageItem>();
                var existing = collection
                    .Find(i => i.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (existing != null)
                    collection.Delete(existing.ID);
            }
        }
    }
}
