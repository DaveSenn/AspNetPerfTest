export PGPASSWORD=$DEV_PG_PASSWORD
createdb -h $DEV_PG_HOST -p 5432 -U $DEV_PG_USER todolist