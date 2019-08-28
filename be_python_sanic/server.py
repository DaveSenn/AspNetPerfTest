import os
from databases import Database
from sanic import Sanic
from sanic.response import json
from models import tasks
from sanic_cors import CORS

app = Sanic()
CORS(app, automatic_options=True)
database = Database(
    'postgresql://{}:{}@{}/todolist'.format(
        os.environ['DEV_PG_USER'], os.environ['DEV_PG_PASSWORD'],
        os.environ['DEV_PG_HOST']
    ),
    min_size=5, max_size=20
)


@app.listener('before_server_start')
async def setup_db(app, loop):
    await database.connect()


@app.listener('after_server_stop')
async def close_db(app, loop):
    await database.disconnect()


@app.route('/tasks', methods=['DELETE'])
async def tasks_delete(request):
    await database.execute(query=tasks.delete())
    return json({'status': 'success'})


@app.route('/tasks', methods=['GET'])
async def tasks_get(request):
    rows = await database.fetch_all(query=tasks.select().order_by(
        tasks.c.priority.asc()))
    results = {
        'tasks': [],
        'position': 0,
        'length': len(rows),
    }
    for row in rows:
        results['tasks'].append({
            'id': row[0],
            'text': row[1],
            'priority': row[2]
        })
    return json(results)


@app.route('/tasks', methods=['POST'])
async def tasks_post(request):
    values = {
        'text': request.json['text'],
        'priority': int(request.json['priority'])
    }
    result = await database.execute(query=tasks.insert(), values=values)
    values['id'] = result
    return json({'task': values})


@app.route('/tasks', methods=['PUT'])
async def tasks_update(request):
    t_id = int(request.json['id'])
    values = {
        'text': request.json['text'],
        'priority': int(request.json['priority'])
    }
    values['id'] = t_id
    await database.execute(
        query=tasks.update().where(tasks.c.id == t_id), values=values)
    return json({'task': values})

if __name__ == '__main__':
    print('Server listening on 8000')
    app.run(host='0.0.0.0', port=8000, workers=4, debug=False, access_log=False)
