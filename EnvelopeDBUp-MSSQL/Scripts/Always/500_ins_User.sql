
CREATE OR ALTER PROCEDURE ins_User
	@e NVARCHAR(255)
	,@p BINARY(32)
	,@s BINARY(16)
	,@m SMALLINT
	,@i SMALLINT
	,@dop SMALLINT
AS
BEGIN
	INSERT INTO [User](Email, PasswordHash, PasswordSalt, MiB, Iterations, DegreeOfParallelism) VALUES(TRIM(@e), @p, @s, @m, @i, @dop);
END
