using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ibliskavka.Common.Cache
{
    /// <summary>
    /// This is a simple implementation of an object cache. The objects are cached on the file system and memory and refreshed from from the source when the file is deleted.
    /// This can cache any data source, simply implement LoadFromSource and pick a unique file name.
    /// Implement Initialize() if you need to set up other storage structures like dictionary, etc.
    /// I use this for caching SharePoint CMS type data on the webserver.
    /// </summary>
    /// <typeparam name="T">The type of object you intend to store</typeparam>
    public abstract class FileBasedCache<T>
    {
        private readonly string _cacheRoot;
        private readonly string _fileName;
        private readonly string _filePath;
        private readonly ICache _cache;

        public bool IgnoreSerializationErrors { get; set; }

        public List<T> Items;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryCache">Implement ICache with for your application. This can be be a session/application/runtime/etc cache.</param>
        /// <param name="fileName">Name of your XML file. Also used for the cache key</param>
        /// <param name="appRoot">Where to store the XML files</param>
        protected FileBasedCache(ICache memoryCache, string fileName, string appRoot)
        {
            _cache = memoryCache;
            _fileName = fileName;

            //Initialize App_Data Directory
            var appData = appRoot + "\\App_Data\\";
            if (!Directory.Exists(appData))
            {
                Directory.CreateDirectory(appData);
            }

            //Initialize Cache Root
            _cacheRoot = appData + "CmsCache\\";
            if (!Directory.Exists(_cacheRoot))
            {
                Directory.CreateDirectory(_cacheRoot);
            }

            _filePath = _cacheRoot + fileName;

            //By default the class will delete the file and try to load from source if a serialization error occurs.
            IgnoreSerializationErrors = true;
        }

        private void SaveToFile()
        {
            Serialization.SerializeToFile(Items, _filePath, false);
        }

        private void SaveToMemoryCache()
        {
            _cache.Put(_fileName, Items);
        }

        public void Load()
        {
            if (_cache.Contains(_fileName))
            {
                //Exists in memory cache
                Items = _cache.Get<List<T>>(_fileName);
            }
            else
            {
                //Load from file cache
                if (!LoadFromFile())
                {
                    //File does not exist, load from SharePoint
                    LoadFromSource();
                    SaveToFile();
                }
                SaveToMemoryCache();
            }

            //Initialize any objects in inherited classes
            Initialize();
        }

        protected abstract void LoadFromSource();

        private bool LoadFromFile()
        {
            if (!File.Exists(_filePath))
            {
                return false;
            }

            List<T> items;
            try
            {
                items = Serialization.DeserializeFromFile<List<T>>(_filePath);
            }
            catch (Exception ex)
            {
                if (IgnoreSerializationErrors)
                {
                    File.Delete(_fileName);
                    return false;
                }
                else
                {
                    throw ex;
                }
            }

            Items = items;
            return true;
        }

        /// <summary>
        /// Implement the Initialize method to configure the object after the file is loaded.
        /// </summary>
        protected virtual void Initialize()
        {
            //Add any additional initialization options here. Will get called after Load() is complete.
        }

        public void InvalidateCache()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
            _cache.Remove(_fileName);
            Items = null;
        }
    }
}
