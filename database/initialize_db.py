import os
from sqlalchemy import create_engine
import sys
sys.path.append('../be_python_sanic/')
from models import metadata

engine = create_engine('postgresql://{}:{}@{}:{}/{}'.format(
    os.environ['DEV_PG_USER'],
    os.environ['DEV_PG_PASSWORD'],
    os.environ['DEV_PG_HOST'],
    os.environ['DEV_PG_PORT'],
    os.environ['DEV_PG_DATABASE']
))
metadata.create_all(engine)
