namespace QbModels.QBOProcessor
{
    public class ClientInfoDto
    {
        private string clientId;
        private string clientSecret;
        private string realmId;

        public ClientInfoDto() { }

        public string ClientId { get => clientId; set => clientId = value; }
        public string ClientSecret { get => clientSecret; set => clientSecret = value; }
        public string RealmId { get => realmId; set => realmId = value; }

        public void SetClientInfo(ClientInfoDto info)
        {
            ClientId = info.ClientId;
            ClientSecret = info.ClientSecret;
            RealmId = info.RealmId;
        }
    }
}
