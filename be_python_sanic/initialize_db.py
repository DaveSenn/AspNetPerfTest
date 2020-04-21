import os
from sqlalchemy import create_engine
from models import metadata

engine = create_engine('postgresql://{}:{}@{}/todolist'.format(
    os.environ('DEV_PG_USER'),
    os.environ('DEV_PG_PASSWORD'),
    os.environ('DEV_PG_HOST')
))
metadata.create_all(engine)
