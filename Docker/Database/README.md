# Первый запуск

Для запуска базы данных **qa_search** в докер контейнере необходимо:
- Собрать образ:
    ```sh
    docker build -t qa-search-mssql .
    ```
- Запустить контейнер из образа:
    ```sh
    docker run -p 1433:1433 -d --name=qa-search-mssql qa-search-mssql
    ```
Теперь можно использовать локальный порт **1433** для подключения к базе данных.
Имя пользователя: **SA**
Пароль указан в **Dockerfile**
```
ENV SA_PASSWORD <пароль>
```

# Обновление базы данных
После внесения изменений в схему базы данных (например, после редактирования *create-db.sql*) необходимо:
- Остановить контейнер с предыдущей версией БД:
    ```sh
    docker container stop qa-search-mssql
    ```
- Удалить его:
    ```sh
    docker container rm qa-search-mssql
    ```
- Выполнить действия из раздела "Первый запуск".

# Полезные команды
Удаление неиспользуемых данных:
- all stopped containers;
- all dangling images;
- all unused networks.
```sh
docker system prune
```
