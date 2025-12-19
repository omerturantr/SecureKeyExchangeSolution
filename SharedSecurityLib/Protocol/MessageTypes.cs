namespace SharedSecurityLib.Protocol
{
    public static class MessageTypes
    {
        // Client -> CA
        public const string REQ_CERT = "REQ_CERT";

        // CA -> Client
        public const string CERT = "CERT";

        // Client -> Client (mevcut)
        public const string GET_PUBLIC_KEY = "GET_PUBLIC_KEY";
        public const string SESSION_KEY = "SESSION_KEY";

        // Client -> Client (certificate)
        public const string GET_CERT = "GET_CERT";
        public const string PEER_CERT = "PEER_CERT";   // NEW: PEER_CERT:<CERT:...>

        // Master Key (Km) handshake
        public const string KM1 = "KM1";
        public const string KM2 = "KM2";
        public const string KM3 = "KM3";

        // Session confirm (Ks ile doğrulama)
        public const string SESSION_CONFIRM = "SESSION_CONFIRM";

        // Adım 10: Ks ile şifreli veri
        public const string DATA = "DATA";
    }
}
