import React from 'react';
import './App.css';

function sleep (time) {
  return new Promise((resolve) => setTimeout(resolve, time));
}

class ToDoListApp extends React.Component {
  constructor(props) {
    super(props);
    this.state = { tasks: [], text: '', priority: 2, update: false};
    this.handleChangePriority = this.handleChangePriority.bind(this);
    this.handleChangeText = this.handleChangeText.bind(this);
    this.handleAdd = this.handleAdd.bind(this);
    this.handleDelete = this.handleDelete.bind(this);
  }

  render() {
    return (
      <div>
        <h3>To Do List</h3>
        <form onSubmit={this.handleAdd}>
          <label htmlFor="new-todo">
          Task:
          </label>
          <input
            id="new-todo"
            onChange={this.handleChangeText}
            value={this.state.text}
          />
          <br/>
          <label htmlFor="new-priority">
          Priority:
          </label>
          <select 
            id="new-priority"
            onChange={this.handleChangePriority}
            value={this.state.priority}
          >
            <option value="1">High (Red)</option>
            <option value="2">Medium (Green)</option>
            <option value="3">Low (Blue)</option>
          </select>
          <br/>
          <button>
            Add New Task
          </button>
        </form>
        <h4>Tasks</h4>
        <TodoList items={this.state.tasks} />
        <form onSubmit={this.handleDelete}>
          <button>
            Delete All Tasks
          </button>
        </form>
      </div>
    );
  }

  componentDidMount() {
    this.refreshData();
  }

  handleChangePriority(e) {
    this.setState({ priority: parseInt(e.target.value) });
  }

  handleChangeText(e) {
    this.setState({ text: e.target.value });
  }

  handleAdd(e) {
    e.preventDefault();
    if (!this.state.text.length) {
      return;
    }
    const newTask = {
      text: this.state.text,
      priority: this.state.priority
    };
    fetch('http://localhost:8000/tasks',
      {
        method: 'POST',  
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(newTask)
      }).then(res => this.refreshData(res));
  }

  handleDelete(e) {
    e.preventDefault();
    fetch('http://localhost:8000/tasks',
      {
        method: 'DELETE'
      }).then(res => this.refreshData(res));
  }

  refreshData(res) {
    fetch('http://localhost:8000/tasks')
      .then(response => response.json())
      .then(data => this.setState({ tasks: data.tasks }));
  }
}

class TodoList extends React.Component {
  render() {
    if (this.props.items == null) {
      return (<span>Empty</span>)
    }
    return (
      <ul>
        {this.props.items.map(item => (
          <li key={item.id} className={'priority_' + item.priority}>
            <img src={'priority_' + item.priority + '.jpg'} alt={'Priority ' + item.priority}/>
            <span>{item.text}</span>
          </li>
        ))}
      </ul>
    );
  }
}

export default ToDoListApp;
