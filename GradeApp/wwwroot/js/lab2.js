const API_URL = 'api';
let token = localStorage.getItem('jwtToken');
let currentUser = localStorage.getItem('userName');
let currentRole = localStorage.getItem('userRole');
let currentTab = 'faculties';
let currentTableData = [];

let dictFaculties = [], dictDepartments = [], dictSubjects = [], dictStudents = [], dictProfessors = [];

if (token) {
    setupDashboard();
}

// АВТОРИЗАЦІЯ
async function login() {
    const name = document.getElementById('login-name').value;
    const pass = document.getElementById('login-password').value;

    try {
        const response = await fetch(`${API_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ FullName: name, Password: pass })
        });

        if (!response.ok) throw new Error("Невірне ім'я або пароль");

        const data = await response.json();
        token = data.token;
        currentUser = name;
        currentRole = name.includes(".") ? "Professor" : "Student";

        localStorage.setItem('jwtToken', token);
        localStorage.setItem('userName', currentUser);
        localStorage.setItem('userRole', currentRole);

        setupDashboard();
    } catch (error) {
        alert(error.message);
    }
}

function logout() {
    localStorage.clear();
    location.reload();
}

function getAuthHeaders() {
    return { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` };
}

// ІНТЕРФЕЙС
async function setupDashboard() {
    document.getElementById('loginBlock').style.display = 'none';
    document.getElementById('logout-btn').style.display = 'inline-block';
    document.getElementById('user-info').innerText = `Ви увійшли як: ${currentUser} (${currentRole})`;

    if (currentRole === "Professor") {
        document.getElementById('mainContent').style.display = 'block';
        await loadAllDependencies();
        showTab('faculties');
    } else {
        document.getElementById('studentDashboard').style.display = 'block';
        showMyGrades();
    }
}

async function loadAllDependencies() {
    const headers = { 'Authorization': `Bearer ${token}` };
    try {
        dictFaculties = await (await fetch(`${API_URL}/Faculties`, { headers })).json();
        dictDepartments = await (await fetch(`${API_URL}/Departments`, { headers })).json();
        dictSubjects = await (await fetch(`${API_URL}/Subjects`, { headers })).json();
        dictStudents = await (await fetch(`${API_URL}/Students`, { headers })).json();
        dictProfessors = await (await fetch(`${API_URL}/Professors`, { headers })).json();

        populateSelect('dep-facultyId', dictFaculties, 'name');
        populateSelect('stu-departmentId', dictDepartments, 'name');
        populateSelect('gra-studentId', dictStudents, 'fullName');
        populateSelect('gra-subjectId', dictSubjects, 'name');
        populateSelect('gra-professorId', dictProfessors, 'fullName');
    } catch (e) {
        console.error("Помилка завантаження списків", e);
    }
}

function populateSelect(elementId, dataArray, nameField) {
    const select = document.getElementById(elementId);
    select.innerHTML = '<option value="" disabled selected>Оберіть...</option>';
    dataArray.forEach(item => {
        select.innerHTML += `<option value="${item.id}">${item[nameField]}</option>`;
    });
}

function showTab(tabName) {
    currentTab = tabName;
    document.querySelectorAll('.crud-form').forEach(f => f.style.display = 'none');
    document.getElementById(`form-${tabName}`).style.display = 'block';

    const titles = { 'faculties': 'Факультети', 'departments': 'Кафедри', 'subjects': 'Предмети', 'students': 'Студенти', 'grades': 'Оцінки' };
    document.getElementById('section-title').innerText = `Таблиця: ${titles[tabName]}`;

    document.getElementById('searchInput').value = '';
    cancelEdit(tabName);
    loadTableData();
}

// CRUD ОПЕРАЦІЇ
async function loadTableData() {
    const response = await fetch(`${API_URL}/${currentTab}`, { headers: getAuthHeaders() });
    if (response.ok) {
        currentTableData = await response.json();
        renderTable(currentTableData);
    }
}

function renderTable(data) {

    const searchBox = document.getElementById('search-container');
    if (searchBox) {
        searchBox.style.display = (currentTab === 'grades') ? 'none' : 'block';
    }

    const thead = document.getElementById('tableHead');
    const tbody = document.getElementById('tableBody');

    thead.innerHTML = '';
    tbody.innerHTML = '';

    let cols = [];
    if (currentTab === 'faculties' || currentTab === 'subjects') cols = ['ID', 'Назва'];
    if (currentTab === 'departments') cols = ['ID', 'Кафедра', 'Факультет'];
    if (currentTab === 'students') cols = ['ID', 'ПІБ', 'Кафедра'];
    if (currentTab === 'grades') cols = ['№ запису', 'Студент', 'Предмет', 'Викладач', 'Оцінка', 'Дата'];
    let trHead = '<tr>';
    cols.forEach(c => trHead += `<th>${c}</th>`);
    trHead += '<th>Дії</th></tr>';
    thead.innerHTML = trHead;

    data.forEach(item => {
        let tr = `<tr><td>${item.id}</td>`;

        if (currentTab === 'faculties' || currentTab === 'subjects') {
            tr += `<td>${item.name}</td>`;
        } else if (currentTab === 'departments') {
            let fac = dictFaculties.find(f => f.id === item.facultyId);
            tr += `<td>${item.name}</td><td>${fac ? fac.name : item.facultyId}</td>`;
        } else if (currentTab === 'students') {
            let dep = dictDepartments.find(d => d.id === item.departmentId);
            tr += `<td>${item.fullName}</td><td>${dep ? dep.name : item.departmentId}</td>`;
        } else if (currentTab === 'grades') {
            let stu = dictStudents.find(s => s.id === item.studentId);
            let sub = dictSubjects.find(s => s.id === item.subjectId);
            let prof = dictProfessors.find(p => p.id === item.professorId);
            let date = item.date ? new Date(item.date).toLocaleDateString('uk-UA') : '';

            tr += `<td>${stu ? stu.fullName : item.studentId}</td>
                   <td>${sub ? sub.name : item.subjectId}</td>
                   <td>${prof ? prof.fullName : item.professorId}</td> 
                   <td>${item.value}</td>
                   <td>${date}</td>`;
        }

        let actionButtons = `
            <button onclick='startEdit(${JSON.stringify(item).replace(/'/g, "&#39;")})'>Редагувати</button>
            <button onclick="deleteEntity(${item.id})">Видалити</button>
        `;

        if (currentTab === 'students') {
            actionButtons += `<button onclick="openTranscript(${item.id})">Витяг оцінок</button>`;
        }

        tr += `<td>${actionButtons}</td></tr>`;

        tbody.innerHTML += tr;
    });
}

function filterTable() {
    const query = document.getElementById('searchInput').value.toLowerCase();
    const filtered = currentTableData.filter(item => {
        const text = (item.name || item.fullName || "").toLowerCase();
        return text.includes(query) || item.id.toString() === query;
    });
    renderTable(filtered);
}

async function saveEntity(tab) {
    let url = `${API_URL}/${tab}`;
    let method = 'POST';
    let bodyData = {};

    if (tab === 'faculties') {
        const id = document.getElementById('fac-id').value;
        bodyData = { name: document.getElementById('fac-name').value };
        if (id) { url += `/${id}`; method = 'PUT'; bodyData.id = parseInt(id); }
    }
    else if (tab === 'departments') {
        const id = document.getElementById('dep-id').value;
        bodyData = { name: document.getElementById('dep-name').value, facultyId: parseInt(document.getElementById('dep-facultyId').value) };
        if (id) { url += `/${id}`; method = 'PUT'; bodyData.id = parseInt(id); }
    }
    else if (tab === 'subjects') {
        const id = document.getElementById('sub-id').value;
        bodyData = { name: document.getElementById('sub-name').value };
        if (id) { url += `/${id}`; method = 'PUT'; bodyData.id = parseInt(id); }
    }
    else if (tab === 'students') {
        const id = document.getElementById('stu-id').value;
        bodyData = { fullName: document.getElementById('stu-name').value, password: document.getElementById('stu-pass').value, departmentId: parseInt(document.getElementById('stu-departmentId').value) };
        if (id) { url += `/${id}`; method = 'PUT'; bodyData.id = parseInt(id); }
    }
    else if (tab === 'grades') {
        const id = document.getElementById('gra-id').value;
        const dateVal = document.getElementById('gra-date').value;
        const valueVal = document.getElementById('gra-value').value;

        if (!dateVal || !valueVal) {
            alert("Будь ласка, обов'язково введіть оцінку та оберіть дату!");
            return;
        }
        bodyData = {
            studentId: parseInt(document.getElementById('gra-studentId').value),
            subjectId: parseInt(document.getElementById('gra-subjectId').value),
            professorId: parseInt(document.getElementById('gra-professorId').value),
            value: parseInt(document.getElementById('gra-value').value),
            date: document.getElementById('gra-date').value
        };
        if (id) { url += `/${id}`; method = 'PUT'; bodyData.id = parseInt(id); }
    }

    try {
        const response = await fetch(url, { method: method, headers: getAuthHeaders(), body: JSON.stringify(bodyData) });
        if (!response.ok) {
            const err = await response.text();
            throw new Error(err || "Помилка збереження. Перевірте правильність заповнення.");
        }
        alert("Збережено!");
        cancelEdit(tab);
        await loadAllDependencies();
        loadTableData();
    } catch (e) { alert(e.message); }
}

async function deleteEntity(id) {
    if (!confirm("Видалити запис?")) return;
    try {
        const response = await fetch(`${API_URL}/${currentTab}/${id}`, { method: 'DELETE', headers: getAuthHeaders() });
        if (!response.ok) {
            const err = await response.text();
            throw new Error(err || "Помилка: запис пов'язаний з іншими даними.");
        }
        loadTableData();
        loadAllDependencies();
    } catch (e) { alert(e.message); }
}

function startEdit(item) {
    cancelEdit(currentTab);
    let prefix = currentTab.substring(0, 3);

    const cancelBtn = document.getElementById(`cancel-${prefix}`);
    if (cancelBtn) cancelBtn.style.display = 'inline-block';

    if (currentTab === 'faculties') {
        document.getElementById('fac-id').value = item.id;
        document.getElementById('fac-name').value = item.name;
    } else if (currentTab === 'departments') {
        document.getElementById('dep-id').value = item.id;
        document.getElementById('dep-name').value = item.name;
        document.getElementById('dep-facultyId').value = item.facultyId;
    } else if (currentTab === 'subjects') {
        document.getElementById('sub-id').value = item.id;
        document.getElementById('sub-name').value = item.name;
    } else if (currentTab === 'students') {
        document.getElementById('stu-id').value = item.id;
        document.getElementById('stu-name').value = item.fullName;
        document.getElementById('stu-pass').value = item.password;
        document.getElementById('stu-departmentId').value = item.departmentId;
    } else if (currentTab === 'grades') {
        document.getElementById('gra-id').value = item.id;
        document.getElementById('gra-studentId').value = item.studentId;
        document.getElementById('gra-subjectId').value = item.subjectId;
        document.getElementById('gra-professorId').value = item.professorId;
        document.getElementById('gra-value').value = item.value;
        document.getElementById('gra-date').value = item.date.split('T')[0];
    }
}

function cancelEdit(tab) {
    const prefix = tab.substring(0, 3);
    document.querySelectorAll(`#form-${tab} input`).forEach(i => i.value = '');
    document.querySelectorAll(`#form-${tab} select`).forEach(s => s.value = '');
    const cancelBtn = document.getElementById(`cancel-${prefix}`);
    if (cancelBtn) cancelBtn.style.display = 'none';
}

// СТУДЕНТ І ДОВІДКИ
async function showMyGrades() {
    const response = await fetch(`${API_URL}/Grades/my`, { headers: getAuthHeaders() });
    if (response.ok) {
        const grades = await response.json();
        let html = `<h3>Таблиця оцінок</h3><table><tr><th>Предмет</th><th>Оцінка</th><th>Дата</th></tr>`;
        grades.forEach(g => {
            html += `<tr><td>${g.subjectName}</td><td>${g.value}</td><td>${new Date(g.date).toLocaleDateString('uk-UA')}</td></tr>`;
        });
        document.getElementById('student-view-area').innerHTML = html + `</table>`;
    }
}

async function showMyDynamics() {
    const response = await fetch(`${API_URL}/Grades/my/dynamics`, { headers: getAuthHeaders() });
    if (response.ok) {
        const data = await response.json();
        let html = `<h3>Динаміка успішності</h3><ul>`;
        data.forEach(d => {
            html += `<li><b>${d.period}:</b> середній бал ${d.averageValue}</li>`;
        });
        document.getElementById('student-view-area').innerHTML = html + `</ul>`;
    }
}

async function generateStudentStudyCert() {
    const response = await fetch(`${API_URL}/Students/me`, { headers: getAuthHeaders() });
    if (response.ok) {
        const me = await response.json();
        const html = `
            <h2>ДОВІДКА ПРО НАВЧАННЯ</h2>
            <p>Студент: <b>${me.fullName}</b></p>
            <p>Факультет: <b>${me.department.faculty.name}</b></p>
            <p>Кафедра: <b>${me.department.name}</b></p>
            <p>Дата видачі: ${new Date().toLocaleDateString('uk-UA')}</p>
        `;
        document.getElementById('modal-body').innerHTML = html;
        document.getElementById('universalModal').style.display = 'block';
    }
}

function generateProfessorWorkCert() {
    const html = `
        <h2>ДОВІДКА З МІСЦЯ РОБОТИ</h2>
        <p>Викладач: <b>${currentUser}</b></p>
        <p>Місце роботи: Київський національний університет імені Тараса Шевченка</p>
        <p>Дата видачі: ${new Date().toLocaleDateString('uk-UA')}</p>
    `;
    document.getElementById('modal-body').innerHTML = html;
    document.getElementById('universalModal').style.display = 'block';
}

async function generateStudentReport(id) {
    const response = await fetch(`${API_URL}/Grades/student/${id}/report`, { headers: getAuthHeaders() });
    if (response.ok) {
        const report = await response.json();
        let html = `
            <h2>Виписка оцінок</h2>
            <p>Студент: <b>${report.studentName}</b></p>
            <p>Загальний середній бал: <b>${report.averageGrade}</b></p>
            <ul>`;
        report.grades.forEach(g => html += `<li>${g.subject}: ${g.value}</li>`);
        html += `</ul>`;
        document.getElementById('modal-body').innerHTML = html;
        document.getElementById('universalModal').style.display = 'block';
    }
}

function closeModal() {
    document.getElementById('universalModal').style.display = 'none';
}

let currentTranscriptData = [];

async function openWorkCertificate() {
    try {
        const res = await fetch(`${API_URL}/Professors/me`, { headers: getAuthHeaders() });
        if (!res.ok) throw new Error("Помилка завантаження даних");
        const data = await res.json();

        let html = `
            <h2 style="text-align:center;">ДОВІДКА З МІСЦЯ РОБОТИ</h2>
            <p>Видана за місцем вимоги.</p>
            <p>Цією довідкою підтверджується, що <b>${data.fullName}</b> дійсно є штатним працівником нашого університету.</p>
            <p><b>Факультет:</b> ${data.facultyName}</p>
            <p><b>Кафедра:</b> ${data.departmentName}</p>
            <p><b>Посада:</b> Викладач</p>
            <p style="margin-top:50px; text-align:right;">Дата видачі: ${new Date().toLocaleDateString('uk-UA')}</p>
            <p style="text-align:right;">Підпис: ________________</p>
        `;
        document.getElementById('certContent').innerHTML = html;
        document.getElementById('certModal').style.display = 'block';
    } catch (e) { alert(e.message); }
}

async function openStudyCertificate() {
    try {
        const res = await fetch(`${API_URL}/Students/me`, { headers: getAuthHeaders() });
        if (!res.ok) throw new Error("Помилка завантаження даних");
        const me = await res.json();

        const html = `
            <h2 style="text-align:center;">ДОВІДКА ПРО НАВЧАННЯ</h2>
            <p>Видана за місцем вимоги.</p>
            <p>Цією довідкою підтверджується, що <b>${me.fullName}</b> дійсно є студентом нашого університету.</p>
            <p><b>Факультет:</b> ${me.facultyName}</p>
            <p><b>Кафедра:</b> ${me.departmentName}</p>
            <p style="margin-top:50px; text-align:right;">Дата видачі: ${new Date().toLocaleDateString('uk-UA')}</p>
            <p style="text-align:right;">Підпис деканату: ________________</p>
        `;
        document.getElementById('certContent').innerHTML = html;
        document.getElementById('certModal').style.display = 'block';
    } catch (e) { alert(e.message); }
}

async function openTranscript(studentId = null) {
    let url = studentId ? `${API_URL}/Grades/student/${studentId}` : `${API_URL}/Grades/my`;
    let studentName = "";

    try {
        if (studentId) {
            let stu = dictStudents.find(s => s.id === studentId);
            studentName = stu ? stu.fullName : "Невідомий студент";
        } else {
            const meRes = await fetch(`${API_URL}/Students/me`, { headers: getAuthHeaders() });
            const me = await meRes.json();
            studentName = me.fullName;
        }

        const res = await fetch(url, { headers: getAuthHeaders() });
        if (!res.ok) throw new Error("Помилка завантаження балів");
        currentTranscriptData = await res.json();

        let sum = 0;
        currentTranscriptData.forEach(g => sum += g.value);
        let avg = currentTranscriptData.length > 0 ? (sum / currentTranscriptData.length).toFixed(2) : 0;

        let html = `
            <h2 style="text-align:center;">АКАДЕМІЧНА ДОВІДКА (ВИТЯГ ОЦІНОК)</h2>
            <p><b>Здобувач:</b> ${studentName}</p>
            <p><b>Загальний середній бал:</b> <span style="font-size:22px; color:green;"><b>${avg}</b></span></p>
            
            <hr style="margin:20px 0;">
            <div style="background: #f1f1f1; padding: 10px; margin-bottom: 20px;" class="no-print">
                <b>Фільтр:</b> 
                Від: <input type="date" id="trans-start"> 
                До: <input type="date" id="trans-end">
                <button onclick="filterTranscript()">Застосувати</button>
            </div>
            
            <div id="transcript-table-area"></div>
        `;
        document.getElementById('certContent').innerHTML = html;
        document.getElementById('certModal').style.display = 'block';

        filterTranscript();
    } catch (e) { alert(e.message); }
}

function filterTranscript() {
    let start = document.getElementById('trans-start').value;
    let end = document.getElementById('trans-end').value;

    let filtered = currentTranscriptData;
    if (start) filtered = filtered.filter(g => new Date(g.date) >= new Date(start));
    if (end) filtered = filtered.filter(g => new Date(g.date) <= new Date(end));

    let tableHtml = `<table border="1" width="100%" style="border-collapse: collapse; text-align: left;">
        <tr style="background:#ddd;"><th>Предмет</th><th>Викладач</th><th>Оцінка</th><th>Дата</th></tr>`;

    filtered.forEach(g => {
        let d = new Date(g.date).toLocaleDateString('uk-UA');
        tableHtml += `<tr>
            <td style="padding:5px;">${g.subjectName}</td>
            <td style="padding:5px;">${g.professorName || '-'}</td>
            <td style="padding:5px;"><b>${g.value}</b></td>
            <td style="padding:5px;">${d}</td>
        </tr>`;
    });
    tableHtml += `</table>`;
    document.getElementById('transcript-table-area').innerHTML = tableHtml;
}