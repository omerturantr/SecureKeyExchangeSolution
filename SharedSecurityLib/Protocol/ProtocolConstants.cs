namespace SharedSecurityLib.Protocol
{
    public static class ProtocolConstants
    {
        public const char FieldSeparator = ':';     // TYPE:field1:field2...
        public const int MaxMessageLength = 64 * 1024;

        public const string Localhost = "127.0.0.1";
        public const int CaPort = 9000;
        public const int Client2Port = 9100;
    }
}
