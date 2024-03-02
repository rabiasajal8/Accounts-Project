CREATE DATABASE AqsaTraders;
GO
USE AqsaTraders;
GO

create table Role(
RoleCode int primary key,
RoleName varchar(20),
)

create table UserData(
UserName varchar(20) primary key,
Password varchar(30),
Name varchar(20),
RoleCode int Foreign key References Role(RoleCode) ON DELETE SET NULL ON UPDATE CASCADE,
Status bit
);

create table Year(
YearCode int primary key,
YearName varchar(20),
YearStatus bit,
)

create table Period(
YearCode int Foreign key References Year(YearCode) ON DELETE CASCADE ON UPDATE CASCADE,
PeriodCode int primary key,
PeriodName varchar (20),
PeriodStatus bit,
)

create table Branch(
BrCode int primary key,
BrName varchar (20),
BrAddresss varchar (30),
BrStatus bit,
)

create table Voucher(
PeriodCode int Foreign key References Period(PeriodCode) ON DELETE CASCADE ON UPDATE CASCADE,
VoucherType varchar(20), -- could be bank payment, cash payment
VoucherNumber int primary key,
VoucherDate date,
BrCode int Foreign key References Branch(BrCode) ON DELETE CASCADE ON UPDATE CASCADE,
Particulars Varchar(30), -- on hand indentifier
Status varchar(10), -- could be posted, in-progress etc
CreatedBy varchar(20),
Updatedby varchar(20),
PostedBy varchar(20)
)

create table Account(
AccountCode int primary key,
AccountName varchar (20) UNIQUE,
AccountGroup varchar (20),
AccountType varchar(20),
AccountStatus bit,
)

create table VoucherDetail(
id int IDENTITY(1,1) Primary Key,
VoucherNumber int Foreign key References Voucher(VoucherNumber) ON DELETE CASCADE ON UPDATE CASCADE,
AccountCode int Foreign key References Account(AccountCode) ON DELETE SET NULL ON UPDATE CASCADE,
Narration varchar(30), -- a lengthy description
DebitAmount numeric(38, 2),
CreditAmount numeric(38, 2),
)

-- App Object refers to services offered in app
-- such as COA, Legdger, Report Generation etc.
-- we need to setup these for role management

create table AppObject(
ObjName varchar(20),
ObjCode int primary key,
)

-- For one to many relationship
create table RoleObject(
ID int IDENTITY(1,1) Primary Key,
ObjCode int Foreign key References AppObject(ObjCode) ON DELETE CASCADE ON UPDATE CASCADE,
RoleCode int Foreign key References Role(RoleCode) ON DELETE CASCADE ON UPDATE CASCADE,
)

-- We need a view for our general ledger

GO
CREATE VIEW Ledger AS
SELECT ROW_NUMBER() OVER(ORDER BY V.VoucherNumber) as id, V.*, VD.AccountCode, VD.DebitAmount, VD.CreditAmount, VD.Narration
FROM VoucherDetail VD JOIN Voucher V 
ON V.VoucherNumber = VD.VoucherNumber
WHERE V.VoucherType = 'Posted';
GO

-- TEST DATA
select * from UserData;
select * from Role;
select * from Period;
select * from Voucher;
select * from Year;
select * from Account;
select * from AppObject;
select * from RoleObject;
select * from VoucherDetail;
select * from Branch;
SELECT * FROM Ledger;

-- Stored Procedures for efficient database interaction through API

-- Select Procedures

GO
CREATE PROCEDURE SelectAllRoles
AS
SELECT * FROM role
GO

CREATE PROCEDURE SelectAllUsers
AS
SELECT * FROM UserData
GO

CREATE PROCEDURE SelectYears
AS
SELECT * FROM Year
GO

CREATE PROCEDURE SelectAllPeriods
AS
SELECT * FROM Period
GO

CREATE PROCEDURE SelectAllBranchDetails
AS
SELECT * FROM Branch
GO

CREATE PROCEDURE SelectAllVouchers
AS 
SELECT * FROM Voucher
GO

CREATE PROCEDURE SelectAllAccounts
AS
SELECT * FROM Account
GO

CREATE PROCEDURE SelectAllVoucherDetails
AS
SELECT * FROM VoucherDetail
GO

CREATE PROCEDURE SelectAppObjects
AS
SELECT * FROM AppObject
GO

CREATE PROCEDURE SelectRoleObjects
AS
SELECT * FROM RoleObject
GO

-- Insert Update Delete Procedures

--1

CREATE PROCEDURE InsertRole 
@Rcode int , @Rname varchar(20)
AS
Begin
INSERT INTO Role(RoleCode ,RoleName)
Values (@Rcode , @Rname)
END
GO

CREATE PROCEDURE UpdateRole
    @RoleCode int,
    @RoleName varchar(20)
AS
BEGIN
    UPDATE Role
    SET RoleName = @RoleName
    WHERE RoleCode = @RoleCode
END
GO

CREATE PROCEDURE DeleteRole
    @RoleCode int
AS
BEGIN
    DELETE FROM Role
    WHERE RoleCode = @RoleCode
END
GO

--2

CREATE PROCEDURE InsertUserData
@Uname varchar(20) , @Upass varchar(30) , @Name varchar(20) , @UserRoleCode int , @UserStatus bit 
AS
BEGIN
    INSERT INTO UserData(UserName, Password, Name , RoleCode , Status)
    VALUES (@Uname, @Upass, @Name, @UserRoleCode, @UserStatus)
END
GO

CREATE PROCEDURE UpdateUserData
    @UserName varchar(20),
    @Password varchar(30),
    @Name varchar(20),
    @RoleCode int,
    @Status bit
AS
BEGIN
    UPDATE UserData
    SET Password = @Password,
        Name = @Name,
        RoleCode = @RoleCode,
        Status = @Status
    WHERE UserName = @UserName
END
GO

CREATE PROCEDURE DeleteUserData
    @UserName varchar(20)
AS
BEGIN
    DELETE FROM UserData
    WHERE UserName = @UserName
END
GO

--3

CREATE PROCEDURE InsertYear
    @YearCode int,
    @YearName varchar(20),
    @YearStatus bit
AS
BEGIN
    INSERT INTO Year (YearCode, YearName, YearStatus)
    VALUES (@YearCode,@YearName ,@YearStatus )
END
GO

--	UPDATE YEAR 
CREATE PROCEDURE UpdateYear
    @YearCode int,
    @YearName varchar(20),
    @YearStatus bit
AS
BEGIN
    UPDATE Year
    SET YearName = @YearName,
        YearStatus = @YearStatus
    WHERE YearCode = @YearCode
END
GO

--Delete Year
CREATE PROCEDURE DeleteYear
    @YearCode int
AS
BEGIN
    DELETE FROM Year
    WHERE YearCode = @YearCode
END
GO

--4
CREATE PROCEDURE InsertPeriod
    @YearCode int,
    @PeriodCode int,
    @PeriodName varchar(20),
    @PeriodStatus bit
AS
BEGIN
    INSERT INTO Period (YearCode, PeriodCode, PeriodName, PeriodStatus)
    VALUES (@YearCode, @PeriodCode, @PeriodName, @PeriodStatus)
END
GO

CREATE PROCEDURE UpdatePeriod
    @YearCode int,
    @PeriodCode int,
    @PeriodName varchar(20),
    @PeriodStatus bit
AS
BEGIN
    UPDATE Period
    SET PeriodName = @PeriodName,
        PeriodStatus = @PeriodStatus
    WHERE YearCode = @YearCode AND PeriodCode = @PeriodCode
END
GO

CREATE PROCEDURE DeletePeriod
    @PeriodCode int
AS
BEGIN
    DELETE FROM Period
    WHERE PeriodCode = @PeriodCode
END
GO

--5

CREATE PROCEDURE InsertBranch
    @BrCode int,
    @BrName varchar(20),
    @BrAddress varchar(30),
    @BrStatus bit
AS
BEGIN
    INSERT INTO Branch (BrCode, BrName, BrAddresss, BrStatus)
    VALUES (@BrCode, @BrName, @BrAddress, @BrStatus)
END
GO

CREATE PROCEDURE UpdateBranch
    @BrCode int,
    @BrName varchar(20),
    @BrAddresss varchar(30),
    @BrStatus bit
AS
BEGIN
    UPDATE Branch
    SET BrName = @BrName,
        BrAddresss = @BrAddresss,
        BrStatus = @BrStatus
    WHERE BrCode = @BrCode
END
GO

CREATE PROCEDURE DeleteBranch
    @BrCode int
AS
BEGIN
    DELETE FROM Branch
    WHERE BrCode = @BrCode
END
GO

--6

CREATE PROCEDURE InsertVoucher
    @PeriodCode int,
    @VoucherType varchar(20),
    @VoucherNumber int,
    @VoucherDate date,
    @BrCode int,
    @Particulars varchar(30),
    @Status varchar(10),
    @CreatedBy varchar(20),
    @PostedBy varchar(20)
AS
BEGIN
    INSERT INTO Voucher (PeriodCode, VoucherType, VoucherNumber, VoucherDate, BrCode, Particulars, Status, CreatedBy, PostedBy)
    VALUES (@PeriodCode, @VoucherType, @VoucherNumber, @VoucherDate, @BrCode, @Particulars, @Status, @CreatedBy, @PostedBy)
END
GO

CREATE PROCEDURE UpdateVoucher
    @PeriodCode INT,
    @VoucherType VARCHAR(20),
    @VoucherNumber INT,
    @VoucherDate DATE,
    @BrCode INT,
    @Particulars VARCHAR(30),
    @Status VARCHAR(10),
    @CreatedBy VARCHAR(20),
    @PostedBy VARCHAR(20)
AS
BEGIN
    UPDATE Voucher
    SET PeriodCode = @PeriodCode,
        VoucherType = @VoucherType,
        VoucherDate = @VoucherDate,
        BrCode = @BrCode,
        Particulars = @Particulars,
        Status = @Status,
        CreatedBy = @CreatedBy,
        PostedBy = @PostedBy
    WHERE VoucherNumber = @VoucherNumber
END
GO

CREATE PROCEDURE DeleteVoucher
    @VoucherNumber int
AS
BEGIN
    DELETE FROM Voucher
    WHERE VoucherNumber = @VoucherNumber
END
GO

--7

CREATE PROCEDURE InsertAccount
    @AccountCode int,
    @AccountName varchar(20),
    @AccountGroup varchar(20),
    @AccountType varchar(20),
    @AccountStatus bit
AS
BEGIN
    INSERT INTO Account (AccountCode, AccountName, AccountGroup, AccountType, AccountStatus)
    VALUES (@AccountCode, @AccountName, @AccountGroup, @AccountType, @AccountStatus)
END
GO

CREATE PROCEDURE UpdateAccount
    @AccountCode INT,
    @AccountName VARCHAR(20),
    @AccountGroup VARCHAR(20),
    @AccountType VARCHAR(20),
    @AccountStatus BIT
AS
BEGIN
    UPDATE Account
    SET AccountName = @AccountName,
        AccountGroup = @AccountGroup,
        AccountType = @AccountType,
        AccountStatus = @AccountStatus
    WHERE AccountCode = @AccountCode
END
GO

CREATE PROCEDURE DeleteAccount
    @AccountCode int
AS
BEGIN
    DELETE FROM Account
    WHERE AccountCode = @AccountCode
END
GO

--8

CREATE PROCEDURE InsertVoucherDetail
    @VoucherNumber int,
    @AccountCode int,
    @Narration varchar(30),
    @DebitAmount numeric(38, 2),
    @CreditAmount numeric(38, 2)
AS
BEGIN
    INSERT INTO VoucherDetail (VoucherNumber, AccountCode, Narration, DebitAmount, CreditAmount)
    VALUES (@VoucherNumber, @AccountCode, @Narration, @DebitAmount, @CreditAmount)
END
GO

CREATE PROCEDURE UpdateVoucherDetail
    @VoucherNumber INT,
    @AccountCode INT,
    @Narration VARCHAR(30),
    @DebitAmount NUMERIC(38, 2),
    @CreditAmount NUMERIC(38, 2)
AS
BEGIN
    UPDATE VoucherDetail
    SET AccountCode = @AccountCode,
        Narration = @Narration,
        DebitAmount = @DebitAmount,
        CreditAmount = @CreditAmount
    WHERE VoucherNumber = @VoucherNumber
END
GO

CREATE PROCEDURE DeleteVoucherDetail
    @VoucherNumber int,
    @AccountCode int
AS
BEGIN
    DELETE FROM VoucherDetail
    WHERE VoucherNumber = @VoucherNumber AND AccountCode = @AccountCode
END
GO

--9

CREATE PROCEDURE InsertAppObject
    @ObjName varchar(20),
    @ObjCode int
AS
BEGIN
    INSERT INTO AppObject (ObjName, ObjCode)
    VALUES (@ObjName, @ObjCode)
END
GO

CREATE PROCEDURE UpdateAppObject
 @ObjName varchar(20),
 @ObjCode int
 AS
 BEGIN
	UPDATE AppObject
	SET ObjName=@ObjName
	WHERE ObjCode=@ObjCode
END
GO

CREATE PROCEDURE DeleteAppObject
    @ObjCode int
AS
BEGIN
    DELETE FROM AppObject
    WHERE ObjCode = @ObjCode
END
GO

--10

CREATE PROCEDURE InsertRoleObject
    @ObjCode int,
    @RoleCode int
AS
BEGIN
    INSERT INTO RoleObject (ObjCode, RoleCode)
    VALUES (@ObjCode, @RoleCode)
END
GO

CREATE PROCEDURE UpdateRoleObject
    @ObjCode int,
    @RoleCode int,
	@ID int
AS
BEGIN
	UPDATE RoleObject
	SET ObjCode = @ObjCode,
		RoleCode = @RoleCode
	WHERE ID=@ID
END
GO

CREATE PROCEDURE DeleteRoleObject
	@id int 
AS
BEGIN
	DELETE FROM RoleObject
	where ID=@ID
END
GO

GO
CREATE FUNCTION getNewChildAccountCode 
(
	@parentCode int,
	@parentLevel int
)
RETURNS int
AS
BEGIN
	DECLARE @nextCode varchar(20)
    
	SELECT @nextCode = Max(AccountCode) FROM Account WHERE Account.ParentCode = @parentCode;

	IF @nextCode is not null
		BEGIN
			IF(@parentLevel) = 1
				-- Second Level
				SET @nextCode = @nextCode + 100
			ELSE
				-- Third Level
				SET @nextCode = @nextCode + 1
		END

	ELSE
		BEGIN
			IF(@parentLevel) = 1
				-- Second Level
				SET @nextCode = CONVERT(int, CONVERT(varchar, @parentCode) + '0100')
			ELSE
				-- Third Level
				SET @nextCode = @parentCode + 1
		END

	SET @nextCode = CONVERT(int, @nextCode);

	RETURN @nextCode
END
GO

CREATE PROCEDURE InsertNewAccount
@accountName varchar(20),
@parentCode int,
@parentLevel int
AS
BEGIN
	DECLARE @accountCode int;
	DECLARE @accountGroup varchar(20);
	DECLARE @accountType varchar(20);

	SELECT @accountCode = dbo.getNewChildAccountCode(@parentCode, @parentLevel);

	IF @parentLevel != 1
		BEGIN
			SELECT @accountGroup = A.AccountName FROM Account A WHERE A.AccountCode = @parentCode;
			SELECT @accountType = A.AccountType FROM Account A WHERE A.AccountCode = @parentCode;
		END
	ELSE 
		SELECT @accountType = A.AccountName FROM Account A WHERE A.AccountCode = @parentCode;

	INSERT INTO Account
	VALUES (@accountCode, @accountName, @accountGroup, @accountType, 1, @parentCode);

END

GO
CREATE PROCEDURE GetNextChildAccountCode
@parentCode int,
@parentLevel int,
@accountCode int OUTPUT
AS
BEGIN
	SELECT @accountCode = dbo.getNewChildAccountCode(@parentCode, @parentLevel)
END

---------------------------------------------------------------