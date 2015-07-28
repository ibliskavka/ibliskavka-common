using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ibliskavka.Common.Cache
{
    /// <summary>
    /// Implement this interface with your memory cache of choice.
    /// For CMS I use HttpContext.Application but a runtime cache or session cache might fit your application better.
    /// </summary>
    public interface ICache
    {
        bool Contains(string key);

        void Remove(string key);

        void Put<T>(string key, T value);

        T Get<T>(string key);
    }
}
