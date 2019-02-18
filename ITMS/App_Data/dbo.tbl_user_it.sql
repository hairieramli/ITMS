CREATE TABLE [dbo].[tbl_user_it]
(
	[IDUser] INT NOT NULL PRIMARY KEY IDENTITY, 
    [user_name] VARCHAR(MAX) NULL, 
    [add_1] VARCHAR(MAX) NULL, 
    [add_2] VARCHAR(MAX) NULL, 
    [add_city] VARCHAR(200) NULL, 
    [add_state] VARCHAR(200) NULL, 
    [add_poscode] VARCHAR(50) NULL, 
    [ic_no] VARCHAR(200) NULL, 
    [user_email] VARCHAR(MAX) NULL, 
    [user_password] VARCHAR(MAX) NULL, 
    [user_type] VARCHAR(100) NULL, 
    [createdDate] DATE NULL, 
    [createdBy] INT NULL
)
