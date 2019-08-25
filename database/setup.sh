docker pull postgres
mkdir -p ~/docker/volumes/postgres
createdb -h localhost -p 5432 -U tdl_user todolist