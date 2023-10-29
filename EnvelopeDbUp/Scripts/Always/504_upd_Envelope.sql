DELIMITER //

DROP PROCEDURE IF EXISTS upd_Envelope;
CREATE PROCEDURE upd_Envelope(
	IN uID INT
	,IN eNumber SMALLINT
	,IN eName NVARCHAR(50)
)
BEGIN
	UPDATE Envelope SET EnvelopeName = eName WHERE UserID = uID AND EnvelopeNumber = eNumber;
END //

DELIMITER ;