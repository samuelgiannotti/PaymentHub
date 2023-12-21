# PaymentHub
The PaymentHub is a smart and robust with NO single point of failure including acquirer.

THIS CODE IMPLEMENTS ITAU SECURE MTLS PRODUCTION ENVIROMENT (THIS Version does not support Itau test environment, because Itau Test Environment does not support mTLS comunication) YOU NEED A VALID PRODUCTION ACCESS TO ITAU PIX API. For mor information on Itau PIX API see (https://devportal.itau.com.br/nossas-apis/itau-ep9-gtw-pix-recebimentos-ext-v2)

This implementation uses only 80 port comunication for performance reasons, so its is recommended to use inside cluster only, if tls or mtls is required I recommend you use a service mash solution.

Payment hub suport multiple acquirers (Cielo and Itau) of PIX payment method (credit card, debit and ewallets are not supported on the open source version).

Each acquirer is implemented using a distinct Project.

This version of PaymentHub integrates with Itau PIX using mtls certificate comunication, just need to fill the configuration file appsettings.json (based on appsettings.Development.json).

Cielo integration will be avaliable in the next version.

Use AcquirerId to indicate Acquirer (Itau or Cielo) in Database configuration AzureSQLDataSource.sql file.

The certificate PFX file must be injected on POD, same way appsettings.json is.

asp.net core 7 
gRPC 
Docker 
AzureSQL 
Azure DevOps