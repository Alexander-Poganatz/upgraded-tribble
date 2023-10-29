DELIMITER //

DROP PROCEDURE IF EXISTS sel_Envelope;
CREATE PROCEDURE sel_Envelope(
	IN uID INT
	,IN eNumber SMALLINT
)
BEGIN
	SELECT EnvelopeName
	FROM Envelope
	WHERE UserID = uID AND EnvelopeNumber = eNumber;
END //

DELIMITER ;