create database messenger
use messenger
go
drop table if exists Attachments
drop table if exists [Messages]
drop table if exists UsersInChats
drop table if exists Chats
drop table if exists Users
go
create table Users
(
Id int primary key identity(1,1),
[Name] nvarchar(30) not null,
[Login] nvarchar(30) not null unique,
[Password] nvarchar(30) not null,
Avatar text 
)

create table Chats
(
Id int primary key identity(1,1),
[Name] nvarchar(50) not null,
[Type] nvarchar(10) not null 
)

create table UsersInChats
(
Id int primary key identity(1,1),
ChatId int not null,
UserId int not null,
unique(ChatId, UserId),
foreign key (ChatId) references Chats(Id),
foreign key (UserId) references Users(Id)
)

create table [Messages]
(
Id int primary key identity(1,1),
ChatId int not null,
UserId int not null,
[Text] text not null,
[Date] datetime default getdate(),
foreign key (ChatId) references Chats(Id),
foreign key (UserId) references Users(Id)
)

create table Attachments
(
Id int primary key identity(1,1),
MessageId int not null,
Link text not null,
foreign key (MessageId) references [Messages](Id)
)