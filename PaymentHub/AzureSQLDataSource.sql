/*
 * These tables supports multiple PIX Aquirers (Cielo and Itau)
 */

CREATE TABLE [Acquirer](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](16) NOT NULL,
	[Default] [bit] NOT NULL,
 CONSTRAINT [PK_Acquirer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))
GO

CREATE TABLE [PIXPayment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[CustomerName] [nvarchar](100) NOT NULL,
	[CustomerDocumentType] [nvarchar](10) NOT NULL,
	[CustomerDocument] [nvarchar](20) NOT NULL,
	[OrderId] [int] NOT NULL,
	[AcquirerOrderId] [nvarchar](30) NULL,
	[QrCodeBase64Image] [varchar](max) NULL,
	[QrCodeString] [nvarchar](512) NOT NULL,
	[ProofOfSale] [nvarchar](20) NULL,
	[Tid] [nvarchar](50) NULL,	
	[PaymentId] [uniqueidentifier] NULL,
	[Status] [numeric](2, 0) NULL,
	[StatusStr] [nvarchar](31) NULL,
	[ReturnCode] [nvarchar](32) NULL,
	[ReturnMessage] [nvarchar](max) NULL,
	[Amount] [int] NOT NULL,
	[Installments] [int] NOT NULL,
	[ReceivedDate] [nvarchar](21) NULL,
	[SelfLink] [nvarchar](255) NULL,
	[EC] [numeric](10, 0) NULL,
	[CNPJ] [nvarchar](14) NULL,
	[AcquirerId] int NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL DEFAULT getDate(),
 CONSTRAINT [PK_PIXPayment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))
GO