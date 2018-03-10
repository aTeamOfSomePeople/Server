use messenger
go
create procedure GetMessagesFromChat(@id int)
as
SELECT * FROM [Messages] WHERE ChatId = @Id
go
create procedure GetUserChats(@id int)
as
select Chats.Id, [Name], [Type] from UsersInChats, Chats where ChatId = Chats.Id and UserId = @Id
go
create procedure GetUser(@login nvarchar(30), @password nvarchar(30))
as
select * from Users where @login = [login] and @password = [password]
go
create procedure GetAttachmentsToMessage(@id int)
as
select Attachments.Id, Link, MessageId from Attachments, [Messages] where MessageId = @id
go
create procedure FindUsers(@name nvarchar(50))
as
select Id, [Name], [Login], '' as [Password], Avatar from Users where [Name] like '%' + @name + '%' or [Login] like '%' + @name + '%'
