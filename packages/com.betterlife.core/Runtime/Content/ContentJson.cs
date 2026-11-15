using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Parses content JSON with safe settings: decimals for exact money, no automatic date conversion,
    /// and duplicate property names rejected.
    /// </summary>
    public static class ContentJson
    {
        private static readonly JsonLoadSettings LoadSettings = new JsonLoadSettings
        {
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
            CommentHandling = CommentHandling.Ignore,
            LineInfoHandling = LineInfoHandling.Load,
        };

        /// <summary>
        /// Parses a JSON object.
        /// </summary>
        /// <param name="text">JSON text.</param>
        /// <returns>The root object.</returns>
        /// <exception cref="JsonException">When the text is not a valid JSON object.</exception>
        public static JObject ParseObject(string text)
        {
            using (var reader = new JsonTextReader(new StringReader(text ?? string.Empty)))
            {
                reader.FloatParseHandling = FloatParseHandling.Decimal;
                reader.DateParseHandling = DateParseHandling.None;
                JToken token = JToken.ReadFrom(reader, LoadSettings);
                if (reader.Read() && reader.TokenType != JsonToken.Comment)
                {
                    throw new JsonReaderException("Unexpected content after the root value.");
                }
                if (!(token is JObject obj))
                {
                    throw new JsonReaderException("The root value must be an object.");
                }
                return obj;
            }
        }
    }
}
