# PaymentHub
asp net core 7 gRPC docker AzureSQL DEVOPS

THIS CODE IMPLEMENTS ITAU SECURE MTLS PRODUCTION ENVIROMENT (THIS Version does not support Itau test environment, because it does not support mTLS comunication)

This payment hub suport multiple acquirers (Cielo and Itau) of PIX payment method (credit card, debit and wallets are not supported on the open source version).

Each aquirer is implemented on distinct Projects.

This version of payment hub integrates with Itau PIX using mtls certificate comunication, just need to fill the configuration file appsettings.json (based on appsettings.Development.json).

Cielo integration will be avaliable in the next version.

Use AcquirerId to indicate Acquirer (Itau or Cielo) in Database configuration AzureSQLDataSource.sql file.

The certificate PSX file must be injected on POD, same way appsettings.json is.