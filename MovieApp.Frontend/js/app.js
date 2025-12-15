// ==================== Configuration ====================
const API_URL = 'http://localhost:5005/api'; // Backend API URL
const HUB_URL = 'http://localhost:5005/movieHub'; // SignalR hub URL

// ==================== State ====================
let token = localStorage.getItem('token');
let userRole = localStorage.getItem('userRole');
let username = localStorage.getItem('username');
let allMovies = [];
let connection = null;
let activityItems = [];

// ==================== Initialize ====================
if (token) {
    showMainApp();
} else {
    showAuthPage();
}

// ==================== SignalR Connection ====================
async function startSignalRConnection() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL, {
            skipNegotiation: false,
            transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Listen for movie activity
    connection.on("ReceiveMovieActivity", (data) => {
        console.log("Received activity:", data);
        addActivityItem(data);
        
        // Also refresh movie list to show new movie
        if (data.action === "added") {
            loadMovies();
        }
    });

    // Handle reconnection
    connection.onreconnecting(() => {
        console.log("SignalR reconnecting...");
    });

    connection.onreconnected(() => {
        console.log("SignalR reconnected!");
    });

    connection.onclose(() => {
        console.log("SignalR connection closed");
    });

    try {
        await connection.start();
        console.log("SignalR Connected!");
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        setTimeout(startSignalRConnection, 5000); // Retry after 5 seconds
    }
}

// ==================== Activity Feed ====================
function addActivityItem(data) {
    const timestamp = new Date(data.timestamp || Date.now());
    const timeAgo = getTimeAgo(timestamp);
    
    activityItems.unshift({
        message: data.message,
        time: timeAgo,
        timestamp: timestamp,
        isNew: true
    });

    // Keep only last 20 items
    if (activityItems.length > 20) {
        activityItems = activityItems.slice(0, 20);
    }

    renderActivityFeed();
}

function renderActivityFeed() {
    const activityList = document.getElementById('activityList');
    
    if (activityItems.length === 0) {
        activityList.innerHTML = '<div class="activity-empty">No recent activity</div>';
        return;
    }

    activityList.innerHTML = activityItems.map((item, index) => `
        <div class="activity-item ${item.isNew && index === 0 ? 'new' : ''}">
            <div class="activity-message">${escapeHtml(item.message)}</div>
            <div class="activity-time">${item.time}</div>
        </div>
    `).join('');

    // Remove 'new' class after animation
    setTimeout(() => {
        if (activityItems[0]) {
            activityItems[0].isNew = false;
        }
    }, 3000);
}

function toggleActivityFeed() {
    const feed = document.getElementById('activityFeed');
    feed.classList.toggle('hidden');
}

function getTimeAgo(date) {
    const seconds = Math.floor((new Date() - date) / 1000);
    
    if (seconds < 60) return 'just now';
    if (seconds < 3600) return `${Math.floor(seconds / 60)} min ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)} hours ago`;
    return `${Math.floor(seconds / 86400)} days ago`;
}

// ==================== Auth Tab Switching ====================
function switchTab(tab) {
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    const tabs = document.querySelectorAll('.tab-btn');

    tabs.forEach(t => t.classList.remove('active'));

    if (tab === 'login') {
        loginForm.classList.remove('hidden');
        registerForm.classList.add('hidden');
        tabs[0].classList.add('active');
    } else {
        loginForm.classList.add('hidden');
        registerForm.classList.remove('hidden');
        tabs[1].classList.add('active');
    }

    hideMessage('authError');
    hideMessage('authSuccess');
}

// ==================== Authentication ====================
document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const usernameInput = document.getElementById('loginUsername').value;
    const password = document.getElementById('loginPassword').value;

    try {
        const response = await fetch(`${API_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: usernameInput, password })
        });

        if (!response.ok) {
            showMessage('authError', 'Invalid username or password');
            return;
        }

        const data = await response.json();
        token = data.token;
        localStorage.setItem('token', token);

        // Decode JWT to get role
        const payload = JSON.parse(atob(token.split('.')[1]));
        userRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role;
        username = usernameInput;
        localStorage.setItem('userRole', userRole);
        localStorage.setItem('username', username);

        showMainApp();
    } catch (err) {
        console.error(err);
        showMessage('authError', `Connection error: ${err.message}`);
    }
});

document.getElementById('registerForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const usernameInput = document.getElementById('registerUsername').value;
    const password = document.getElementById('registerPassword').value;
    const role = document.getElementById('registerRole').value;

    try {
        const response = await fetch(`${API_URL}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: usernameInput, password, role })
        });

        if (!response.ok) {
            const error = await response.json();
            showMessage('authError', error.message || 'Username already exists');
            return;
        }

        showMessage('authSuccess', 'Registration successful! Please login.');
        setTimeout(() => switchTab('login'), 2000);
    } catch (err) {
        console.error(err);
        showMessage('authError', `Connection error: ${err.message}`);
    }
});

function logout() {
    // Stop SignalR connection
    if (connection) {
        connection.stop();
    }

    // Call logout endpoint
    fetch(`${API_URL}/auth/logout`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` }
    }).catch(err => console.error('Logout error:', err));

    localStorage.clear();
    token = null;
    userRole = null;
    username = null;
    activityItems = [];
    showAuthPage();
}

// ==================== Page Navigation ====================
function showAuthPage() {
    document.getElementById('authPage').classList.remove('hidden');
    document.getElementById('mainApp').classList.add('hidden');
}

function showMainApp() {
    document.getElementById('authPage').classList.add('hidden');
    document.getElementById('mainApp').classList.remove('hidden');
    document.getElementById('userDisplay').textContent = `${username} (${userRole})`;

    if (userRole === 'Admin') {
        document.getElementById('addMovieBtn').classList.remove('hidden');
    }

    loadMovies();
    startSignalRConnection(); // Start SignalR connection
    loadTopMoviesViaRest(); // Load top 10 movies
}

// ==================== Movie Operations ====================
async function loadMovies() {
    try {
        const response = await fetch(`${API_URL}/movies`);
        if (!response.ok) throw new Error('Failed to fetch movies');

        allMovies = await response.json();
        displayMovies(allMovies);
    } catch (err) {
        document.getElementById('moviesContainer').innerHTML =
            `<div class="error-message">Failed to load movies: ${err.message}</div>`;
    }
}

function displayMovies(movies) {
    const container = document.getElementById('moviesContainer');

    if (!movies.length) {
        container.innerHTML = '<div class="loading">No movies found</div>';
        return;
    }

    container.innerHTML = '<div class="movies-grid">' + movies.map(movie => `
        <div class="movie-card">
            <div class="movie-title">${escapeHtml(movie.title)}</div>
            <div class="movie-meta">
                <span class="movie-genre">${escapeHtml(movie.genre || 'N/A')}</span>
                <span class="movie-rating">⭐ ${movie.rating}/10</span>
                <span>${movie.releaseYear}</span>
            </div>
            <div class="movie-description">${escapeHtml(movie.description || '')}</div>
            ${userRole === 'Admin' ? `
                <div class="movie-actions">
                    <button class="secondary" onclick="editMovie(${movie.id})">Edit</button>
                    <button onclick="deleteMovie(${movie.id})">Delete</button>
                </div>
            ` : ''}
        </div>
    `).join('') + '</div>';
}

// ==================== Add / Update Movie ====================
document.getElementById('movieForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const movieData = {
        title: document.getElementById('movieTitle').value,
        description: document.getElementById('movieDescription').value,
        releaseYear: parseInt(document.getElementById('movieYear').value),
        genre: document.getElementById('movieGenre').value,
        rating: parseFloat(document.getElementById('movieRating').value)
    };

    const movieId = document.getElementById('movieId').value;
    const url = movieId ? `${API_URL}/movies/${movieId}` : `${API_URL}/movies`;
    const method = movieId ? 'PUT' : 'POST';

    try {
        const response = await fetch(url, {
            method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(movieData)
        });

        if (!response.ok) {
            showMessage('modalError', 'Failed to save movie');
            return;
        }

        closeModal();
        loadMovies();
    } catch (err) {
        showMessage('modalError', 'Connection error');
    }
});

function editMovie(id) {
    const movie = allMovies.find(m => m.id === id);
    if (!movie) return;

    document.getElementById('modalTitle').textContent = 'Edit Movie';
    document.getElementById('movieId').value = movie.id;
    document.getElementById('movieTitle').value = movie.title;
    document.getElementById('movieDescription').value = movie.description || '';
    document.getElementById('movieYear').value = movie.releaseYear;
    document.getElementById('movieGenre').value = movie.genre || '';
    document.getElementById('movieRating').value = movie.rating;
    document.getElementById('movieModal').classList.add('active');
}

async function deleteMovie(id) {
    if (!confirm('Are you sure you want to delete this movie?')) return;

    try {
        const response = await fetch(`${API_URL}/movies/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            loadMovies();
        } else {
            alert('Failed to delete movie');
        }
    } catch (err) {
        alert('Connection error');
    }
}

// ==================== Modal ====================
function openAddModal() {
    document.getElementById('modalTitle').textContent = 'Add Movie';
    document.getElementById('movieForm').reset();
    document.getElementById('movieId').value = '';
    document.getElementById('movieModal').classList.add('active');
}

function closeModal() {
    document.getElementById('movieModal').classList.remove('active');
    hideMessage('modalError');
}

// ==================== Search / Filter ====================
document.getElementById('searchBox').addEventListener('input', filterMovies);
document.getElementById('genreFilter').addEventListener('change', filterMovies);

function filterMovies() {
    const search = document.getElementById('searchBox').value.toLowerCase();
    const genre = document.getElementById('genreFilter').value;

    const filtered = allMovies.filter(movie => {
        const matchesSearch = movie.title.toLowerCase().includes(search) || 
                             (movie.description && movie.description.toLowerCase().includes(search));
        const matchesGenre = !genre || movie.genre === genre;
        return matchesSearch && matchesGenre;
    });

    displayMovies(filtered);
}

// ==================== Helper Functions ====================
function showMessage(elementId, message) {
    const el = document.getElementById(elementId);
    el.textContent = message;
    el.classList.remove('hidden');
    setTimeout(() => el.classList.add('hidden'), 5000);
}

function hideMessage(elementId) {
    document.getElementById(elementId).classList.add('hidden');
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}