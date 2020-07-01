package org.todolist

import com.github.jasync.sql.db.Configuration
import com.github.jasync.sql.db.ConnectionPoolConfiguration
import com.github.jasync.sql.db.pool.ConnectionPool
import com.github.jasync.sql.db.postgresql.pool.PostgreSQLConnectionFactory
import io.jooby.CorsHandler
import io.jooby.MediaType
import io.jooby.cors
import io.jooby.json.GsonModule
import io.jooby.runApp
import java.util.concurrent.TimeUnit

data class Task (
    val id : Int?,
    val text : String?,
    val priority : Int?
)

fun main(args: Array<String>) {
    val host = System.getenv("DEV_PG_HOST")
    val port = System.getenv("DEV_PG_PORT")
    val user = System.getenv("DEV_PG_USER")
    val pass = System.getenv("DEV_PG_PASSWORD")
    val poolConfiguration = ConnectionPoolConfiguration(
        maxActiveConnections = 20,
        maxIdleTime = TimeUnit.MINUTES.toMillis(15),
        maxPendingQueries = 10000
    )
    val connection = ConnectionPool(
        PostgreSQLConnectionFactory(
            Configuration(
                username=user,
                password=pass,
                host=host,
                port=port.toInt(),
                database="todolist"
            )), poolConfiguration
    )
    runApp(args) {
        val corsOptions = cors {
            methods = listOf("GET", "POST", "PUT", "DELETE")
        }
        decorator(CorsHandler(corsOptions))
        install(GsonModule())
        delete("/tasks") {
            val future = connection.sendPreparedStatement("DELETE FROM tasks;")
            future.get()
            "ok"
        }
        get ("/status") {
            "ok"
        }
        get ("/tasks") {
            val future = connection.sendPreparedStatement("SELECT * FROM tasks ORDER BY priority asc")
            val result = future.get()
            val taskList = mutableListOf<Task>()
            for (row in result.rows) {
                taskList.add(Task(
                    id=row.getInt("id"),
                    text=row.getString("text"),
                    priority=row.getInt("priority")
                ))
            }
            val tasks = mapOf(
                "tasks" to taskList,
                "position" to 0,
                "length" to result.rows.size
            )
            ctx.responseType = MediaType.json
            tasks
        }
        post ("/tasks") {
            val task = ctx.body(Task::class.java)
            val sql = "INSERT INTO tasks (text, priority) VALUES (?, ?)"
            val future = connection.sendPreparedStatement(sql, listOf(task.text, task.priority))
            future.get()
            mapOf("task" to task)
        }
        put ("/tasks") {
            val task = ctx.body(Task::class.java)
            val sql = "UPDATE tasks SET text = ?, priority = ? WHERE id = ?"
            val future = connection.sendPreparedStatement(sql, listOf(task.text, task.priority, task.id))
            future.get()
            mapOf("task" to task)
        }
        onStop {
            connection.disconnect().get()
        }
    }
}