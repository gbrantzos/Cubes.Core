using Newtonsoft.Json;

namespace Cubes.Web.UIHelpers.Schema
{
    public enum ValidatorType
    {
        Min,
        Max,
        Required,
        RequiredTrue,
        Email,
        MinLength,
        MaxLength,
        Pattern
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Validator
    {
        [JsonConverter(typeof(CustomStringEnumConverter))]
        public ValidatorType Name { get; set; }
        public object Parameters { get; set; }
    }
}