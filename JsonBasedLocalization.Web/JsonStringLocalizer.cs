using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace JsonBasedLocalization.Web
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly IDistributedCache _cache;

        public JsonStringLocalizer(IDistributedCache cache)
        {
            _cache = cache;
        }

        private readonly JsonSerializer _serializer = new();
        public LocalizedString this[string name]
        {
        
                get
            {
                    var value = GetString(name);
                    return new LocalizedString(name, value);
                }
            
        }

        public LocalizedString this[string name, params object[] arguments] 
        {
            get
            {
                var actualValue = this[name];
                return !actualValue.ResourceNotFound
                    ? new LocalizedString(name, string.Format(actualValue.Value, arguments))
                    : actualValue;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var filePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";

            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader streamReader = new(stream);
            using JsonTextReader reader = new(streamReader);

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    continue;

                var key = reader.Value as string;
                reader.Read();
                var value = _serializer.Deserialize<string>(reader);
                yield return new LocalizedString(key, value);
            }
        }

        private string GetString(string key)
        {

            var filePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";
            var FullFilePath = Path.GetFullPath(filePath);

            if (File.Exists(FullFilePath)) 
            {
                var cacheKey = $"locale_{Thread.CurrentThread.CurrentCulture.Name}_{key}";
                // locale_en-US_welcome
                // locale_ar-EG_welcome
                var cacheValue = _cache.GetString(cacheKey);
                if (!string.IsNullOrEmpty(cacheValue))
                    return cacheValue;

                var result = GetValueFromJSON(key, FullFilePath);
                if(string.IsNullOrEmpty(result))
                    _cache.SetString(cacheKey, result);

                return result;
            }

            return string.Empty;
        }


        private string GetValueFromJSON(string propertyName , string filePath)
        {
            if(string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(filePath))
                return string.Empty;

            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader streamReader = new(stream);
            using JsonTextReader reader = new(streamReader);

            while (reader.Read()) 
            {
                if (reader.TokenType == JsonToken.PropertyName && reader.Value as string == propertyName) {
                    reader.Read();
                    return _serializer.Deserialize<string>(reader);
                }
            }

            return string.Empty;
        }
         
    }
}
