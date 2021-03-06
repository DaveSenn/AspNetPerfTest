const fastify = require('fastify')({ logger: false })
const connectionString = 'postgres://' + process.env.DEV_PG_USER + ':' + 
  process.env.DEV_PG_PASSWORD + '@' + process.env.DEV_PG_HOST + '/todolist'

// fastify-postgres uses pooling under the hood.
fastify.register(require('fastify-postgres'), {
  connectionString: connectionString
})

fastify.register(require('fastify-cors'), { 
  'origin': true,
})

fastify.delete('/tasks', async (request, reply) => {
  const client = await fastify.pg.connect()
  await client.query('DELETE FROM tasks')
  client.release()
  return { status: 'success' }
})

fastify.get('/status', async (request, reply) => {
  reply.send('ok')
})

fastify.get('/tasks', async (request, reply) => {
  var limit = 10
  var page = 1;
  if (request.query["page"] != null) { page = parseInt(request.query["page"]); }
  var offset = ((page - 1) * limit);
  const client = await fastify.pg.connect()
  var sql = 'SELECT id, text, priority FROM tasks ORDER BY priority OFFSET ' + offset.toString() + " LIMIT " + limit.toString()
  const { rows } = await client.query(sql)
  client.release()
  var results = {tasks: rows, position: offset, page: page}
  reply.send(results)
})

fastify.post('/tasks', async (request, reply) => {
  const client = await fastify.pg.connect()
  const id = await client.query('INSERT INTO tasks (text, priority) VALUES ($1, $2)', 
    [request.body.text, request.body.priority],)
  client.release()
  return { task: 
      { id: id, text: request.body.text, priority: request.body.priority} }
})

fastify.put('/tasks', async (request, reply) => {
  const client = await fastify.pg.connect()
  await client.query('UPDATE tasks SET text = $1, priority = $2 WHERE id = $3',
    [request.body.text, request.body.priority, request.body.id],)
  client.release()
  return { task: 
      { id:request.body.id , text: request.body.text, 
        priority: request.body.priority} }
})

const start = async () => {
  try {
    await fastify.listen(8000)
    fastify.log.info(`Server listening on ${fastify.server.address().port}`)
  } catch (err) {
    fastify.log.error(err)
    process.exit(1)
  }
}
start()