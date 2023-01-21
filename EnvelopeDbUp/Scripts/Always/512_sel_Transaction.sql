DELIMITER //

DROP PROCEDURE IF EXISTS sel_Transaction;
CREATE PROCEDURE sel_Transaction(
	IN uID INT UNSIGNED
	,IN eNumber SMALLINT UNSIGNED
	,IN tNumber INT UNSIGNED
)
BEGIN
	SELECT t.TransactionNumber, t.TransactionAmount, t.TransactionDate, t.Note
	FROM envelope e
	INNER JOIN envelopetransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = uID AND EnvelopeNumber = eNumber AND t.TransactionNumber = tNumber;
END //

DELIMITER ;