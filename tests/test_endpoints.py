import os
import pytest
import sqlalchemy
import requests

metadata = sqlalchemy.MetaData()

tasks = sqlalchemy.Table(
    'tasks',
    metadata,
    sqlalchemy.Column('id', sqlalchemy.Integer, primary_key=True),
    sqlalchemy.Column('text', sqlalchemy.String()),
    sqlalchemy.Column('priority', sqlalchemy.Integer),
)

task_ids = []


def add_task(connection, text='Super important task', priority=1):
    result = connection.execute(tasks.insert().values(
        text=text, priority=priority))
    task_id = result.inserted_primary_key[0]
    task_ids.append(task_id)
    return task_id


@pytest.fixture(scope='module')
def db_connection():
    # Setup
    database = sqlalchemy.create_engine(
        'postgresql://{}:{}@{}/todolist'.format(
            os.environ['DEV_PG_USER'], os.environ['DEV_PG_PASSWORD'],
            os.environ['DEV_PG_HOST']
        )
    )
    connection = database.connect()
    yield connection
    # Teardown
    print('Deleting: {}'.format(task_ids))
    connection.execute(tasks.delete().where(tasks.c.id.in_(task_ids)))
    connection.close()


# def test_delete(db_connection):
#     add_task(db_connection)
#     r = requests.delete('http://localhost:8000/tasks')
#     assert r.status_code == 200


def test_get(db_connection):
    add_task(db_connection, text='elephants')
    r = requests.get('http://localhost:8000/tasks')
    tasks = r.json()['tasks']
    found = False
    for task in tasks:
        if 'elephants' == task['text']:
            found = True
    assert found


def test_post(db_connection):
    r = requests.post(
        'http://localhost:8000/tasks',
        json={
            'text': 'A new task!',
            'priority': 2
        })
    assert r.status_code == 200
    assert r.json()['task']['priority'] == 2


def test_put(db_connection):
    task_id = add_task(db_connection, 'wow', 3)
    r = requests.put(
        'http://localhost:8000/tasks',
        json={
            'id': task_id,
            'text': 'wowzers!',
            'priority': 2
        })
    assert r.status_code == 200
    assert r.json()['task']['priority'] == 2
