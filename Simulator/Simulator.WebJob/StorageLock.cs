using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob
{
    class StorageLock
    {
        private const string TABLE_PREFIX = "LockTable";
        private const string ENTRY_KEY = "Lock";

        private class Entity : TableEntity
        {
            public Entity()
            {
                this.PartitionKey = ENTRY_KEY;
                this.RowKey = ENTRY_KEY;
            }

            public Guid id { get; set; }
        }

        private string _connectionString;
        private string _name;
        private TimeSpan _heartbeat;
        private TimeSpan _timeout;
        private CancellationToken _cancellationToken;

        private Guid _id;
        private CloudTable _table;
        private Entity _entity;

        public StorageLock(string connectionString, string name, TimeSpan heartbeat, TimeSpan timeout, CancellationToken cancellationToken)
        {
            _connectionString = connectionString;
            _name = name;
            _heartbeat = heartbeat;
            _timeout = timeout;
            _cancellationToken = cancellationToken;

            _id = Guid.NewGuid();
        }

        public bool Acquire()
        {
            if (_table == null)
            {
                _table = AzureTableStorageHelper.GetTableAsync(_connectionString, TABLE_PREFIX + _name).Result;
            }

            var acquired = false;
            try
            {
                // Since we need to block execution on this thread until we obtain the lock, this method is synchronous
                while (!_cancellationToken.IsCancellationRequested && !(acquired = Renew().Result))
                {
                    Trace.TraceInformation("{0} awaiting lock '{1}'", _id.ToString(), _name);
                    Task.Delay(_heartbeat).Wait(_cancellationToken);
                }
            }
            catch (Exception) { }
            if (acquired)
            {
                Trace.TraceInformation("{0} acquired lock '{1}'", _id.ToString(), _name);
            }
            return acquired;
        }

        public async Task<bool> Renew()
        {
            try
            {
                _entity = (await _table.ExecuteAsync(TableOperation.Retrieve<Entity>(ENTRY_KEY, ENTRY_KEY), _cancellationToken)).Result as Entity;

                // We can obtain the lock if...
                if (_entity == null || //... there is no lock
                    _entity.Timestamp.Add(_timeout).CompareTo(DateTimeOffset.Now) < 0 || //... the lock has timed out
                    _entity.id.Equals(_id)) //... the lock is already ours
                {
                    if (_entity == null)
                    {
                        _entity = new Entity();
                    }
                    _entity.id = _id;

                    await _table.ExecuteAsync(TableOperation.InsertOrReplace(_entity), _cancellationToken);

                    // Having written our id to the table, we have successfully acquired the lock
                    return true;
                }
            }
            catch (Exception) { } // Any exception results in a failure to hold the lock

            // We do not have the lock; do not save it
            _entity = null;
            return false;
        }

        public bool Release()
        {
            if (_entity != null)
            {
                Trace.TraceInformation("{0} releasing lock '{1}'", _id.ToString(), _name);
                try
                {
                    // This is an attempt to clean up nicely; we don't want to interrupt shutdown,
                    // so if anything goes wrong we just return a simple failure result
                    _table.Execute(TableOperation.Delete(_entity));
                    _entity = null;
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }
    }
}
