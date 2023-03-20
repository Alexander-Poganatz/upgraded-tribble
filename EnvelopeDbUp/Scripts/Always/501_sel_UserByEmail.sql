
DELIMITER //
DROP PROCEDURE IF EXISTS sel_UserByEmail;
CREATE PROCEDURE sel_UserByEmail(
	IN e NVARCHAR(255)
)
BEGIN
	SELECT UserID, PasswordHash, PasswordSalt, MiB, Iterations, DegreeOfParallelism, LockoutExpiry FROM user WHERE Email = e;
END //

DELIMITER ;