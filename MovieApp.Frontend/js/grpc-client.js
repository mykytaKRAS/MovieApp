// Statistics Widget (calling gRPC Calculator Service via Main API)

// Load collection statistics
async function loadCollectionStats() {
    try {
        const response = await fetch(`${API_URL}/stats/collection`);
        if (!response.ok) throw new Error('Failed to fetch stats');

        const stats = await response.json();
        displayCollectionStats(stats);
        console.log('✅ Statistics calculated via gRPC Calculator Service');
    } catch (err) {
        console.error('Error loading stats:', err);
        document.getElementById('topMoviesList').innerHTML = 
            '<div class="error-message">Failed to load statistics</div>';
    }
}

// Display collection statistics
function displayCollectionStats(stats) {
    const container = document.getElementById('topMoviesList');

    if (!stats.totalMovies) {
        container.innerHTML = '<div class="loading">No statistics available</div>';
        return;
    }

    container.innerHTML = `
        <div class="stats-card">
            <div class="stats-row">
                <span class="stats-label">Total Movies:</span>
                <span class="stats-value">${stats.totalMovies}</span>
            </div>
            <div class="stats-row">
                <span class="stats-label">Average Rating:</span>
                <span class="stats-value">⭐ ${stats.averageRating.toFixed(2)}/10</span>
            </div>
            <div class="stats-row">
                <span class="stats-label">Highest Rating:</span>
                <span class="stats-value">${stats.highestRating.toFixed(1)}/10</span>
            </div>
            <div class="stats-row">
                <span class="stats-label">Lowest Rating:</span>
                <span class="stats-value">${stats.lowestRating.toFixed(1)}/10</span>
            </div>
            <div class="stats-divider"></div>
            <div class="stats-distribution">
                <h4>Rating Distribution:</h4>
                <div class="distribution-item">
                    <span>🌟 Excellent (8.0-10.0):</span>
                    <span>${stats.distribution.excellent}</span>
                </div>
                <div class="distribution-item">
                    <span>⭐ Good (6.0-7.9):</span>
                    <span>${stats.distribution.good}</span>
                </div>
                <div class="distribution-item">
                    <span>🙂 Average (4.0-5.9):</span>
                    <span>${stats.distribution.average}</span>
                </div>
                <div class="distribution-item">
                    <span>😐 Poor (0.0-3.9):</span>
                    <span>${stats.distribution.poor}</span>
                </div>
            </div>
            <div class="stats-footer">
                <small>📡 ${stats.message}</small>
            </div>
        </div>
    `;
}