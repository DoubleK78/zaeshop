using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Common.Enums;

namespace Common
{
    public static class CommonHelper
    {
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }

        public static string? JoinSeparator(this IEnumerable<string>? list, string separator = ",", bool isSpace = false)
        {
            if (list == null)
                return null;

            if (isSpace)
            {
                return string.Join(separator + " ", list);
            }
            else
            {
                return string.Join(separator, list);
            }
        }

        public static string GetServiceUrl(EServiceHost serviceHost)
        {
            var baseUrls = new Dictionary<EServiceHost, string>
            {
                { EServiceHost.Identity, "http://identity:5287" },
                { EServiceHost.Portal, "http://portal:5288" },
                { EServiceHost.Hangfire, "http://hangfireserver:5286" }
            };

            return baseUrls.TryGetValue(serviceHost, out string? baseUrl) ? baseUrl! : string.Empty;
        }

        public static string RemoveVietnameseCharacters(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            var result = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }

            return result.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string? GenerateFriendlyName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            // Remove all accents and make the string lower case
            var normalizedName = RemoveVietnameseCharacters(name);
            return normalizedName.ToLower().Replace(" ", "-");
        }

        public static async Task<byte[]> GetFileBytesByStreamAsync(Stream? stream)
        {
            if (stream == null) return Array.Empty<byte>();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public static ERoleType GetRoleType(List<string>? roles)
        {
            if (roles == null || roles.Count == 0) return ERoleType.User;
            if (roles.Exists(r => r == GetDescription(ERoleType.Administrator)))
            {
                return ERoleType.Administrator;
            }
            else if (roles.Exists(r => r == GetDescription(ERoleType.Partner)))
            {
                return ERoleType.Partner;
            }
            else if (roles.Exists(r => r == GetDescription(ERoleType.UserSuperPremium)))
            {
                return ERoleType.UserSuperPremium;
            }
            else if (roles.Exists(r => r == GetDescription(ERoleType.UserPremium)))
            {
                return ERoleType.UserPremium;
            }
            else if (roles.Exists(r => r == GetDescription(ERoleType.User)))
            {
                return ERoleType.User;
            }

            return ERoleType.User;
        }

        public static string GetSubscriptionByRoleType(ERoleType roleType, int? days)
        {
            string subscriptionLevel = string.Empty;
            if (days == 30)
            {
                subscriptionLevel = "1";
            }
            else if (days == 90)
            {
                subscriptionLevel = "2";
            }
            else if (days == 365)
            {
                subscriptionLevel = "3";
            }

            switch (roleType)
            {
                case ERoleType.UserSuperPremium:
                    return "SPremium Lv." + subscriptionLevel;
                case ERoleType.UserPremium:
                    return "Premium Lv." + subscriptionLevel;
                case ERoleType.User:
                    return "Free";
            }

            return string.Empty;
        }

        public static int RemainMinutesToNextCalculateExp(int minutes)
        {
            if (minutes >= 0 && minutes < 15)
            {
                return 15 - minutes;
            }
            else if (minutes >= 15 && minutes < 45)
            {
                return 45 - minutes;
            }
            else if (minutes >= 45 && minutes < 60)
            {
                return 60 - minutes + 15;
            }
            
            // Default
            return 30;
        }
    }

    public static class JsonSerializationHelper
    {
        private static readonly JsonSerializerOptions s_writeOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions s_readOptions = new()
        {
            AllowTrailingCommas = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, s_writeOptions);
        }

        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, s_readOptions);
        }
    }

    public static partial class RegexHelper
    {
        [GeneratedRegex(@"\d+")]
        private static partial Regex DigitRegex();

        public static int GetNumberByText(string title)
        {
            var match = DigitRegex().Match(title);
            return match.Success ? int.Parse(match.Value) : int.MaxValue;
        }
    }
}