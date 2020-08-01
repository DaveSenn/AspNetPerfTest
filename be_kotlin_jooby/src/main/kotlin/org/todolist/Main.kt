package org.todolist

import com.github.jasync.sql.db.Configuration
import com.github.jasync.sql.db.ConnectionPoolConfiguration
import com.github.jasync.sql.db.pool.ConnectionPool
import com.github.jasync.sql.db.postgresql.pool.PostgreSQLConnectionFactory
import io.jooby.*
import io.jooby.json.GsonModule
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
            val limit = 10
            val page = (ctx.query.get("page")).intValue(1)
            val offset = ((page - 1) * limit)
            val sql = ("SELECT * FROM tasks ORDER BY priority asc OFFSET "
                        + offset.toString() + " LIMIT " + limit.toString())
            val future = connection.sendPreparedStatement(sql)
            val result = future.get()
            val taskList = result.rows.map {
                Task(it.getInt("id"), it.getString("text"), it.getInt("priority"))
            }
            val tasks = mapOf(
                "tasks" to taskList,
                "position" to offset,
                "page" to page
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