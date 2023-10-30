
DELIMITER //

DROP PROCEDURE IF EXISTS ins_EnvelopeTransaction;
CREATE PROCEDURE ins_EnvelopeTransaction(
	IN uID INT
	,IN eNumber SMALLINT
	,IN amount INT SIGNED
	,IN tDate DATE
	, IN tNote NVARCHAR(50)
)
BEGIN
	SELECT @eID := EnvelopeID FROM Envelope WHERE UserID = uID AND envelopeNumber = eNumber;
	SELECT @newNum := IFNULL(MAX(TransactionNumber),0) + 1 FROM EnvelopeTransaction WHERE EnvelopeID = @eID;
	INSERT INTO EnvelopeTransaction(EnvelopeID, TransactionNumber, TransactionAmount, TransactionDate, Note)
	VALUES(@eID, @newNum, amount, tDate, tNote);
	SELECT @newNum;
END //

DELIMITER ;