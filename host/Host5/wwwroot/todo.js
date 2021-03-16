const todoUrl = "/api/todos";
const todos = document.getElementById("todos");

document.getElementById("createNewButton").addEventListener("click", addNew);
const name = document.getElementById("name");
const date = document.getElementById("date");

async function addNew() {
    let request = new Request(todoUrl, {
        method: "POST",
        headers: {
            "content-type": "application/json",
            'x-csrf':'1'
        },
        body: JSON.stringify({
            name: name.value,
            date: date.value,
        })
    });

    let result = await fetch(request);
    if (result.ok) {
        todos.innerHTML = "";
        showTodos();
    }
}

async function showTodos() {
    let result = await fetch(new Request(todoUrl, {
        headers: {
            'x-csrf': '1'
        },
    }));

    if (result.ok) {
        let data = await result.json();
        data.forEach(item => addTodo(item));
    }
}

async function deleteTodo(id) {
    let request = new Request(todoUrl + "/" + id, {
        headers: {
            'x-csrf': '1'
        },
        method: "DELETE"
    });

    let result = await fetch(request);
    if (result.ok) {
        todos.innerHTML = "";
        showTodos();
    }
}

function addTodo(item) {
    let row = document.createElement("tr");
    todos.appendChild(row);

    function addCell(row, text) {
        let cell = document.createElement("td");
        cell.innerText = text;
        row.appendChild(cell);
    }

    function addDeleteButton(row, id) {
        let cell = document.createElement("td");
        row.appendChild(cell);
        let btn = document.createElement("button");
        cell.appendChild(btn);
        btn.textContent = "delete";
        btn.addEventListener("click", async () => await deleteTodo(id));
    }

    addDeleteButton(row, item.id);
    addCell(row, item.id);
    addCell(row, item.date);
    addCell(row, item.name);
    addCell(row, item.user);
}


showTodos();


