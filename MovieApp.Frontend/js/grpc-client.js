// gRPC-Web Client for Top Movies Widget
const GRPC_URL = 'http://localhost:5005';

// Load top 10 movies via gRPC (simple one-time request)
async function loadTopMoviesViaGrpc() {
    try {
        // For simplicity, we'll use REST API as fallback
        // In production, you would use generated gRPC-Web client code
        await loadTopMoviesViaRest();
    } catch (err) {
        console.error('gRPC Error:', err);
        await loadTopMoviesViaRest();
    }
}

// Load top movies via REST API
async function loadTopMoviesViaRest() {
    try {
        const response = await fetch(`${API_URL}/movies`);
        if (!response.ok) throw new Error('Failed to fetch movies');

        const movies = await response.json();
        const topMovies = movies
            .sort((a, b) => b.rating - a.rating)
            .slice(0, 10);
        
        displayTopMovies(topMovies);
    } catch (err) {
        console.error('REST API Error:', err);
        document.getElementById('topMoviesList').innerHTML = 
            '<div class="error-message">Failed to load top movies</div>';
    }
}

// Display top movies in the widget
function displayTopMovies(movies) {
    const container = document.getElementById('topMoviesList');

    if (!movies || movies.length === 0) {
        container.innerHTML = '<div class="loading">No movies found</div>';
        return;
    }

    container.innerHTML = movies.map((movie, index) => `
        <div class="top-movie-item">
            <div class="top-movie-rank">#${index + 1}</div>
            <div class="top-movie-info">
                <div class="top-movie-title">${escapeHtml(movie.title)}</div>
                <div class="top-movie-meta">
                    <span>${movie.releaseYear || movie.release_year}</span>
                    <span>•</span>
                    <span>${escapeHtml(movie.genre || 'N/A')}</span>
                </div>
            </div>
            <div class="top-movie-rating">
                ⭐ ${movie.rating.toFixed(1)}
            </div>
        </div>
    `).join('');
}