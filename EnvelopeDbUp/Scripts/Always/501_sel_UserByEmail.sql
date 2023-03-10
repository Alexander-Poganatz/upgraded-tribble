
DELIMITER //
DROP PROCEDURE IF EXISTS sel_UserByEmail;
CREATE PROCEDURE sel_UserByEmail(
	IN e NVARCHAR(255)
)
BEGIN
	SELECT UserID, PasswordHash, LockoutExpiry FROM user WHERE Email = e;
END //

DELIMITER ;