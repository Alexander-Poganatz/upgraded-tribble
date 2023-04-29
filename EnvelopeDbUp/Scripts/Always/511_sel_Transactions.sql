DELIMITER //

DROP PROCEDURE IF EXISTS sel_Transactions;
CREATE PROCEDURE sel_Transactions(
	IN uID INT UNSIGNED
	,IN eNumber SMALLINT UNSIGNED
	,IN limitNum INT UNSIGNED
	,IN offsetNum INT UNSIGNED
)
BEGIN
	SELECT t.TransactionNumber, t.TransactionAmount, t.TransactionDate, t.Note
	FROM envelope e
	INNER JOIN envelopetransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = uID AND EnvelopeNumber = eNumber
	ORDER BY 1 DESC
	LIMIT limitNum OFFSET offsetNum;
	
	SELECT COUNT(*) AS NumberOfTransactions
	FROM envelope e
	INNER JOIN envelopetransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = uID AND EnvelopeNumber = eNumber;
END //

DELIMITER ;
