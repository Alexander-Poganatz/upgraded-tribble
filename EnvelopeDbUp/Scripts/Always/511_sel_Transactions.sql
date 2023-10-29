DELIMITER //

DROP PROCEDURE IF EXISTS sel_Transactions;
CREATE PROCEDURE sel_Transactions(
	IN uID INT
	,IN eNumber SMALLINT
	,IN limitNum INT
	,IN offsetNum INT
)
BEGIN
	SELECT t.TransactionNumber, t.TransactionAmount, t.TransactionDate, t.Note
	FROM Envelope e
	INNER JOIN EnvelopeTransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = uID AND EnvelopeNumber = eNumber
	ORDER BY 1 DESC
	LIMIT limitNum OFFSET offsetNum;
	
	SELECT COUNT(*) AS NumberOfTransactions
	FROM Envelope e
	INNER JOIN EnvelopeTransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = uID AND EnvelopeNumber = eNumber;
END //

DELIMITER ;
