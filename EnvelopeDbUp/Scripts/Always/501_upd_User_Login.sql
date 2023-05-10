DELIMITER //
DROP PROCEDURE IF EXISTS upd_User_Login;
CREATE PROCEDURE upd_User_Login(
	IN uID INT UNSIGNED
	,IN isValid BOOL
)
BEGIN
	IF isValid = 1 THEN
		UPDATE User SET FailedPasswordCount = 0 WHERE UserID = uID;
	ELSE
		UPDATE User SET FailedPasswordCount = FailedPasswordCount + 1 WHERE UserID = uID AND FailedPasswordCount < 1000;
	
		UPDATE User SET LockoutExpiry = DATE_ADD(UTC_TIMESTAMP(), INTERVAL (FailedPasswordCount*FailedPasswordCount - 15) MINUTE) WHERE UserID = uID AND FailedPasswordCount > 4;
	END IF;
END //

DELIMITER ;