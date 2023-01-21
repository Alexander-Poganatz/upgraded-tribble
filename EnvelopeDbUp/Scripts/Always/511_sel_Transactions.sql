DELIMITER //

DROP PROCEDURE IF EXISTS sel_Transactions;
CREATE PROCEDURE sel_Transactions(
	IN uID INT UNSIGNED
	,IN eNumber SMALLINT UNSIGNED
)
BEGIN
	SELECT t.TransactionNumber, t.TransactionAmount, t.TransactionDate, t.Note
	FROM envelope e
	INNER JOIN envelopetransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = uID AND EnvelopeNumber = eNumber;
END //

DELIMITER ;envelopetransaction