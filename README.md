## To Do List App

A simple To Do List app with multiple backends.

Each backend supports basic CRUD and enables permissive CORS for easy local dev.
The frontend is independent of the backend used as long as it implements the 
correct API.
The tests are also independend of the backend used - so adding and validating a
new backend is easy.

### frontend:
Javascript/Markup - React **fe_javascript_react** (run with `npm start`)

### backends:

C# - Dotnet Core/Kestrel - **be_csharp_dotnet_core** (`dotnet run` or `dotnet build --configuration Release;bin/Release/netcoreapp3.0/TodoList`)

F# - Dotnet Core/Kestrel - **be_fsharp_dotnet_core** (`dotnet run` or `dotnet build --configuration Release;bin/Release/netcoreapp3.0/TodoList`)

Go - **be_go** (`go run server.go`)

Javascript - Fastify **be_javascript_fastify** (`npm install`; `node server.js`)

Python 3 - Sanic **be_python_sanic** (`pip install -r requirements.txt`; `python server.py`)

Python 3 - FastAPI **be_python_fastapi** (`pip install -r requirements.txt`; `uvicorn server:app`)

Rust - Actix-Web **be_rust_actix** (`cargo run` or `cargo build --release;target/release/be_actix`)

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

If you get a `cannot find -lpq` error when running `cargo`.

    sudo apt install libpq-dev

Optionally run `python initialize_db.py` in database/ to create the tasks table.
(Or create the table yourself in your preferred manner).
Note on some systems you made need to run the equivalent of:

### running:
Start the db (bash database/run.sh).
Pick a backend, run it, then start up the front end and away you go.

### tests:
While in many cases writing tests within the context of your app makes the most
sense, it's interesting to think about writing tests that, like your frontend,
are decoupled from your backend. This let's you change up your backend freely 
and still have some tests in place, whether you are refactoring or doing 
something drastic like switching frameworks.

In the case of this specific project, I found it wonderful to be able to add a
new feature in each language/framework or refactor and have tests ready to go.

Of course one still needs to consider mocking, test databases, and other
concerns I didn't feel like putting the time into for something like this.

Run `py.test` in the `tests` directory after `pip install -r requirements.txt`

### status:
This is just for messing around. I might spruce up the error handling, tests,
or add some other backends later on down the road. Or even style the frontend.
