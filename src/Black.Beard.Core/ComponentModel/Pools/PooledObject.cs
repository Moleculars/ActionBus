using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bb.Core.Pools
{

    /// <summary>
    /// Box for the embedded instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PooledObject<T> : IDisposable
    {

        private ObjectPool<T> pool;

        internal PooledObject(ObjectPool<T> pool)
        {
            this.pool = pool;
        }

        public void Release()
        {
            pool.PutObject(this);
        }

        public int InstanceId { get; internal set; }

        public T Instance { get; internal set; }

        public void Dispose(bool disposing)
        {
            if (disposing)
                Release();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~PooledObject()
        {
            Dispose(false);
        }


    }

}
