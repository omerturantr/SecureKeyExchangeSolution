using System;

namespace SharedSecurityLib.Protocol
{
    public static class ProtocolMessage
    {
        public static string Build(params string[] parts)
        {
            if (parts == null || parts.Length == 0) return string.Empty;
            return string.Join(ProtocolConstants.FieldSeparator, parts);
        }

        public static string[] Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
            return raw.Trim().Split(ProtocolConstants.FieldSeparator);
        }

        public static string GetType(string raw)
        {
            var parts = Parse(raw);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }
    }
}
