
CREATE OR ALTER PROCEDURE upd_User_Password(
	@uid INT
	,@p BINARY(32)
	,@s BINARY(16)
	,@m SMALLINT
	,@i SMALLINT
	,@dop SMALLINT
	)
AS
BEGIN
	UPDATE [User] SET PasswordHash = @p, PasswordSalt = @s, MiB = @m, Iterations = @i, DegreeOfParallelism = @dop WHERE UserID = @uid;
END
