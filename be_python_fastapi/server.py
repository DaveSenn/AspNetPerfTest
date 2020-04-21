import os
from databases import Database
from fastapi import FastAPI
# from fastapi.middleware.cors import CORSMiddleware
from models import tasks


app = FastAPI()
database = Database(
    'postgresql://{}:{}@{}/todolist'.format(
        os.environ['DEV_PG_USER'], os.environ['DEV_PG_PASSWORD'],
        os.environ['DEV_PG_HOST']
    ),
    min_size=5, max_size=20
)

@app.on_event('startup')
async def startup():
    await database.connect()


@app.on_event('shutdown')
async def shutdown():
    await database.disconnect()

@app.delete('/tasks')
async def tasks_delete():
    await database.execute(query=tasks.delete())
    return {'status': 'success'}


@app.get('/status')
async def status_get():
    return 'ok'


@app.get('/tasks')
async def tasks_get():
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
    return results


@app.post('/tasks')
async def tasks_post(request_json):
    values = {
        'text': request_json['text'],
        'priority': int(request_json['priority'])
    }
    result = await database.execute(query=tasks.insert(), values=values)
    values['id'] = result
    return {'task': values}


@app.put('/tasks')
async def tasks_update(request_json):
    t_id = int(request_json['id'])
    values = {
        'text': request_json['text'],
        'priority': int(request_json['priority'])
    }
    values['id'] = t_id
    await database.execute(
        query=tasks.update().where(tasks.c.id == t_id), values=values)
    return {'task': values}
