namespace Game.Services
{
    /// <summary>
    /// Interface for data saving and loading service.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// Load string data by key.
        /// </summary>
        string Load(string key);
        
        /// <summary>
        /// Load object with JSON deserialization.
        /// </summary>
        T LoadJson<T>(string key);
        
        /// <summary>
        /// Save string data by key.
        /// </summary>
        void Save(string key, string value);
        
        /// <summary>
        /// Save object with JSON serialization.
        /// </summary>
        void SaveJson(string key, object data);
    }
}