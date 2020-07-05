## To Do List App

A simple Todo List app with multiple backends.

Each backend supports basic CRUD and enables permissive CORS for easy local dev.
The frontend is independent of the backend used as long as it implements the 
correct API.
The tests are also independend of the backend used - so adding and validating a
new backend is easy.

### frontend:
Javascript/Markup - React **fe_javascript_react** (run with `npm start`)

### backends:

C# - Dotnet Core/Kestrel - **be_csharp_dotnet_core** (`dotnet run` or `dotnet build --configuration Release;bin/Release/netcoreapp[version]/TodoList`)

F# - Dotnet Core/Kestrel - **be_fsharp_dotnet_core** (`dotnet run` or `dotnet build --configuration Release;bin/Release/netcoreapp[version]/TodoList`)

F# - Giraffe/Dotnet Core/Kestrel - **be_fsharp_giraffe** (`dotnet run` or `dotnet build --configuration Release;bin/Release/netcoreapp[version]/TodoList`)

Javascript - Fastify **be_javascript_fastify** (`npm install`; `node server.js`)

Kotlin - Jooby (Kooby) - **be_kotlin_jooby** (`gradle build`; `java -jar build/libs/TodoList-<version>.jar`)

Python - Sanic **be_python_sanic** (`pip install -r requirements.txt`; `python server.py`)

Python - Falcon **be_python_falcon** (`pip install -r requirements.txt`;`gunicorn --workers=8 --worker-class="egg:meinheld#gunicorn_worker" server:app`)

Python - FastAPI **be_python_fastapi** (`pip install -r requirements.txt`; `uvicorn server:app`)

### setup:

Install requirements locally:

    Docker
    Dotnet Core 3.0
    Go 1.12+
    Python 3.6+
        Pip 9.0+
    Node 10.15+
    Rust 1.34+

Setup postgres via docker (in the database directory).

    bash setup.sh
    bash run.sh
    bash stop.sh

Set environment variables for the user/password/host:
    
    DEV_PG_USER
    DEV_PG_PASSWORD
    DEV_PG_HOST
    DEV_PG_PORT
    DEV_PG_DATABASE
    DEV_PG_CONTAINER

If you get a `cannot find -lpq` error when running `cargo`.

    sudo apt install libpq-dev

Optionally run `python initialize_db.py` in database/ to create the tasks table.
(Or create the table yourself in your preferred manner).

You will need to install the Python headers (`apt install python-dev`) for the version of python you're running with Falcon.

### running:
Start the db (database/run.sh).
Pick a backend, run it, then start up the front end and away you go.

### tests:
While in many cases writing tests within the context of your app makes the most
sense, it's interesting to think about writing tests that, like your frontend,
are decoupled from your backend. This let's you change up your backend freely 
and still have some tests in place, whether you are refactoring or doing 
something drastic like switching frameworks.

In the case of this specific project, I found it wonderful to be able to add a
new feature in each language/framework or refactor and have tests ready to go.

**Note: You will need to run one of the backends to run these tests.**

The test script will setup, run, and teardown the database for you.

Run `test.sh` as follows:

```
./test.sh dotnet run --project tests/fs_expecto/fs_expecto.fsproj 
```

or

```
./test.sh py.test
```

### status:
This is just for messing around. I might spruce up the error handling, tests,
or add some other backends later on down the road. Or even style the frontend.
