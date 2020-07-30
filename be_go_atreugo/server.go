package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"os"

	_ "github.com/lib/pq"
	"github.com/savsgio/atreugo/v11"
)

type Task struct {
	Id       int    `json:"id"`
	Text     string `json:"text"`
	Priority int    `json:"priority"`
}

func connect() *sql.DB {
	connectionString := fmt.Sprintf("host=%s user=%s password=%s "+
		" dbname=todolist sslmode=disable", os.Getenv("DEV_PG_HOST"),
		os.Getenv("DEV_PG_USER"), os.Getenv("DEV_PG_PASSWORD"))
	db, err := sql.Open("postgres", connectionString)
	if err != nil {
		panic(err)
	}
	return db
}

func add_task(db *sql.DB, ctx *atreugo.RequestCtx) error {
	var task Task
	json.Unmarshal(ctx.PostBody(), &task)
	statement := `INSERT INTO tasks (text, priority) VALUES ($1, $2) RETURNING id`
	err := db.QueryRow(statement, task.Text, task.Priority).Scan(&task.Id)
	if err != nil {
		panic(err)
	}
	result := map[string]interface{}{}
	result["task"] = task
	return ctx.JSONResponse(result)
}

func delete_tasks(db *sql.DB, ctx *atreugo.RequestCtx) error {
	statement := `DELETE FROM tasks`
	_, err := db.Exec(statement)
	if err != nil {
		panic(err)
	}
	resp := map[string]string{"status": "ok"}
	return ctx.JSONResponse(resp)
}

func get_tasks(db *sql.DB, ctx *atreugo.RequestCtx) error {
	results := map[string]interface{}{}
	var tasks []Task
	rows, err := db.Query(`SELECT id, text, priority FROM tasks ORDER BY priority`)
	if err != nil {
		panic(err)
	}
	defer rows.Close()
	for rows.Next() {
		task := Task{}
		err = rows.Scan(&task.Id, &task.Text, &task.Priority)
		if err != nil {
			panic(err)
		}
		tasks = append(tasks, task)
	}
	results["tasks"] = tasks
	results["position"] = 0
	results["len"] = len(tasks)
	return ctx.JSONResponse(results)
}

func update_task(db *sql.DB, ctx *atreugo.RequestCtx) error {
	var task Task
	json.Unmarshal(ctx.PostBody(), &task)
	statement := `UPDATE tasks SET text = $1, priority = $2 WHERE id = $3`
	_, err := db.Exec(statement, task.Text, task.Priority, task.Id)
	if err != nil {
		panic(err)
	}
	result := map[string]interface{}{}
	result["task"] = task
	return ctx.JSONResponse(result)
}

func main() {
	connection := connect()
	connection.SetMaxOpenConns(10)
	connection.SetMaxIdleConns(10)
	defer connection.Close()
	config := atreugo.Config{
		Addr: "0.0.0.0:8000",
	}
	server := atreugo.New(config)
	server.GET("/status", func(ctx *atreugo.RequestCtx) error {
		return ctx.TextResponse("ok")
	})
	server.DELETE("/tasks", func(ctx *atreugo.RequestCtx) error {
		return delete_tasks(connection, ctx)
	})
	server.GET("/tasks", func(ctx *atreugo.RequestCtx) error {
		return get_tasks(connection, ctx)
	})
	server.POST("/tasks", func(ctx *atreugo.RequestCtx) error {
		return add_task(connection, ctx)
	})
	server.PUT("/tasks", func(ctx *atreugo.RequestCtx) error {
		return update_task(connection, ctx)
	})
	fmt.Println("Server listening on 8000")
	if err := server.ListenAndServe(); err != nil {
		panic(err)
	}
}
