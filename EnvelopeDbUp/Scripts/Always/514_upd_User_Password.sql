
DELIMITER //

DROP PROCEDURE IF EXISTS upd_User_Password;
CREATE PROCEDURE upd_User_Password(
	IN uid INT
	,IN p BINARY(32)
	,IN s BINARY(16)
	,IN m SMALLINT
	,IN i SMALLINT
	,IN dop SMALLINT
	)
BEGIN
	UPDATE User SET PasswordHash = p, PasswordSalt = s, MiB = m, Iterations = i, DegreeOfParallelism = dop WHERE UserID = uid;
END //

DELIMITER ;