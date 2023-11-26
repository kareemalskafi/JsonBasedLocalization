using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace JsonBasedLocalization.Web
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly JsonSerializer _serializer = new();
        public LocalizedString this[string name] => throw new NotImplementedException();

        public LocalizedString this[string name, params object[] arguments] => throw new NotImplementedException();

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
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
