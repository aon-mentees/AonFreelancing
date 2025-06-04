CREATE TRIGGER ON_USER_UPDATE
    ON AspNetUsers
    AFTER UPDATE
              AS
BEGIN
    SET NOCOUNT ON
UPDATE AspNetUsers
SET UpdatedAt = GETDATE()
    FROM AspNetUsers u INNER JOIN inserted i ON u.Id = i.Id
END
