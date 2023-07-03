DELIMITER //

DROP PROCEDURE IF EXISTS sel_Envelope_Summary;
CREATE PROCEDURE sel_Envelope_Summary(
	IN uID INT UNSIGNED
)
BEGIN
	SELECT EnvelopeNumber, EnvelopeName, IFNULL(SUM(t.TransactionAmount),0)
	FROM Envelope e
	LEFT OUTER JOIN EnvelopeTransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = uID
	GROUP BY EnvelopeNumber, EnvelopeName;
END //

DELIMITER ;