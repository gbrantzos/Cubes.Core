using Newtonsoft.Json;

namespace Cubes.Core.Web.UIHelpers.Schema
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

        public static Validator Required() => new Validator { Name = ValidatorType.Required };
        public static Validator Min(int value) => new Validator { Name = ValidatorType.Min, Parameters = value };
        public static Validator Max(int value) => new Validator { Name = ValidatorType.Max, Parameters = value };
        public static Validator RequiredTrue() => new Validator { Name = ValidatorType.RequiredTrue };
        public static Validator Email() => new Validator { Name = ValidatorType.Email };
        public static Validator MinLength(int value) => new Validator { Name = ValidatorType.MinLength, Parameters = value };
        public static Validator MaxLength(int value) => new Validator { Name = ValidatorType.MaxLength, Parameters = value };
        public static Validator Pattern(string regex) => new Validator { Name = ValidatorType.Pattern, Parameters = regex };
    }
}