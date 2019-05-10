from sqlalchemy import create_engine
from models import metadata

engine = create_engine('postgresql://tdl_user:secretz@localhost/todolist')
metadata.create_all(engine)
