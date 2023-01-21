DELIMITER //

DROP PROCEDURE IF EXISTS sel_Envelope;
CREATE PROCEDURE sel_Envelope(
	IN uID INT UNSIGNED
	,IN eNumber SMALLINT UNSIGNED
)
BEGIN
	SELECT EnvelopeName
	FROM envelope
	WHERE UserID = uID AND EnvelopeNumber = eNumber;
END //

DELIMITER ;