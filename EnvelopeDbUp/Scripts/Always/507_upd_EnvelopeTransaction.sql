
DELIMITER //

DROP PROCEDURE IF EXISTS upd_EnvelopeTransaction;
CREATE PROCEDURE upd_EnvelopeTransaction(
	IN uID INT UNSIGNED
	,IN eNumber SMALLINT UNSIGNED
	,IN tNumber INT UNSIGNED
	,IN amount INT SIGNED
	,IN tDate DATE
	,IN tNote NVARCHAR(50)
)
BEGIN
	UPDATE envelopetransaction SET TransactionAmount = amount, TransactionDate = tDate, Note = tNote
	WHERE EnvelopeID = (SELECT EnvelopeID FROM envelope WHERE UserID = uID AND EnvelopeNumber = eNumber) AND TransactionNumber = tNumber;
END //

DELIMITER ;