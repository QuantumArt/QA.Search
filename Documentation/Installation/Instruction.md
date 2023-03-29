# Установка продукта QP8.Search

## Предварительные зависимости

1. ОС Linux с версией ядра 4.9 или новее;  
2. Systemd;  
3. ASP.NET Core Runtime (в версии под .NET 6);  
4. PostgreSQL 14 или новее (может быть устанлвлен на другом сервере);  
5. psql (обычно идёт в составе пакета postgresqlX.x86-64, где X - номер версии);  
6. Установленный и настроенный продук QuantumArt QP8 CMS (может быть устанлвлен на другом сервере);  
7. ElasticSearch 8 (может быть устанлвлен на другом сервере);  
8. Sudo;  
9. Nano;  
10. Tar.  

## Скачивание продукта QP8.Search

Продукт доступен к загрузке на сайте QuantumArt по адресу DOWNLOAD_URL.  

## Создание пользователя и группы

Для запуска компонентов продукта QP8.Search рекомендуется создать отдельную группу и пользователя в системе.  
В качестве примера будем использовать имя пользователя и группы `quantumart`.  
Создадим группу следующей командой:  
```bash
sudo groupadd quantumart
```

После чего создадим пользователя и добавим его в созданную ранее группу:  
```bash
sudo useradd quantumart -g quantumart -m
```

Зададим пароль созданному пользователю:  
```bash
sudo passwd quantumart
```

## Распаковка архива с продуктом QP8.Search

Разархивируем скачанный ранее архив в домашнюю директорию созданного ранее пользователя командой:  
```bash
sudo tar -xf /path/to/archive -С /home/quantumart/
```

После распаковки выполнив команду `sudo ls /home/quantumart/` можно увидеть три новых директории:  
1. QP.Search.Admin - компонент веб-интерфейса администратора;  
2. QP.Search.Api - компонент api-поиска;  
3. QP.Search.Intergration - компонент сервиса индексации данных из QP в ElasticSearch.  

Выдадим пользователю `quantumart` права на владение всеми тремя директориями командами:  
```bash
sudo chown quantumart:quantumart /home/quantumart/QP.Search.Admin
sudo chown quantumart:quantumart /home/quantumart/QP.Search.Api
sudo chown quantumart:quantumart /home/quantumart/QP.Search.Integration
```

## Установка компонента QP8.Search.Integration

### Создание директории для log-файлов

Создадим директорию куда будут сохраняться log-файлы компонента QP8.Search.Integration командой:  
```bash
sudo mkdir /var/log/search-integration
```

После чего выдадим права владения этой директорией пользователю `quantumart` командой:  
```bash
sudo chown quantumart:quantumart /var/log/search-integration
```

### Настройка компонента QP8.Search.Integration

Откроем на редактирование файл конфигурации компонента командой:  
```bash
sudo nano /home/quantumart/QP.Search.Integration/appsettings.json
```

Параметр `IndexPermissions` отвечает за необходимость индексировать доступы к контентам на основе ролевой модели QP8 CMS. Указываем `true` если в проекте планируется использование индексации на основе ролевой модели QP8 CMS, в противном случае оставляем `false`.  

Параметр `IndexerLibraries` содежит массив имён библиотек индексации, описывающих логику индексации данных из QP8 CMS. Прописываем туда названия библиотек индексации, которые планируется использовать на проекте.  
В качестве примера будем использовать стандартную библиотеку индексации Демо-сайта под названием `QA.Search.Integration.Demosite`.  
Пример корректного заполнения параметра:  
```JSON
"IndexerLibraries":
[
   "QA.Search.Integration.Demosite"
]
```

Секция `ElasticSettings` описывает настройки ElasticSearch и содержит следующие параметры:  
1. Address - адрес сервера ElasticSearch;  
2. ProjectName - короткое название проекта латиницей;  
3. RequestTimeout - время ожидания ответа от ElasticSearch.  

**Внимание:** данная секция должна совпадать с одноимённой секцией, используемой при настройке компонентов `QP8.Search.Admin` и `QP8.Search.Api`.

Пример корректного заполнения секции:  
```JSON
"ElasticSettings":
{
  "Address": "http://127.0.0.1:9200",
  "ProjectName": "demosite",
  "RequestTimeout": "00:10:00.000"
}
```

Секция `Settings.QP` описывает правила полной переиндексации контентов и статей QP8 CMS. В единственном параметре `CronSchedule` указываем частоту с которой будет запускаться полная переиндексация всех контентов (создание нового индекса в ElasticSearch, заполнение его данными из QP8 CMS и удаление старого индекса) в формате CronTab.  
Создать расписание в формате CronTab можно например на этом сайте https://crontab.guru/.  
Для примера будем использовать запуск каждую десятую минуту.  
Пример корректного заполнения секции:  
```JSON
"Settings.QP":
{
  "CronSchedule": "*/10 * * * *"
}
```
**Внимание:** полная переиндексация является достаточно долгой и тяжелой операцией, запускать её желательно как можно реже.

Секция `Settings.QP.Update` описывает правила обновления и добавления контентов и статей QP8 CMS с момента последнего обновления или полной индексации. В единственном параметре `CronSchedule` указываем частоту с которой будет запускаться обновление данных в индексе ElasticSearch (добавление в существующий индекс ElasticSearch новых данных и обновление изменившихся данных в QP8 CMS) в формате CronTab.  
Создать расписание в формате CronTab можно например на этом сайте https://crontab.guru/.  
Для примера будем использовать запуск каждую пятую минуту.  
Пример корректного заполнения секции:  
```JSON
"Settings.QP.Update":
{
  "CronSchedule": "*/5 * * * *"
}
```
**Внимание:** Обновление должно запускаться не реже полной переиндексации.  

Секция `Settings.QP.Permissions` описывает правила индексации прав доступа согласно ролевой модели QP8 CMS. В единственном параметре `CronSchedule` указываем частоту с которой будет запускаться переинексация индекса с правами на контенты в формате CronTab.  
Создать расписание в формате CronTab можно например на этом сайте https://crontab.guru/.  
Для примера будем использовать запуск каждую десятую минуту.  
Пример корректного заполнения секции:  
```JSON
"Settings.QP.Permissions":
{
  "CronSchedule": "*/10 * * * *"
}
```
**Внимание:** При выключенной настройке `IndexPermissions` - эта настройка не используется.  

Секция `PermissionsConfiguration` описывает настройки индексации ролевой модели QP8 CMS и содержит следующие параметры:  
1. DefaultRoleAlias - стандартное имя роли используемой для публичных разделов;  
2. PermissionIndexName - название индекса ElasticSearch куда индексируются права доступа к контентам QP8 CMS;  
3. QPAbstractItems - список alias-ов разделов в QP8 CMS для которых необходимо индексировать права доступа. Массив строк.  

Пример корректного заполнения секции:  
```JSON
"PermissionsConfiguration":
{
  "DefaultRoleAlias": "Reader",
  "PermissionIndexName": "Permissions",
  "QPAbstractItems":
  [
    "corporate_releases",
    "technology_news",
    "anticorruption_policies"
  ]
}
```

**Внимание:** значения параметров `DefaultRoleAlias` и `PermissionIndexName` должны совпадать с параметрами `DefaultReaderRole` и `PermissionsIndexName` соответственно, используемыми при настройке компонента `QP8.Search.Api`.  

Секция `ViewOptions` описывает кол-во данных, которые нужно выбирать при чтении из QP8 CMS при индексации за один проход. Настройка влияет на потребляемый объём памяти и скорость индексации. Чем больше указано значение, тем больше будет потребляться памяти компонентом во время индексации и тем дольше будет ElasticSearch выполнять сохранение данных в индекс, но общее кол-во проходов будет меньше.  
Для примера если в контенте QP8 CMS содержится 1 000 статей, и указан размер в 10 статей, то будет выполнено 100 проходов. Если же указать размер в 100, то будет выполнено 10 проходов.  
Секция содержит как настройку размера пачки по умолчанию `DefaultBatchSize`, так и возможность указать индивидуальный размер для каждого индексируемого контента QP8 CMS.  
При указании индивидуального размера пачки размер указывается в параметре `BatchSize`, а он, в свою очередь, помещается в параметр с именем `View` который отвечает за индексацию контента в подключаемой библиотеке индексации имя которой указано в параметре `IndexerLibraries`.  
Пример корректного заполнения этой секции для проекта индексатора демо-сайта:  
```JSON
"ViewOptions":
{
  "DefaultBatchSize": 10,
  "ViewParameters":
  {
    "NewsPostView":
    {
      "BatchSize": 10
    },
    "TextPageExtensionView":
    {
      "BatchSize": 10
    }
  }
}
```

В секции `ContextConfiguration` описываются параметры подключения к БД, в которой QP8 CMS хранит данные. В секции присутствуют следующие параметры:  
1. ConnectionString - строка подключения к БД, где QP8 CMS хранит данные. Требуется учётная запись с правами на чтение из таблиц и представлений;  
2. SqlServerType - тип используемой БД, может быть `PostgreSQL` или `MSSQL`;  
3. DefaultSchemeName - имя схемы которая используется в БД QP8 CMS;  
4. ContentAccess - `Live` или `Stage` в зависимости от того, используется QP8 CMS в качестве Live окружения или Stage окружения;  
5. FormatTableName - формат именования таблиц в БД.  

Пример корректного заполнения секции:  
```JSON
"ContextConfiguration":
{
  "ConnectionString": "Server=127.0.0.1:5432;Database=qa_demosite_rus;User Id=user;Password=password;",
  "SqlServerType": "PostgreSQL",
  "DefaultSchemeName": "public",
  "ContentAccess": "Live",
  "FormatTableName": "{0}.{1}"
}
```

**Не упомянутые выше параметры конфигурации изменять не рекомендуется.**

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

Откроем файл конфигурации log-файлов командой:  
```bash
sudo nano /home/quantumart/QP.Search.Integration/NLog.config
```

Найдём строку содержащую текст `logDirectory` и в ней у параметра `value` заменим содержимое на путь к директории log-файлов, которую создали ранее.  
Должно получиться так:  
```XML
<variable name="logDirectory" value="/var/log/search-integration"/>
```

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

### Создание service файла для systemd

Для работы с компонентом как с сервисом с возможностью управления и автоматического запуска создаём файл с именем `search-integration.service` в директории `/usr/lib/systemd/system/` командой:  
```bash
sudo touch /usr/lib/systemd/system/search-integration.service
```

После чего открываем файл на редактирование командой:  
```bash
sudo nano /usr/lib/systemd/system/search-integration.service
```

И заполняем его следующим содержимым:  
```
[Unit]
Description=QP Search Integration Service
After=network.service
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=on-failure
RestartSec=5
User=quantumart
WorkingDirectory=/home/quantumart/QP.Search.Integration/
ExecStart=dotnet /home/quantumart/QP.Search.Integration/QA.Search.Generic.Integration.API.dll --urls http://*:5500

[Install]
WantedBy=multi-user.target
```

В этом файле нас интересует:  
1. Description - понятное описание сервиса;  
2. After - указание на то, после запуска какого сервиса нужно запускать этот;  
3. Restart - политика автоматического перезапуска;  
4. RestartSec - пауза между перезапусками;  
5. User - имя пользователя от которого будет запущен сервис;  
6. WorkingDirectory - директория где лежат все файлы приложения;  
7. ExecStart - команда выполняемая для запуска сервиса;  
   1. --urls - указание под каким DNS-именем или IP-адресом разршено подключаться к компоненту и на каком порту компонент будет ожидать подключения.  

Подробно узнать как настраивать systemd сервисы можно тут: https://www.freedesktop.org/software/systemd/man/systemd.service.html  

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

### Запуск компонента

Для запуска компонента выполняем команду:  
```bash
sudo systemctl start search-integration.service
```

После чего проверяем что компонент запустился без ошибок командой:  
```bash
sudo systemctl status search-integration.service
```

В случае успешного запуска в строке `Active` будет указано `active (running)`, дата и время когда сервис был запущен, а так же сколько прошло времени с момента запуска.  
Если при запуске компонента что-то пошло не так и у сервиса статус не `active`, то верятнее всего при конфигурировании была допущена ошибка. Проверить в чём ошибка можно в log-файлах компонента.

### Автоматический запуск компонента при старте ОС

Если сервис успешно запустился, то можно добавить его в автоматический запуск после загрузки ОС. Для этого выполним команду:  
```bash
sudo systemctl enable search-integration.service
```

## Установка компонента QP8.Search.Admin

### Создание директории для log-файлов

Создадим директорию куда будут сохраняться log-файлы компонента QP8.Search.Admin командой:  
```bash
sudo mkdir /var/log/search-admin
```

После чего выдадим права владения этой директорией пользователю `quantumart` командой:  
```bash
sudo chown quantumart:quantumart /var/log/search-admin
```

### Подготовка БД к использованию

В директории `/home/quantumart/QP.Search.Admin/Scripts` находится `sql`-файл `create-dbs.sql` для создания БД и пользователя баз данных, необходимых для компонента `QP8.Search.Admin` и `Crawler`-а.  
Откроем файл на редактирование командой:  
```bash
sudo nano /home/quantumart/QP.Search.Admin/Scripts/create-dbs.sql
```

Файл имеет примерно следующее содержимое:  
```SQL
CREATE DATABASE qa_search_admin;

CREATE USER qa_search_admin_user WITH PASSWORD 'StrongPass1234';

GRANT CONNECT ON DATABASE qa_search_admin TO qa_search_admin_user;

GRANT CREATE ON DATABASE qa_search_admin TO qa_search_admin_user;

GRANT pg_read_all_data TO qa_search_admin_user;
GRANT pg_write_all_data TO qa_search_admin_user;

CREATE DATABASE qa_search_crawler;

CREATE USER qa_search_crawler_user WITH PASSWORD 'StrongPass1234';

GRANT CONNECT ON DATABASE qa_search_crawler TO qa_search_crawler_user;

GRANT CREATE ON DATABASE qa_search_crawler TO qa_search_crawler_user;

GRANT pg_read_all_data TO qa_search_crawler_user;
GRANT pg_write_all_data TO qa_search_crawler_user;
```

В скрипте стоит выделить 5 основных параметров:  
1. qa_search_admin - название БД используемой для портала администратора поиска;  
2. qa_search_admin_user - имя пользователя БД для портала администратора поиска;  
3. qa_search_crawler - название БД используемой для crawler-а;  
4. qa_search_crawler_user - имя пользователя БД для crawler-а;  
5. StrongPass1234 - пароль пользователей.  

Эти данные требуется откорректировать согласно правилам именования БД, пользователей, а так же правилам стойкости паролей, принятым на проекте или в компании.  

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

Следующим шагом требуется выполнить этот скрипт на сервере БД. Для этого выполним команду:  
```bash
sudo psql -h 127.0.0.1 -p 5432 -U postgres -d postgres -a -f /home/quantumart/QP.Search.Admin/Scripts/create-dbs.sql
```

Разберём запрос по параметрам:  
1. `-h` - DNS-имя или IP адрес сервера на котором работает PostgreSQL;  
2. `-p` - номер порта который слушает PostgreSQL;  
3. `-U` - имя пользователя с которым будет производиться подключение (у пользователя должны быть права администратора БД);  
4. `-d` - название базы данных (должна существовать);  
5. `-a` - флаг отвечающий за вывод в консоль всех выполняемых команд;  
6. `-f` - путь до SQL-файла, который требуется выполнить.  

### Настройка компонента QP8.Search.Admin

Откроем на редактирование файл конфигурации компонента командой:  
```bash
sudo nano /home/quantumart/QP.Search.Admin/appsettings.json
```

В секции `ConnectionStrings` присутствуют две строки подключения к базам данных, созданным на предыдущем шаге. Заполняем их актуальными данными, использованными в скрипте создания.  
Пример корректного заполнения секции:  
```JSON
"ConnectionStrings": {
  "AdminSearchDbContextConnection": "Server=127.0.0.1:5432;Database=qa_search_admin;User Id=qa_search_admin_user;Password=StrongPass1234;",
  "CrawlerSearchDbContextConnection": "Server=127.0.0.1:5432;Database=qa_search_crawler;User Id=qa_search_crawler_user;Password=StrongPass1234;"
}
```

Секция `Settings` описывает общие параметры компонента, тут следует актуализировать только параметр `AdminAppUrl` в котором указать DNS-имя или адрес сервера, на котором работает компонент QP8.Search.Admin, этот адрес используется для формирования ссылки на восстановление пароля.  
Пример: `"AdminAppUrl": "http://127.0.0.1:5600"`  

Секция `SmtpServiceSettings` отвечает за настройки подключения к SMTP-серверу для отправки писем восстановления пароля доступа к панели администратора и содержит следующие параметры:  
1. Host - адрес smtp-сервера;  
2. Port - порт smtp-сервера;  
3. From - адрес электронной почты с которой будет происходить отправка писем для восстановления;  
4. DisplayName - имя пользователя подставляемое к адресу отправителя;  
5. UseDefaultCredentials - если `true`, то не используется авторизация по логину и паролю (SMTP-сервер должен быть настроен для принятия запросов без авторизации);  
6. EnableSsl - использовать ли ssl для установки подключения;  
7. User - имя пользователя для аутентификации на почтовом сервере (используется если параметр `UseDefaultCredentials` установлен в `false`);  
8. Password - пароль для аутентификации на почтовом сервере (используется если параметр `UseDefaultCredentials` установлен в `false`).  

Пример заполнения секции:  
```JSON
"SmtpServiceSettings": {
  "Host": "smtp.yandex.ru",
  "Port": 465,
  "From": "qp-search-admin@yandex.ru",
  "DisplayName": "Search Admin App",
  "UseDefaultCredentials": false,
  "EnableSsl": true,
  "User": "qp-search-admin@yandex.ru",
  "Password": "super_strong_password"
}
```

Секция `IndexingApiServiceConfiguration` содержит настройки подключения к компоненту QP8.Search.Integration.  
Параметры которые требуется поменять:  
1. Scheme - http-схема которая используется для подключения к API (`http` или `https`);  
2. Host - DNS-имя или IP-адрес сервера, на котором работает компонент индексации;  
3. Port - номер порта, который слушает компонент индексации;  
4. ProxyAddress - адрес proxy-сервера, если используется (если не используется - оставить пустым).  

Пример корректно заполнения секции:  
```JSON
"IndexingApiServiceConfiguration": {
  "Scheme": "http",
  "Host": "127.0.0.1",
  "Port": "5500",
  "Timeout": "00:01:10",
  "RelativePath": "api",
  "ProxyAddress": ""
}
```

Секция `ReindexWorkerSettings` описывает параметры внутреннего сервиса переиндексации в компоненте QP8.Search.Admin и содержит следующие параметры:  
1. Interval - периодичность запуска внутренних задач переиндексации;  
2. RunTasks - признак необходимости выполнять задачи переиндексации.  

Пример корректных настроек:  
```JSON
"ReindexWorkerSettings": {
  "Interval": "00:00:10",
  "RunTasks": true
}
```

Секция `ElasticSettings` описывает настройки ElasticSearch и содержит следующие параметры:  
1. Address - адрес сервера ElasticSearch;  
2. ProjectName - короткое название проекта латиницей;  
3. RequestTimeout - время ожидания ответа от ElasticSearch.  

**Внимание:** данная секция должна совпадать с одноимённой секцией, используемой при настройке компонентов `QP8.Search.Integration` и `QP8.Search.Api`.

Пример корректного заполнения секции:  
```JSON
"ElasticSettings":
{
  "Address": "http://127.0.0.1:9200",
  "ProjectName": "demosite",
  "RequestTimeout": "00:10:00.000"
}
```

**Не упомянутые выше параметры конфигурации изменять не рекомендуется.**

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

Откроем файл конфигурации log-файлов командой:  
```bash
sudo nano /home/quantumart/QP.Search.Admin/NLog.config
```

Найдём строку содержащую текст `logDirectory` и в ней у параметра `value` заменим содержимое на путь к директории log-файлов, которую создали ранее.  
Должно получиться так:  
```XML
<variable name="logDirectory" value="/var/log/search-admin"/>
```

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

### Создание service файла для systemd

Для работы с компонентом как с сервисом с возможностью управления и автоматического запуска создаём файл с именем `search-admin.service` в директории `/usr/lib/systemd/system/` командой:  
```bash
sudo touch /usr/lib/systemd/system/search-admin.service
```

После чего открываем файл на редактирование командой:  
```bash
sudo nano /usr/lib/systemd/system/search-admin.service
```

И заполняем его следующим содержимым:  
```
[Unit]
Description=QP Search Administration Console
After=search-integration.service
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=on-failure
RestartSec=5
User=quantumart
WorkingDirectory=/home/quantumart/QP.Search.Admin/
ExecStart=dotnet /home/quantumart/QP.Search.Admin/QA.Search.Admin.dll --urls http://*:5600

[Install]
WantedBy=multi-user.target
```

В этом файле нас интересует:  
1. Description - понятное описание сервиса;  
2. After - указание на то, после запуска какого сервиса нужно запускать этот;  
3. Restart - политика автоматического перезапуска;  
4. RestartSec - пауза между перезапусками;  
5. User - имя пользователя от которого будет запущен сервис;  
6. WorkingDirectory - директория где лежат все файлы приложения;  
7. ExecStart - команда выполняемая для запуска сервиса;  
   1. --urls - указание под каким DNS-именем или IP-адресом разршено подключаться к компоненту и на каком порту компонент будет ожидать подключения.  

Подробно узнать как настраивать systemd сервисы можно тут: https://www.freedesktop.org/software/systemd/man/systemd.service.html  

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

### Запуск компонента

Для запуска компонента выполняем команду:  
```bash
sudo systemctl start search-admin.service
```

После чего проверяем что компонент запустился без ошибок командой:  
```bash
sudo systemctl status search-admin.service
```

В случае успешного запуска в строке `Active` будет указано `active (running)`, дата и время когда сервис был запущен, а так же сколько прошло времени с момента запуска.  
Если при запуске компонента что-то пошло не так и у сервиса статус не `active`, то верятнее всего при конфигурировании была допущена ошибка. Проверить в чём ошибка можно в log-файлах компонента.

### Автоматический запуск компонента при старте ОС

Если сервис успешно запустился, то можно добавить его в автоматический запуск после загрузки ОС. Для этого выполним команду:  
```bash
sudo systemctl enable search-admin.service
```

## Установка компонента QP8.Search.Api

### Создание директории для log-файлов

Создадим директорию куда будут сохраняться log-файлы компонента QP8.Search.Api командой:  
```bash
sudo mkdir /var/log/search-api
```

После чего выдадим права владения этой директорией пользователю `quantumart` командой:  
```bash
sudo chown quantumart:quantumart /var/log/search-api
```

### Настройка компонента QP8.Search.Api

Откроем на редактирование файл конфигурации компонента командой:  
```bash
sudo nano /home/quantumart/QP.Search.Api/appsettings.json
```

Секция `Settings` описывает основные настройки компонента. Среди всех параметров изменим следующие:  
1. UsePermissions - указывает использовать ли систему ролевой модели QP8 CMS для ограничения доступа к индексам;  
2. PermissionsIndexName - название индекса хранящего списки доступов. Должен совпадать с параметром `PermissionIndexName` в конфигурации компонента `QP8.Search.Integration`;  
3. DefaultReaderRole - название роли доступа к данным без ограничения ролевой модели. Должен совпадать с параметром `DefaultRoleAlias` в конфигурации компонента `QP8.Search.Integration`;  
4. SuggestionsDefaultLength - кол-во результатов выдаваемых в Api подсказок по умолчанию.  

Пример корректного заполнения секции:  
```JSON
"Settings": {
  "ContextualFields": [ "SearchUrl" ],
  "UserQueryIndex": "userquery",
  "UsePermissions": false,
  "PermissionsIndexName": "",
  "DefaultReaderRole": "Reader",
  "SuggestionsDefaultLength": 10
}
```

Секция `ElasticSettings` описывает настройки ElasticSearch и содержит следующие параметры:  
1. Address - адрес сервера ElasticSearch;  
2. ProjectName - короткое название проекта латиницей;  
3. RequestTimeout - время ожидания ответа от ElasticSearch.  

**Внимание:** данная секция должна совпадать с одноимённой секцией, используемой при настройке компонентов `QP8.Search.Integration` и `QP8.Search.Admin`.

Пример корректного заполнения секции:  
```JSON
"ElasticSettings":
{
  "Address": "http://127.0.0.1:9200",
  "ProjectName": "demosite",
  "RequestTimeout": "00:10:00.000"
}
```

**Не упомянутые выше параметры конфигурации изменять не рекомендуется.**

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

Откроем файл конфигурации log-файлов командой:  
```bash
sudo nano /home/quantumart/QP.Search.Api/NLog.config
```

Найдём строку содержащую текст `logDirectory` и в ней у параметра `value` заменим содержимое на путь к директории log-файлов, которую создали ранее.  
Должно получиться так:  
```XML
<variable name="logDirectory" value="/var/log/search-api"/>
```

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

### Создание service файла для systemd

Для работы с компонентом как с сервисом с возможностью управления и автоматического запуска создаём файл с именем `search-api.service` в директории `/usr/lib/systemd/system/` командой:  
```bash
sudo touch /usr/lib/systemd/system/search-api.service
```

После чего открываем файл на редактирование командой:  
```bash
sudo nano /usr/lib/systemd/system/search-api.service
```

И заполняем его следующим содержимым:  
```
[Unit]
Description=QP Search Api Service
After=search-integration.service
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=on-failure
RestartSec=5
User=quantumart
WorkingDirectory=/home/quantumart/QP.Search.Api/
ExecStart=dotnet /home/quantumart/QP.Search.Api/QA.Search.Api.dll --urls http://*:5700

[Install]
WantedBy=multi-user.target
```

В этом файле нас интересует:  
1. Description - понятное описание сервиса;  
2. After - указание на то, после запуска какого сервиса нужно запускать этот;  
3. Restart - политика автоматического перезапуска;  
4. RestartSec - пауза между перезапусками;  
5. User - имя пользователя от которого будет запущен сервис;  
6. WorkingDirectory - директория где лежат все файлы приложения;  
7. ExecStart - команда выполняемая для запуска сервиса;  
   1. --urls - указание под каким DNS-именем или IP-адресом разршено подключаться к компоненту и на каком порту компонент будет ожидать подключения.  

Подробно узнать как настраивать systemd сервисы можно тут: https://www.freedesktop.org/software/systemd/man/systemd.service.html  

Сохраним внесённые изменения путём нажатия сочетания клавиш `Ctrl+O`, затем закроем файл сочетанием клавиш `Ctrl+X`.  

### Запуск компонента

Для запуска компонента выполняем команду:  
```bash
sudo systemctl start search-api.service
```

После чего проверяем что компонент запустился без ошибок командой:  
```bash
sudo systemctl status search-api.service
```

В случае успешного запуска в строке `Active` будет указано `active (running)`, дата и время когда сервис был запущен, а так же сколько прошло времени с момента запуска.  
Если при запуске компонента что-то пошло не так и у сервиса статус не `active`, то верятнее всего при конфигурировании была допущена ошибка. Проверить в чём ошибка можно в log-файлах компонента.

### Автоматический запуск компонента при старте ОС

Если сервис успешно запустился, то можно добавить его в автоматический запуск после загрузки ОС. Для этого выполним команду:  
```bash
sudo systemctl enable search-api.service
```
