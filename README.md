## To Do List App

A simple To Do List app with multiple backends.

Each backend supports basic CRUD.
The frontend is independent of the backend used as long as it implements the 
correct API.
The tests are also independend of the backend used.

Setup is local because I am too lazy to wrestle Docker for this project.

### frontend:
Javascript/Markup - React **fe_react** (run with `npm start`)

### backends:
Python 3 - Sanic **be_sanic** (`pip install -r requirements.txt`; `python server.py`)

Javascript - Fastify **be_fastify** (`npm install`; `node server.js`)

Go - **be_go** (`go run server.go`)

Rust - Actix-Web **be_actix** (`cargo run` or `cargo build --release;target/release/todolist`)

### setup:

Install requirements locally:

    Postgresql 10.0+ (Though any 9.x should be fine)
    Node 10.15+
    Python 3.6+
        Pip 9.0+
    Go 1.12+
    Rust 1.34+

Setup postgres locally with a database (todolist) and user.
Set environment variables for the user/password/host:
    
    DEV_PG_USER
    DEV_PG_PASSWORD
    DEV_PG_HOST

Note on some systems you made need to run the equivalent of:

    sudo apt install libpq-dev

If you get a `cannot find -lpq` error when running `cargo`.

Optionally run `python initialize_db.py` in be_sanic to create the tasks table.

### running:
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
