# Server
<br>
<h1>Локальный запуск данного сервера</h1>
<ol>
    <li>Скачать данный проект и запустить его в visual studio</li>
    <li>В файле Web.config в тэге ConnectionStrings у LocalDB заменить connectionString на необходимый.</li>
    <li>Открыть в верхней панели Средства -> Диспетчер пакетов NuGet -> Консоль диспетчера пакетов.</li>
    <li>Когда она прогрузится выполнить команду "update-database".</li>
    <li>Запустить сервер</li>
    <li>Сервер запущен, Вы замечательны!</li>
</ol>
<br>
<h1>Методы API</h1>
Базовый адрес методов - https://localhost:44364/
<ul>
    <li>
        <h2>Accounts</h2>
        <ul>
            <li>
                <h3>Auth</h3>
                HTTP POST<br>
                accounts/auth<br>
                <h4>Параметры</h4>
                <ul>
                    <li>login - тип: string; максимальная длина: 30 символов; минимальная длина: 5 символов; запрещённые символы: ;</li>
                    <li>password - тип: string; максимальная длина: 30 символов; минимальная длина: 4 символа; запрещённые символы: ;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
                "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
                "userId":2<br>
                }
            </li>
            <li>
                <h3>OAuth</h3>
                HTTP POST<br>
                accounts/oauth<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                    <li>service - тип: string; доступные значения: vk, google, instagram;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
                "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
                "userId":2<br>
                }
            </li>
            <li>
                <h3>Register</h3>
                HTTP POST<br>
                accounts/register<br>
                <h4>Параметры</h4>
                <ul>
                    <li>login - тип: string; максимальная длина: 30 символов; минимальная длина: 5 символов; запрещённые символы: ;</li>
                    <li>password - тип: string; максимальная длина: 30 символов; минимальная длина: 4 символа; запрещённые символы: ;</li>
                    <li>name - тип: string; максимальная длина: 30 символов; минимальная длина: 2 символа; запрещённые символы: ;</li>
                </ul>
            </li>
            <li>
                <h3>Delete</h3>
                HTTP POST<br>
                accounts/delete<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                </ul>
            </li>
            <li>
                <h3>ChangePassword</h3>
                HTTP POST<br>
                accounts/changepassword<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                    <li>oldpassword - тип: string; максимальная длина: 30 символов; минимальная длина: 4 символа; запрещённые символы: ;</li>
                    <li>newpassword - тип: string; максимальная длина: 30 символов; минимальная длина: 4 символа; запрещённые символы: ;</li>
                </ul>
            </li>
        </ul>
    </li>
    <li>
        <h2>Users</h2>
        <ul>
            <li>
                <h3>GetUserInfo</h3>
                HTTP GET<br>
                users/getuserinfo<br>
                <h4>Параметры</h4>
                <ul>
                    <li>userId - тип: long</li>
                    <li>fields - тип: string; доступные значения(можно использовать несколько через запятую): name, avatar, description;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
            <li>
                <h3>ChangeName</h3>
                HTTP POST<br>
                users/changename<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                    <li>newname - тип: string; максимальная длина: 30 символов; минимальная длина: 2 символа; запрещённые символы: ;</li>
                </ul>
            </li>
            <li>
                <h3>ChangeDescription</h3><br>
                HTTP POST<br>
                users/changedescription<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                    <li>newdescription - тип: string;</li>
                </ul>
            </li>
            <li>
                <h3>ChangeAvatar</h3>
                HTTP POST<br>
                users/changeavatar<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                    <li>fileid - тип: long; значение полученное при использовании метода Files.UploadFile</li>
                </ul>
            </li>
            <li>
                <h3>BanUser</h3>
                HTTP POST<br>
                users/banuser<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                    <li>userid - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>UnBanUser</h3>
                HTTP POST<br>
                users/unbanuser<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                    <li>userid - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>GetBannedUsers</h3>
                HTTP GET<br>
                users/getbannedusers<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                    <li>count - тип: int;</li>
                    <li>start - тип: int;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
            <li>
                <h3>FindUsersByName</h3>
                HTTP GET<br>
                users/findusersbyname<br>
                <h4>Параметры</h4>
                <ul>
                    <li>name - тип: string;</li>
                    <li>count - тип: int;</li>
                    <li>start - тип: int;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
        </ul>
    </li>
    <li>
        <h2>Chats</h2>
        <ul>
            <li>
                <h3>SetMessagesReaded</h3>
                HTTP POST<br>
                chats/setmessagesreaded<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
            <li>
                <h3>GetChatInfo</h3>
                HTTP GET<br>
                chats/getchatinfo<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>fields - тип: string; доступные значения(можно использовать несколько через запятую): name, avatar, creator, type, unreadedmessagescount, memberscount;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
            <li>
                <h3>CreateDialog</h3>
                HTTP POST<br>
                chats/createdialog<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;/li>
                    <li>secondUserId - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>CreateGroup</h3>
                HTTP POST<br>
                chats/creategroup<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>name - тип: string;</li>
                    <li>userIds - тип: string; ID перечисленные через ',' минимум 2 дополнительных пользователя</li>
                </ul>
            </li>
            <li>
                <h3>CreatePublic</h3>
                HTTP POST<br>
                chats/createpublic<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>name - тип: string;</li>
                    <li>userIds - тип: string; ID перечисленные через ','</li>
                </ul>
            </li>
            <li>
                <h3>ChangeName</h3>
                HTTP POST<br>
                chats/changename<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>newName - тип: string;</li>
                    <li>chatId - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>ChangeAvatar</h3>
                HTTP POST<br>
                chats/changeavatar<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>fileId - тип: long; значение полученное при использовании метода Files.UploadFile</li>
                </ul>
            </li>
            <li>
                <h3>JoinThePublic</h3>
                HTTP POST<br>
                chats/jointhepublic<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>GetUsers</h3>
                HTTP GET<br>
                chats/getusers<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>count - тип: int;</li>
                    <li>start - тип: int;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
            <li>
                <h3>RemoveUserFromGroup</h3>
                HTTP POST<br>
                chats/removeuserfromgroup<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>userId - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>Leave</h3>
                HTTP POST<br>
                chats/leave<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>BanUser</h3>
                HTTP POST<br>
                chats/banuser<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>userId - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>UnBanUser</h3>
                HTTP POST<br>
                chats/unbanuser<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>userId - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>GetBannedUsers</h3>
                HTTP GET<br>
                chats/getbannedusers<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>count - тип: int;</li>
                    <li>start - тип: int;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
            <li>
                <h3>InviteToGroup</h3>
                HTTP POST<br>
                chats/invitetogroup<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>userId - тип: long;</li>
                </ul>
            </li>
            <li>
                <h3>FindPublicByName</h3>
                HTTP GET<br>
                accounts/auth/<br>
                <h4>Параметры</h4>
                <ul>
                    <li>name - тип: string;</li>
                    <li>count - тип: int;</li>
                    <li>start - тип: int;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
        </ul>
    </li>
    <li>
        <h2>Messages</h2>
        <ul>
            <li>
                <h3>SendMessage</h3>
                HTTP POST<br>
                messages/sendmessage<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>text - тип: string;</li>
                    <li>fileIds - тип: string; значение полученное при использовании метода Files.UploadFile</li>
                </ul>
            </li>
            <li>
                <h3>GetMessages</h3>
                HTTP POST<br>
                messages/getmessages<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>chatId - тип: long;</li>
                    <li>count - тип: int;</li>
                    <li>start - тип: int;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
            <li>
                <h3>EditMessage</h3>
                HTTP POST<br>
                messages/editnessage<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>messageId - тип: long;</li>
                    <li>newText - тип: string;</li>
                </ul>
            </li>
            <li>
                <h3>DeleteMessage</h3>
                HTTP POST<br>
                messages/deletemessage<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accessToken - тип: string;</li>
                    <li>messageId - тип: long;</li>
                    <li>fromAll - тип: bool; удаление сообщения у всех пользователей или только у себя</li>
                </ul>
            </li>
        </ul>
    </li>
    <li>
        <h2>Files</h2>
        <ul>
            <li>
                <h3>UploadFile</h3>
                HTTP POST<br>
                files/uploadfile<br>
                <h4>Параметры</h4>
                <ul>
                    <li>file - тип: stream; загрузка файла, доступные форматы: .jpg, .bmp</li>
                </ul>
            </li>
        </ul>
    </li>
    <li>
        <h2>Tokens</h2>
        <ul>
            <li>
                <h3>RefreshTokens</h3>
                HTTP POST<br>
                tokens/refreshtokens<br>
                <h4>Параметры</h4>
                <ul>
                    <li>refreshtoken - тип: string;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
                "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
                "userId":2<br>
                }
            </li>
            <li>
                <h3>CheckToken</h3>
                HTTP GET<br>
                tokens/checktoken<br>
                <h4>Параметры</h4>
                <ul>
                    <li>accesstoken - тип: string;</li>
                </ul>
                <h4>Возвращаемое значение</h4>
                {<br>
                }
            </li>
        </ul>
    </li>
</ul>
