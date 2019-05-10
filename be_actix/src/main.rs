#[macro_use]
extern crate diesel;
use actix_web::{delete, get, http, post, put, web, App, HttpRequest, 
    HttpResponse, HttpServer};
use actix_web::web::Json;
use actix_web::middleware::cors::Cors;
use diesel::prelude::*;
use diesel::pg::PgConnection;
use diesel::r2d2::{self, ConnectionManager};
use serde_json::{self, json};
use serde_derive::{Deserialize, Serialize};
use std::env;

mod models;
mod schema;

type Pool = r2d2::Pool<ConnectionManager<PgConnection>>;

#[derive(Serialize)]
struct Results {
    position: i32,
    length: i32,
    tasks: Vec<Task>
}

#[derive(Deserialize, Serialize)]
struct Task {
    id: i32,
    text: String,
    priority: i32
}

#[derive(Deserialize)]
struct TaskPost {
    text: String,
    priority: i32
}

#[delete("/tasks")]
fn tasks_delete(pool: web::Data<Pool>) -> HttpResponse {
    use self::schema::tasks::dsl::*;
    let conn: &PgConnection = &pool.get().unwrap();
    diesel::delete(tasks).execute(conn).unwrap();
    HttpResponse::Ok().json(json!({"status": "success"}))
}

#[get("/tasks")]
fn tasks_get(_req: HttpRequest, pool: web::Data<Pool>) -> HttpResponse {
    use self::schema::tasks::dsl::*;
    let conn: &PgConnection = &pool.get().unwrap();
    let mut rows = tasks.order(priority.asc())
                        .load::<models::Task>(conn).unwrap();
    let length = rows.len() as i32;
    let mut tasks_list: Vec<Task> = Vec::new();
    for row in rows {
        let task = Task {
            id: row.id,
            text: row.text,
            priority: row.priority
        };
        tasks_list.push(task);
    }
    let results = Results {
        position: 0,
        length: length,
        tasks: tasks_list
    };
    HttpResponse::Ok().json(json!(results))
}

#[post("/tasks")]
fn tasks_post(task: Json<TaskPost>, pool: web::Data<Pool>) -> HttpResponse {
    use self::schema::tasks::dsl::*;
    let conn: &PgConnection = &pool.get().unwrap();
    let new_task = models::NewTask {
        text: &task.text,
        priority: &task.priority
    };
    let row_id: i32 = diesel::insert_into(tasks).values(&new_task)
        .returning(id).get_result(conn).unwrap();
    HttpResponse::Ok().json(json!({
        "task": {
            "id": row_id,
            "text": task.text,
            "priority": task.priority
        }
    }))
}

#[put("/tasks")]
fn tasks_put(task: Json<Task>, pool: web::Data<Pool>) -> HttpResponse {
    use self::schema::tasks::dsl::*;
    let conn: &PgConnection = &pool.get().unwrap();
    diesel::update(tasks.filter(id.eq(task.id)))
        .set((text.eq(&task.text), priority.eq(&task.priority)))
        .execute(conn).unwrap();
    HttpResponse::Ok().json(json!({
        "task": {
            "id": task.id,
            "text": task.text,
            "priority": task.priority
        }
    }))
}

fn main() -> std::io::Result<()> {
    let connection_string = format!("postgresql://{}:{}@{}/todolist",
        env::var("DEV_PG_USER").unwrap(),
        env::var("DEV_PG_PASSWORD").unwrap(),
        env::var("DEV_PG_HOST").unwrap());
    let manager = ConnectionManager::<PgConnection>::new(connection_string);
    let pool = r2d2::Pool::builder()
        .build(manager)
        .expect("Failed to create pool.");
    println!("Server listening on 8000");
    HttpServer::new(move || App::new()
        .data(pool.clone())
        .wrap(
            Cors::new()
            .allowed_origin("http://localhost:3000")
            .allowed_methods(vec!["DELETE", "GET", "POST"])
            .allowed_headers(vec![http::header::AUTHORIZATION,
                http::header::ACCEPT])
            .allowed_header(http::header::CONTENT_TYPE)
            .max_age(3600),)
        .service(tasks_delete)
        .service(tasks_get)
        .service(tasks_post)        
        .service(tasks_put))
        .bind("127.0.0.1:8000")?
        .run()
}