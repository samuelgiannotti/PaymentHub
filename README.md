# PaymentHub
asp net core 7 gRPC docker AzureSQL

This hub integrates with Itau PIX using mtls certificate comunication, just need to fill the following configuration file (appsettings.json):

{
  "ConnectionStrings": {
    "PaymentHubConnection": "Server=<IP>;Database=<dbName>;User Id=<userName>;Password=<userPass>;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Grpc": "Debug"
    }
  },
  "AllowedHosts": "*",
  "PIXItau": {
    "ClientID": "<clientGUID>",
    "ClientSecret": "<secretStr>",
    "CertificatePath": "<fileName>.pfx",
    "CertificatePass": "<passStr>",
    "TokenAuthURL": "https://sts.itau.com.br/api/oauth/token",
    "CobrancaImediataURL": "https://secure.api.itau/pix_recebimentos/v2/cob",
    "ChavePIXCobranca": "<cnpj>",
    "AcquirerId": <int> //used for multiples different acquirers
  }
  }
