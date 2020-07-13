package EndpointTests
import java.sql.Connection
import java.sql.DriverManager
import java.util.*

data class TodoTask (
        val id : Int,
        val text : String,
        val priority : Int
)

data class TodoList (
        val tasks : List<TodoTask>,
        val position: Int,
        val length : Int
)

class TodoData {
    fun connect() : Connection {
        val props = Properties()
        with (props) {
            put("user", System.getenv("DEV_PG_USER"))
            put("password", System.getenv("DEV_PG_PASSWORD"))
        }
        return DriverManager.getConnection("jdbc:postgresql://%s:%s/%s".format(
                System.getenv("DEV_PG_HOST"),
                System.getenv("DEV_PG_PORT"),
                System.getenv("DEV_PG_DATABASE")
        ), props)
    }

    fun addTask(text: String, priority: Int) {
        val conn = connect()
        val stmt = conn.prepareStatement("INSERT INTO tasks (text, priority) VALUES (?, ?)")
        stmt.setString(1, text)
        stmt.setInt(2, priority)
        stmt.execute()
        conn.close()
    }
}