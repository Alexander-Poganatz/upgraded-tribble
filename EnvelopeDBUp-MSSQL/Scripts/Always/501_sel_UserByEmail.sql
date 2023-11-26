
CREATE OR ALTER PROCEDURE sel_UserByEmail(
	@e NVARCHAR(255)
)
AS
BEGIN
	SELECT UserID, PasswordHash, PasswordSalt, MiB, Iterations, DegreeOfParallelism, LockoutExpiry FROM [User] WHERE Email = @e;
END
