import sqlalchemy

metadata = sqlalchemy.MetaData()

tasks = sqlalchemy.Table(
    'tasks',
    metadata,
    sqlalchemy.Column('id', sqlalchemy.Integer, primary_key=True),
    sqlalchemy.Column('text', sqlalchemy.String()),
    sqlalchemy.Column('priority', sqlalchemy.Integer),
)
