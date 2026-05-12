const authUri = 'api/auth/login';
const studentsUri = 'api/Students';
const departmentsUri = 'api/Departments';

let token = localStorage.getItem('jwtToken');
let departmentsList = [];

if (token) {
    showMainContent();
    loadDepartmentsAndStudents();
}

// АВТОРИЗАЦІЯ
function login() {
    const name = document.getElementById('login-name').value;
    const password = document.getElementById('login-password').value;

    fetch(authUri, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ FullName: name, Password: password })
    })
        .then(response => {
            if (!response.ok) throw new Error('Невірний логін або пароль');
            return response.json();
        })
        .then(data => {
            token = data.token;
            localStorage.setItem('jwtToken', token);
            document.getElementById('login-error').innerText = '';
            showMainContent();
            getStudents();
        })
        .catch(error => {
            document.getElementById('login-error').innerText = error.message;
        });
}

function logout() {
    token = null;
    localStorage.removeItem('jwtToken');
    document.getElementById('loginBlock').style.display = 'block';
    document.getElementById('mainContent').style.display = 'none';
}

function showMainContent() {
    document.getElementById('loginBlock').style.display = 'none';
    document.getElementById('mainContent').style.display = 'block';
}

// Допоміжна функція для додавання токена в заголовки
function getAuthHeaders() {
    return {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

// ЗАВАНТАЖЕННЯ КАФЕДР

function loadDepartmentsAndStudents() {
    fetch(departmentsUri, { headers: getAuthHeaders() })
        .then(response => response.json())
        .then(data => {
            departmentsList = data;

            const select = document.getElementById('add-department');
            select.innerHTML = '<option value="" disabled selected>Оберіть кафедру...</option>';

            data.forEach(dep => {
                let opt = document.createElement('option');
                opt.value = dep.id;
                opt.innerHTML = dep.name;
                select.appendChild(opt);
            });

            getStudents();
        })
        .catch(error => console.error('Помилка завантаження кафедр:', error));
}

// УПРАВЛІННЯ СТУДЕНТАМИ

function getStudents() {
    fetch(studentsUri, { headers: getAuthHeaders() })
        .then(response => {
            if (response.status === 401) {
                logout();
                throw new Error(' Увійдіть знову');
            }
            return response.json();
        })
        .then(data => displayStudents(data))
        .catch(error => console.error('Помилка завантаження:', error));
}

function addStudent() {
    const name = document.getElementById('add-name').value;
    const password = document.getElementById('add-password').value;
    const departmentId = document.getElementById('add-department').value;

    const student = {
        FullName: name.trim(),
        Password: password.trim(),
        DepartmentId: parseInt(departmentId, 10)
    };

    fetch(studentsUri, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(student)
    })
        .then(async response => {
            if (!response.ok) {
                const errText = await response.text();
                throw new Error(errText || 'Невідома помилка на сервері');
            }
            return response.json();
        })
        .then(() => {
            getStudents();
            document.getElementById('add-name').value = '';
            document.getElementById('add-password').value = '';
            document.getElementById('add-department').value = '';
            alert("Студента успішно додано!");
        })
        .catch(error => console.error('Помилка додавання:', error));
}

function deleteStudent(id) {
    if (!confirm("Ви впевнені, що хочете видалити цього студента?")) return;

    fetch(`${studentsUri}/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
    })
        .then(async response => {
            if (!response.ok) {
                const errText = await response.text();
                throw new Error(errText || 'Не вдалося видалити');
            }
            getStudents();
        })
        .catch(error => alert("Помилка видалення: " + error.message));
}

function displayStudents(data) {
    const tBody = document.getElementById('studentsTable');
    tBody.innerHTML = '';

    data.forEach(student => {
        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        td1.appendChild(document.createTextNode(student.id));

        let td2 = tr.insertCell(1);
        td2.appendChild(document.createTextNode(student.fullName));

        let td3 = tr.insertCell(2);
        let dep = departmentsList.find(d => d.id === student.departmentId);
        let depName = dep ? dep.name : `ID: ${student.departmentId}`;
        td3.appendChild(document.createTextNode(depName));

        let td4 = tr.insertCell(3);
        let deleteButton = document.createElement('button');
        deleteButton.innerText = 'Видалити';
        deleteButton.setAttribute('onclick', `deleteStudent(${student.id})`);
        td4.appendChild(deleteButton);
    });
}