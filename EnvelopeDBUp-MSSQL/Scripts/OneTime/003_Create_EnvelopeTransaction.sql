CREATE TABLE EnvelopeTransaction(
	EnvelopeTransactionID INT NOT NULL IDENTITY(1,1)
	,EnvelopeID INT NOT NULL
	,TransactionNumber INT NOT NULL
	,TransactionAmount INT NOT NULL
	,TransactionDate DATE NOT NULL
	,Note NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_EnvelopeTransaction PRIMARY KEY(EnvelopeTransactionID)
	,CONSTRAINT FK_EnvelopeTransaction_Envelope FOREIGN KEY (EnvelopeID) REFERENCES Envelope(EnvelopeID)
	,CONSTRAINT AK_EnvelopeTransaction_Envelope_Number UNIQUE(EnvelopeID, TransactionNumber)
);
