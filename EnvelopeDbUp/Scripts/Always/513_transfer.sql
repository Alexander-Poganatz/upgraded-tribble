DELIMITER //

DROP PROCEDURE IF EXISTS transfer;
CREATE PROCEDURE transfer(
	IN uID INT
	,IN eSourceNumber SMALLINT
	,IN eDestinationNumber SMALLINT
	,IN amount INT SIGNED
)
BEGIN
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;
	
	IF (SELECT COUNT(*) FROM Envelope WHERE UserID = uID AND EnvelopeNumber IN (eSourceNumber, eDestinationNumber)) <> 2 THEN
		SELECT 0;
	ELSE
		START TRANSACTION;
	
		CALL ins_EnvelopeTransaction(uID, eSourceNumber, ABS(amount) * -1, UTC_DATE(),CONCAT('Transfer to ', (SELECT EnvelopeName FROM Envelope WHERE UserID = uID AND EnvelopeNumber = eDestinationNumber)));
		CALL ins_EnvelopeTransaction(uID, eDestinationNumber, ABS(amount), UTC_DATE(),CONCAT('Transfer from ', (SELECT EnvelopeName FROM Envelope WHERE UserID = uID AND EnvelopeNumber = eSourceNumber)));
		COMMIT;
		SELECT 1;
	END IF;
	
END //

DELIMITER ;