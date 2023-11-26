
CREATE OR ALTER PROCEDURE ins_EnvelopeTransaction(
	@uID INT
	,@eNumber SMALLINT
	,@amount INT
	,@tDate DATE
	,@tNote NVARCHAR(50)
)
AS
BEGIN
	DECLARE @eID INT
	DECLARE @newNum INT
	SELECT @eID = EnvelopeID FROM Envelope WHERE UserID = @uID AND envelopeNumber = @eNumber;
	SELECT @newNum = ISNULL(MAX(TransactionNumber),0) + 1 FROM EnvelopeTransaction WHERE EnvelopeID = @eID;
	INSERT INTO EnvelopeTransaction(EnvelopeID, TransactionNumber, TransactionAmount, TransactionDate, Note)
	VALUES(@eID, @newNum, @amount, @tDate, @tNote);
	SELECT @newNum;
END
