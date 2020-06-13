import json
import os
import falcon
from sqlalchemy import create_engine
from models import tasks

app = falcon.App(cors_enable=True)

engine = create_engine('postgresql://{}:{}@{}/todolist'.format(
    os.environ['DEV_PG_USER'],
    os.environ['DEV_PG_PASSWORD'],
    os.environ['DEV_PG_HOST']
))


class StatusResource:
    def on_get(self, req, resp):
        resp.status = falcon.HTTP_200
        resp.content_type = 'text'
        resp.body = 'ok'


class TaskResource:

    def __init__(self, engine):
        self.engine = engine

    def on_delete(self, req, resp):
        self.engine.execute(query=tasks.delete())
        resp.status = falcon.HTTP_200
        resp.body = json.dumps({'status': 'success'})

    def on_get(self, req, resp):
        rows = self.engine.execute(
            tasks.select().order_by(tasks.c.priority.asc())
        ).fetchall()
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
        resp.status = falcon.HTTP_200
        resp.body = json.dumps(results)

    def on_post(self, req, resp):
        data = json.load(req.stream)
        values = {
            'text': data['text'],
            'priority': int(data['priority'])
        }
        result = self.engine.execute(tasks.insert(), values=values)
        values['id'] = result
        resp.status = falcon.HTTP_200
        resp.body = json.dumps({'task': values})

    def on_put(self, req, resp):
        data = json.load(req.stream)
        t_id = int(data['id'])
        values = {
            'text': data['text'],
            'priority': int(data['priority'])
        }
        values['id'] = t_id
        self.engine.execute(
            tasks.update().where(tasks.c.id == t_id), values=values)
        resp.status = falcon.HTTP_200
        resp.body = json.dumps({'task': values})


statusr = StatusResource()
taskr = TaskResource(engine)

app.add_route('/status', statusr)
app.add_route('/tasks', taskr)
