package TodoList

import com.github.jasync.sql.db.Configuration
import com.github.jasync.sql.db.ConnectionPoolConfiguration
import com.github.jasync.sql.db.pool.ConnectionPool
import com.github.jasync.sql.db.postgresql.pool.PostgreSQLConnectionFactory
import io.ktor.server.netty.Netty
import io.ktor.routing.*
import io.ktor.application.*
import io.ktor.http.*
import io.ktor.response.*
import io.ktor.server.engine.*
import io.ktor.request.receive
import java.util.concurrent.TimeUnit
import com.google.gson.*

data class TodoTask (
    val id : Int?,
    val text : String?,
    val priority : Int?
)

fun main(args: Array<String>) {
    val host = System.getenv("DEV_PG_HOST")
    val port = System.getenv("DEV_PG_PORT")
    val user = System.getenv("DEV_PG_USER")
    val pass = System.getenv("DEV_PG_PASSWORD")
    val db = System.getenv("DEV_PG_DATABASE")
    val poolConfiguration = ConnectionPoolConfiguration(
        maxActiveConnections = 20,
        maxIdleTime = TimeUnit.MINUTES.toMillis(15),
        maxPendingQueries = 10000
    )
    val gson = Gson()
    val connection = ConnectionPool(
        PostgreSQLConnectionFactory(
            Configuration(
                username=user,
                password=pass,
                host=host,
                port=port.toInt(),
                database=db
            )
        ), poolConfiguration
    )
    embeddedServer(Netty, 8000) {
        routing {
            get("/status") {
                call.respondText("ok", ContentType.Text.Plain)
            }
            get("/tasks") {
                val future = connection.sendPreparedStatement("SELECT * FROM tasks ORDER BY priority asc")
                val result = future.get()
                val taskList = result.rows.map {
                    TodoTask(it.getInt("id"), it.getString("text"), it.getInt("priority"))
                }
                val tasks = mapOf(
                    "tasks" to taskList,
                    "position" to 0,
                    "length" to result.rows.size
                )
                call.respondText(gson.toJson(tasks), contentType=ContentType.Application.Json)
            }
            post ("/tasks") {
                val task = call.receive<TodoTask>()
                val sql = "INSERT INTO tasks (text, priority) VALUES (?, ?)"
                val future = connection.sendPreparedStatement(sql, listOf(task.text, task.priority))
                future.get()
                call.respondText(gson.toJson(mapOf("task" to task)), contentType=ContentType.Application.Json)
            }
            put ("/tasks") {
                val task = call.receive<TodoTask>()
                val sql = "UPDATE tasks SET text = ?, priority = ? WHERE id = ?"
                val future = connection.sendPreparedStatement(sql, listOf(task.text, task.priority, task.id))
                future.get()
                call.respondText(gson.toJson(mapOf("task" to task)), contentType=ContentType.Application.Json)
            }
        }
        environment.monitor.subscribe(ApplicationStopping){
            connection.disconnect().get()
        }
    }.start(wait = true)
}