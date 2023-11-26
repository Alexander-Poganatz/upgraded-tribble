
CREATE OR ALTER PROCEDURE sel_Envelope_Summary(
	@uID INT
)
AS
BEGIN
	SELECT EnvelopeNumber, EnvelopeName, ISNULL(SUM(t.TransactionAmount),0)
	FROM Envelope e
	LEFT OUTER JOIN EnvelopeTransaction t ON e.EnvelopeID = t.EnvelopeID
	WHERE UserID = @uID
	GROUP BY EnvelopeNumber, EnvelopeName;
END
