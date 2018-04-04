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
 <li><h2>Accounts</h2>
  <ul>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>OAuth</h3>
    HTTP POST<br>
    accounts/oauth<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    <li>service - Название сервиса, из которого получен accesstoken (Доступны "vk", "google", "instagram")</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Register</h3>
    HTTP POST<br>
    accounts/register<br>
    <h4>Параметры</h4>
    <ul>
     <li>login</li>
     <li>password</li>
     <li>name</li>
    </ul>
   </li>
   <li><h3>Delete</h3>
     HTTP POST<br>
    accounts/delete<br>
    <h4>Параметры</h4>
    <ul>
     <li>accesstoken</li>
    </ul>
   </li>
   <li><h3>ChangePassword</h3><br>
    HTTP POST<br>
    accounts/changepassword<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    <li>oldpassword</li>
    <li>newpassword</li>
    </ul>
   </li>
  </ul>
 </li>
 <li><h2>Users</h2>
  <ul>
  <li><h3>ChangeName</h3><br>
    HTTP POST<br>
    users/changename<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    <li>newname</li>
    </ul>
    </li>
   <li><h3>ChangeDescription</h3><br>
    HTTP POST<br>
    users/changedescription<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    <li>newdescription</li>
    </ul>
    </li>
   <li><h3>ChangeAvatar</h3><br>
    HTTP POST<br>
    users/changeavatar<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    <li>fileid</li>
    </ul>
    </li>
   <li><h3>BanUser</h3><br>
    HTTP POST<br>
    users/banuser<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    <li>userid</li>
    </ul>
   </li>
   <li><h3>UnBanUser</h3><br>
    HTTP POST<br>
    users/unbanuser<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    <li>userid</li>
    </ul>
   </li>
   <li><h3>GetBannedUsers</h3><br>
    HTTP GET<br>
    users/getbannedusers<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    <li>count</li>
     <li>start</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>FindUsersByName</h3><br>
    HTTP GET<br>
    users/findusersbyname<br>
    <h4>Параметры</h4>
    <ul>
    <li>name</li>
    <li>count</li>
     <li>start</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
 </ul>
  </li>
 <li><h2>Chats</h2>
  <ul>
  <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
 </ul>
  </li>
 <li><h2>Messages</h2>
  <ul>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
  <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
   
 </ul></li>
 <li><h2>Files</h2>
  <ul>
  <li><h3>Auth</h3><br>
    HTTP POST<br>
    accounts/auth/<br>
    <h4>Параметры</h4>
    <ul>
    <li>login</li>
    <li>password</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
 </li>
 </ul></li>
 <li><h2>Tokens</h2><br>
 <ul>
  <li><h3>RefreshTokens</h3><br>
    HTTP POST<br>
    tokens/refreshtokens<br>
    <h4>Параметры</h4>
    <ul>
    <li>refreshtoken</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
  <li><h3>CheckToken</h3><br>
    HTTP GET<br>
    tokens/checktoken<br>
    <h4>Параметры</h4>
    <ul>
    <li>accesstoken</li>
    </ul>
    <h4>Возвращаемое значение</h4>
    {<br>
      "accessToken":"f15d0a1db83d4e60a1ba4bae45968e90",<br>
      "refreshToken":"1397ff052f004f6f8169246f3fc3deb5",<br>
      "userId":2<br>
    }
   </li>
  </ul></li>
 </ul>
