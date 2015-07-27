using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ibliskavka.Common
{
    /// <summary>
    /// If you dont want the FileBasedCache object to store data in a memory cache (file only), pass new DummyCache() to the constructor.
    /// </summary>
    public class DummyCache : ICache
    {
        public bool Contains(string key)
        {
            return false;
        }

        public T Get<T>(string key)
        {
            throw new NotImplementedException("This dummy cache object does not store anything, there is nothing to return.");
        }

        public void Put<T>(string key, T value)
        {
            
        }

        public void Remove(string key)
        {
                       
        }
    }
}
