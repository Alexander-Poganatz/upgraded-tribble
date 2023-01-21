DELIMITER //

DROP PROCEDURE IF EXISTS upd_Envelope;
CREATE PROCEDURE upd_Envelope(
	IN uID INT UNSIGNED
	,IN eNumber SMALLINT UNSIGNED
	,IN eName NVARCHAR(50)
)
BEGIN
	UPDATE envelope SET EnvelopeName = eName WHERE UserID = uID AND EnvelopeNumber = eNumber;
END //

DELIMITER ;