using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ctorx.JsonStore
{
    /// <summary>
    /// Provides simple json store functionality for a specific type
    /// </summary>
    public abstract class JsonStore<TStoreItem, TIdType> where TStoreItem : class
    {
        static object Locker = new object();

        readonly IHostingEnvironment HostingEnvironment;
        readonly JsonStoreOptions JsonStoreOptions;
        readonly Func<TStoreItem, TIdType> IdExpression;

        string StorePath => Path.IsPathRooted(this.JsonStoreOptions.FileStorePath) 
            ? this.JsonStoreOptions.FileStorePath 
            : Path.Combine(this.HostingEnvironment.ContentRootPath, this.JsonStoreOptions.FileStorePath);
        
        string FilePath => Path.Combine(this.StorePath, $"{typeof(TStoreItem).Name}.json");
        
        /// <summary>
        /// ctor the Mighty
        /// </summary>
        protected JsonStore(IHostingEnvironment hostingEnvironment, IOptions<JsonStoreOptions> jsonStoreOptionsProvider, Func<TStoreItem, TIdType> idExpression)
        {
            this.HostingEnvironment = hostingEnvironment;
            this.JsonStoreOptions = jsonStoreOptionsProvider.Value;
            this.IdExpression = idExpression;

            // Ensure directory
            if (!Directory.Exists(this.StorePath))
                Directory.CreateDirectory(this.StorePath);

            if (!File.Exists(this.FilePath))
                File.WriteAllText(this.FilePath, "[]");
        }

        /// <summary>
        /// Gets the set of items in the Store
        /// </summary>
        public IQueryable<TStoreItem> GetSet()
        {
            var fileContents = File.ReadAllText(this.FilePath);
            return JsonConvert.DeserializeObject<IList<TStoreItem>>(fileContents, this.JsonStoreOptions.SerializerSettings).AsQueryable();
        }

        /// <summary>
        /// Saves an item to the store
        /// </summary>
        public void Save(TStoreItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var itemList = this.GetSet().ToList();
            
            var existing = itemList.FirstOrDefault(x => this.IdExpression(x).Equals(this.IdExpression(item)));

            if (existing == null)
                itemList.Add(item);
            else
                itemList[itemList.IndexOf(existing)] = item;

            this.WriteList(itemList);
        }

        /// <summary>
        /// Deletes an item from the store
        /// </summary>
        public void Delete(TStoreItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var itemList = this.GetSet().ToList();

            var existing = itemList.FirstOrDefault(x => this.IdExpression(x).Equals(this.IdExpression(item)));

            if (existing == null)
                throw new InvalidOperationException("item does not exist");

            itemList.Remove(existing);

            this.WriteList(itemList);
        }
        
        /// <summary>
        /// Writes the list to disk
        /// </summary>
        void WriteList(IList<TStoreItem> list)
        {
            if (list == null)
                list = new List<TStoreItem>();

            var serialized = JsonConvert.SerializeObject(list, this.JsonStoreOptions.SerializerSettings);

            lock (Locker)
            {                 
                File.WriteAllText(this.FilePath, serialized);
            }
        }
    }
}