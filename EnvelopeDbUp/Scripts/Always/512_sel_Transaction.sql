DELIMITER //

DROP PROCEDURE IF EXISTS sel_Transaction;
CREATE PROCEDURE sel_Transaction(
	IN uID INT
	,IN eNumber SMALLINT
	,IN tNumber INT
)
BEGIN
	SELECT t.TransactionNumber, t.TransactionAmount, t.TransactionDate, t.Note
	FROM Envelope e
	INNER JOIN EnvelopeTransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = uID AND EnvelopeNumber = eNumber AND t.TransactionNumber = tNumber;
END //

DELIMITER ;