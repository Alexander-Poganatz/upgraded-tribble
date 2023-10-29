
DELIMITER //

DROP PROCEDURE IF EXISTS ins_User;
CREATE PROCEDURE ins_User(
	IN e NVARCHAR(255)
	,IN p BINARY(32)
	,IN s BINARY(16)
	,IN m SMALLINT
	,IN i SMALLINT
	,IN dop SMALLINT
	)
BEGIN
	INSERT INTO User(Email, PasswordHash, PasswordSalt, MiB, Iterations, DegreeOfParallelism) VALUES(TRIM(e), p, s, m, i, dop);
END //

DELIMITER ;