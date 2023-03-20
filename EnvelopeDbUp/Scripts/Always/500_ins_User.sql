
DELIMITER //

DROP PROCEDURE IF EXISTS ins_User;
CREATE PROCEDURE ins_User(
	IN e NVARCHAR(255)
	,IN p BINARY(32)
	,IN s BINARY(16)
	,IN m TINYINT UNSIGNED
	,IN i TINYINT UNSIGNED
	,IN dop TINYINT UNSIGNED
	)
BEGIN
	INSERT INTO user(Email, PasswordHash, PasswordSalt, MiB, Iterations, DegreeOfParallelism) VALUES(TRIM(e), p, s, m, i, dop);
END //

DELIMITER ;