
DELIMITER //


DROP PROCEDURE IF EXISTS ins_Envelope;
CREATE PROCEDURE ins_Envelope(
	IN uID INT UNSIGNED
	,IN eName NVARCHAR(50)
)
BEGIN
	SELECT @newNum := IFNULL(MAX(EnvelopeNumber),0) + 1 FROM envelope WHERE UserID = uID;
	INSERT INTO envelope(UserID, EnvelopeNumber, EnvelopeName) VALUES
	(uID, @newNum, eName);
	
	SELECT @newNum;
END //

DELIMITER ;