using Newtonsoft.Json;

namespace ctorx.JsonStore
{
    public class JsonStoreOptions
    {
        /// <summary>
        /// Gets or sets the Serializer Settings
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Gets or sets the FileStorePath
        /// </summary>
        public string FileStorePath { get; set; }
    }
}