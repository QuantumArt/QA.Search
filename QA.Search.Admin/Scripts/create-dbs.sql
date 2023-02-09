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