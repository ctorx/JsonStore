using Newtonsoft.Json;

namespace ctorx.JsonStore
{
    public class JsonStoreOptions
    {
        /// <summary>
        /// Gets or sets the serializer settings used to persist json to disk
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Gets or sets the path to the file store
        /// </summary>
        public string FileStorePath { get; set; }
    }
}