
DELIMITER //

DROP PROCEDURE IF EXISTS ins_EnvelopeTransaction;
CREATE PROCEDURE ins_EnvelopeTransaction(
	IN uID INT UNSIGNED
	,IN eNumber SMALLINT UNSIGNED
	,IN amount INT SIGNED
	,IN tDate DATE
	, IN tNote NVARCHAR(50)
)
BEGIN
	SELECT @eID := EnvelopeID FROM envelope WHERE UserID = uID AND envelopeNumber = eNumber;
	SELECT @newNum := IFNULL(MAX(TransactionNumber),0) + 1 FROM envelopetransaction WHERE EnvelopeID = @eID;
	INSERT INTO envelopetransaction(EnvelopeID, TransactionNumber, TransactionAmount, TransactionDate, Note)
	VALUES(@eID, @newNum, amount, tDate, tNote);
	SELECT @newNum;
END //

DELIMITER ;