// GraphQL Client for Search Suggestions

const GRAPHQL_URL = 'http://localhost:5005/graphql';

let searchTimeout = null;
let currentSearchQuery = '';

// Initialize GraphQL search
function initGraphQLSearch() {
    const searchInput = document.getElementById('graphqlSearch');
    const suggestionsBox = document.getElementById('searchSuggestions');

    if (!searchInput) return;

    // Search on input with debounce
    searchInput.addEventListener('input', (e) => {
        const query = e.target.value.trim();
        currentSearchQuery = query;

        // Clear previous timeout
        clearTimeout(searchTimeout);

        if (query.length < 2) {
            hideSuggestions();
            return;
        }

        // Show loading
        showLoadingSuggestions();

        // Debounce: wait 300ms after user stops typing
        searchTimeout = setTimeout(() => {
            searchMoviesGraphQL(query);
        }, 300);
    });

    // Hide suggestions when clicking outside
    document.addEventListener('click', (e) => {
        if (!searchInput.contains(e.target) && !suggestionsBox.contains(e.target)) {
            hideSuggestions();
        }
    });

    // Show suggestions when focusing if there's a query
    searchInput.addEventListener('focus', () => {
        if (currentSearchQuery.length >= 2) {
            searchMoviesGraphQL(currentSearchQuery);
        }
    });
}

// Search movies using GraphQL
async function searchMoviesGraphQL(query) {
    try {
        const graphqlQuery = {
            query: `
                query SearchMovies($query: String!, $limit: Int!) {
                    searchMovies(query: $query, limit: $limit) {
                        id
                        title
                        releaseYear
                        genre
                        rating
                    }
                }
            `,
            variables: {
                query: query,
                limit: 8
            }
        };

        const response = await fetch(GRAPHQL_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(graphqlQuery)
        });

        if (!response.ok) {
            throw new Error('GraphQL request failed');
        }

        const result = await response.json();
        
        if (result.errors) {
            console.error('GraphQL errors:', result.errors);
            showEmptySuggestions('Search error');
            return;
        }

        const movies = result.data.searchMovies;
        displaySuggestions(movies);

    } catch (err) {
        console.error('GraphQL Search Error:', err);
        showEmptySuggestions('Connection error');
    }
}

// Display search suggestions
function displaySuggestions(movies) {
    const suggestionsBox = document.getElementById('searchSuggestions');

    if (!movies || movies.length === 0) {
        showEmptySuggestions('No movies found');
        return;
    }

    suggestionsBox.innerHTML = movies.map(movie => `
        <div class="suggestion-item" onclick="selectMovie(${movie.id})">
            <div class="suggestion-title">${escapeHtml(movie.title)}</div>
            <div class="suggestion-meta">
                <span>${movie.releaseYear}</span>
                <span>•</span>
                <span>${escapeHtml(movie.genre || 'N/A')}</span>
                <span>•</span>
                <span class="suggestion-rating">⭐ ${movie.rating.toFixed(1)}</span>
                <span class="suggestion-badge">GraphQL</span>
            </div>
        </div>
    `).join('');

    suggestionsBox.classList.remove('hidden');
}

// Show loading state
function showLoadingSuggestions() {
    const suggestionsBox = document.getElementById('searchSuggestions');
    suggestionsBox.innerHTML = '<div class="suggestion-loading">Searching...</div>';
    suggestionsBox.classList.remove('hidden');
}

// Show empty state
function showEmptySuggestions(message) {
    const suggestionsBox = document.getElementById('searchSuggestions');
    suggestionsBox.innerHTML = `<div class="suggestion-empty">${message}</div>`;
    suggestionsBox.classList.remove('hidden');
}

// Hide suggestions
function hideSuggestions() {
    const suggestionsBox = document.getElementById('searchSuggestions');
    suggestionsBox.classList.add('hidden');
}

// Select a movie from suggestions
function selectMovie(movieId) {
    hideSuggestions();
    
    // Find the movie in allMovies and show only that movie
    const movie = allMovies.find(m => m.id === movieId);
    if (movie) {
        displayMovies([movie]);
        
        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
        
        // Add button to show all movies again
        const container = document.getElementById('moviesContainer');
        const showAllBtn = document.createElement('button');
        showAllBtn.textContent = '← Show All Movies';
        showAllBtn.className = 'show-all-button';
        showAllBtn.onclick = () => {
            displayMovies(allMovies);
            showAllBtn.remove();
        };
        container.insertBefore(showAllBtn, container.firstChild);
    }

    // Clear search input
    document.getElementById('graphqlSearch').value = '';
    currentSearchQuery = '';
}