/*
	Altering to use standard types common among popular relational databases.
*/
ALTER TABLE EnvelopeTransaction DROP CONSTRAINT FK_EnvelopeTransaction_Envelope;
ALTER TABLE Envelope DROP CONSTRAINT FK_Envelope_UserID;

ALTER TABLE User MODIFY UserID INT NOT NULL AUTO_INCREMENT, 
	MODIFY MiB SMALLINT NOT NULL, 
	MODIFY Iterations SMALLINT NOT NULL, 
	MODIFY DegreeOfParallelism SMALLINT NOT NULL, 
	MODIFY FailedPasswordCount SMALLINT NOT NULL;
	
ALTER TABLE Envelope MODIFY EnvelopeID INT NOT NULL AUTO_INCREMENT,
	MODIFY UserID INT NOT NULL, 
	MODIFY EnvelopeNumber SMALLINT NOT NULL;
	
ALTER TABLE EnvelopeTransaction MODIFY EnvelopeTransactionID INT NOT NULL AUTO_INCREMENT,
	MODIFY EnvelopeID INT NOT NULL, 
	MODIFY TransactionNumber INT NOT NULL;

ALTER TABLE Envelope ADD CONSTRAINT FK_Envelope_UserID FOREIGN KEY (UserID) REFERENCES User(UserID);
ALTER TABLE EnvelopeTransaction ADD CONSTRAINT FK_EnvelopeTransaction_Envelope FOREIGN KEY (EnvelopeID) REFERENCES Envelope(EnvelopeID);
