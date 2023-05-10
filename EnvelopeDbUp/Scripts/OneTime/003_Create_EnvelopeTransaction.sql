CREATE TABLE EnvelopeTransaction(
	EnvelopeTransactionID INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY
	,EnvelopeID INT UNSIGNED NOT NULL
	,TransactionNumber INT UNSIGNED NOT NULL
	,TransactionAmount INT SIGNED NOT NULL
	,TransactionDate DATE NOT NULL
	,Note NVARCHAR(50) NOT NULL
	,CONSTRAINT FK_EnvelopeTransaction_Envelope FOREIGN KEY (EnvelopeID) REFERENCES Envelope(EnvelopeID)
	,CONSTRAINT AK_EnvelopeTransaction_Envelope_Number UNIQUE(EnvelopeID, TransactionNumber)
);