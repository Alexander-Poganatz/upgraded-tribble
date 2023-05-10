
DELIMITER //

DROP PROCEDURE IF EXISTS del_EnvelopeTransaction;
CREATE PROCEDURE del_EnvelopeTransaction(
	IN uID INT UNSIGNED
	,IN eNumber SMALLINT UNSIGNED
	,IN tNumber INT
)
BEGIN
	DELETE FROM EnvelopeTransaction WHERE EnvelopeID = (SELECT EnvelopeID FROM Envelope WHERE UserID = uID AND EnvelopeNumber = eNumber) AND TransactionNumber = tNumber;
END //

DELIMITER ;