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
   <li><h3>OAuth</h3>
    HTTP POST<br>
    accounts/oauth/<br>
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
   <li><h3>Register</h3></li>
   <li><h3>Delete</h3></li>
  </ul>
 </li>
 <li><h2>Users</h2>
  <ul>
  <li><h3></h3></li>
 </ul>
  </li>
 <li><h2>Chats</h2>
  <ul>
   <li><h3></h3></li>
 </ul>
  </li>
 <li><h2>Messages</h2>
  <ul>
  <li><h3></h3></li>
 </ul></li>
 <li><h2>Files</h2>
  <ul>
  <li><h3></h3></li>
 </ul></li>
 </ul>
