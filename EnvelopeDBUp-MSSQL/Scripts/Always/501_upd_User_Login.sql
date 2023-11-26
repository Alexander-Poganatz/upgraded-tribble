
CREATE OR ALTER PROCEDURE upd_User_Login(
	@uID INT
	,@isValid BIT
)
AS
BEGIN
	IF @isValid = 1
		UPDATE [User] SET FailedPasswordCount = 0 WHERE UserID = @uID;
	ELSE
	BEGIN
		UPDATE [User] SET FailedPasswordCount = FailedPasswordCount + 1 WHERE UserID = @uID AND FailedPasswordCount < 1000;
	
		UPDATE [User] SET LockoutExpiry = DATEADD(MINUTE, (FailedPasswordCount*FailedPasswordCount - 15), GETUTCDATE()) WHERE UserID = @uID AND FailedPasswordCount > 4;
	END
END
