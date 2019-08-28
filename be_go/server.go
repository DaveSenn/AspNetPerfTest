package main

import (
	"fmt"
	"database/sql"
  	"encoding/json"
	"net/http"
	"os"
	_ "github.com/lib/pq"
    "github.com/rs/cors"
)

type Task struct {
	Id         int 		`json:"id"`
	Text       string 	`json:"text"`
	Priority   int 		`json:"priority"`
}

func connect() *sql.DB {
	connectionString := fmt.Sprintf("host=%s user=%s password=%s " +
	" dbname=todolist sslmode=disable", os.Getenv("DEV_PG_HOST"),
    os.Getenv("DEV_PG_USER"), os.Getenv("DEV_PG_PASSWORD"))
	db, err := sql.Open("postgres", connectionString)
	if err != nil { panic(err) }
	return db
}

func add_task(db *sql.DB, w http.ResponseWriter, r *http.Request) {
	decoder := json.NewDecoder(r.Body)
	var task Task
	err := decoder.Decode(&task)
	if err != nil { panic(err) }
	statement := `INSERT INTO tasks (text, priority) VALUES ($1, $2) RETURNING id`
	err = db.QueryRow(statement, task.Text, task.Priority).Scan(&task.Id)
	if err != nil { panic(err) }
	results := map[string]interface{}{}
	results["task"] = task
	j, err := json.Marshal(results)
	if err != nil { panic(err) }
	w.Header().Set("Content-Type", "application/json")
	w.Write(j)
}

func delete_tasks(db *sql.DB, w http.ResponseWriter, r *http.Request) {
	statement := `DELETE FROM tasks`
	_, err := db.Exec(statement)
	if err != nil { panic(err) }
	resp := map[string]string{"status": "ok"}
	j, err := json.Marshal(resp)
	if err != nil { panic(err) }
	w.Header().Set("Content-Type", "application/json")
	w.Write(j)
}

func get_tasks(db *sql.DB, w http.ResponseWriter, r *http.Request) {
	results := map[string]interface{}{}
	var tasks []Task
	rows, err := db.Query(`
		SELECT id, text, priority FROM tasks ORDER BY priority`)
	if err != nil { panic(err) }
	defer rows.Close()
	for rows.Next() {
		task := Task{}
		err = rows.Scan(&task.Id, &task.Text, &task.Priority)
		if err != nil { panic(err) }
		tasks = append(tasks, task)
	}
	results["tasks"] = tasks
	results["position"] = 0
	results["len"] = len(tasks)
	j, err := json.Marshal(results)
	if err != nil { panic(err) }
	w.Header().Set("Content-Type", "application/json")
	w.Write(j)
}

func update_task(db *sql.DB, w http.ResponseWriter, r *http.Request) {
	decoder := json.NewDecoder(r.Body)
	var task Task
	err := decoder.Decode(&task)
	if err != nil { panic(err) }
	statement := `UPDATE tasks SET text = $1, priority = $2 WHERE id = $3`
	_, err = db.Exec(statement, task.Text, task.Priority, task.Id)
	if err != nil { panic(err) }
	results := map[string]interface{}{}
	results["task"] = task
	j, err := json.Marshal(results)
	if err != nil { panic(err) }
	w.Header().Set("Content-Type", "application/json")
	w.Write(j)
}

func main() {
	connection := connect()
    connection.SetMaxOpenConns(5)
    connection.SetMaxIdleConns(5)
	defer connection.Close()
	mux := http.NewServeMux()
	mux.HandleFunc("/tasks", func (w http.ResponseWriter, r *http.Request) {
		switch  r.Method {
		case "DELETE":
			delete_tasks(connection, w, r)
		case "GET":
			get_tasks(connection, w, r)
		case "POST":
			add_task(connection, w, r)
		case "PUT":
			update_task(connection, w, r)
		}
	})
	handler := cors.New(cors.Options{
        AllowedOrigins:   []string{"*"},
        AllowedMethods:   []string{
        	http.MethodGet, http.MethodPost, http.MethodDelete},
        AllowCredentials: true,
    }).Handler(mux)
    fmt.Println("Server listening on 8000")
	http.ListenAndServe(":8000", handler)
}