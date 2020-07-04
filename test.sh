# Pass in test command to run:
# dotnet run --project tests/fs_expecto/fs_expecto.fsproj 
# py.test
echo 'Setting Test Env Variables'
export O_PG_DATABASE=$DEV_PG_DATABASE
export DEV_PG_DATABASE="test_todolist"
export O_DB_CONTAINE=$DEV_PG_CONTAINER
export DEV_PG_CONTAINER="test-todolist"
cd database
echo 'Stopping Test Container Just in Case'
./stop.sh # in case already running
echo 'Running Test Container'
./run.sh
echo 'Sleep for Docker (1)' # Docker tends to need a second.
sleep 1
echo 'Creating Database'
./create_db.sh
echo 'Initialize Database'
python initialize_db.py
cd ..
echo 'Running Tests'
$@
cd database
echo 'Dropping Test Database'
./drop_db.sh
echo 'Stopping Test Container'
./stop.sh
echo 'Resetting Env Variables'
export DEV_PG_DATABASE=$O_PG_DATABASE
export DEV_DB_CONTAINER=$O_DB_CONTAINER