using System;
using System.IO;
using Cubes.Core.Base;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Security
{
    public class SecurityStorage : IDisposable
    {
        private readonly LiteDatabase _storage;
        private bool _disposedValue;

        public SecurityStorage(IOptions<CubesConfiguration> configuration)
        {
            var dbPath = Path.Combine(configuration.Value.StorageFolder, CubesConstants.Authentication_Persistence);
            _storage = new LiteDatabase(dbPath);
        }

        public ILiteCollection<Role> Roles => _storage.GetCollection<Role>();
        public ILiteCollection<User> Users => _storage.GetCollection<User>();

        #region IDisposable implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                    _storage.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
