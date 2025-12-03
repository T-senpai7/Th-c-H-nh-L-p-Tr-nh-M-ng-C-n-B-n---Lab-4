// State
let movies = [];
let selectedMovie = null;
let ticketPrice = 0;
let selectedSeats = [];
let selectedDate = 'Today';
let selectedTime = '04:00 PM';
let customerName = '';

// Initialize seats
const seatRows = ['A', 'B', 'C'];
const seatCols = 8;

// Load movies on page load
document.addEventListener('DOMContentLoaded', () => {
    loadMovies();
    initializeSeats();
    setupEventListeners();
});

// Load movies from API
async function loadMovies() {
    try {
        const response = await fetch('/api/movies');
        movies = await response.json();
        renderMovies();
    } catch (error) {
        console.error('Error loading movies:', error);
        document.getElementById('movies-container').innerHTML = 
            '<div class="text-center text-red-400 py-20">Error loading movies. Please try again.</div>';
    }
}

// Helper function to decode URL-encoded poster URLs
function decodePosterUrl(url) {
    if (!url || url.trim() === '') return null;
    try {
        // Decode URL-encoded characters (e.g., %2f -> /)
        return decodeURIComponent(url);
    } catch (e) {
        console.warn('Error decoding poster URL:', url, e);
        return url; // Return original if decoding fails
    }
}

// Helper function to handle image errors gracefully (must be global for inline handlers)
window.handleImageError = function(img) {
    // Prevent infinite loop by checking if we've already tried to fix it
    if (img.dataset.errorHandled === 'true') {
        // Hide the broken image and show a placeholder div
        img.style.display = 'none';
        const placeholder = document.createElement('div');
        placeholder.className = 'w-full h-full bg-gray-800 flex items-center justify-center';
        placeholder.innerHTML = '<i class="fas fa-image text-gray-600 text-4xl"></i>';
        img.parentElement.appendChild(placeholder);
        return;
    }
    img.dataset.errorHandled = 'true';
    // Try to use a data URI as fallback instead of external service
    img.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAwIiBoZWlnaHQ9IjYwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjMWMxYzI0Ii8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCIgZm9udC1zaXplPSIyNCIgZmlsbD0iIzljYTNhZiIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPk5vIEltYWdlPC90ZXh0Pjwvc3ZnPg==';
};

// Render movies in the grid
function renderMovies() {
    const container = document.getElementById('movies-container');
    
    if (movies.length === 0) {
        container.innerHTML = '<div class="text-center text-gray-400 py-20">No movies available. Please crawl movies first.</div>';
        return;
    }

    container.innerHTML = movies.map((movie, index) => {
        // Decode the poster URL if it exists
        const posterUrl = decodePosterUrl(movie.poster);
        const hasPoster = posterUrl && posterUrl.trim() !== '';
        
        return `
        <label class="movie-card-label">
            <input type="radio" name="movie" value="${movie.title}" data-price="${movie.basePrice || 85000}" data-index="${index}" ${index === 0 ? 'checked' : ''}>
            <div class="movie-card">
                <div class="poster-wrapper">
                    ${hasPoster ? 
                        `<img src="${posterUrl}" 
                             alt="${movie.title}" 
                             onerror="handleImageError(this)"
                             loading="lazy">` : 
                        `<div class="w-full h-full bg-gray-800 flex items-center justify-center">
                            <i class="fas fa-image text-gray-600 text-4xl"></i>
                        </div>`}
                </div>
                <div class="movie-content">
                    <div>
                        <h3 class="movie-title">${movie.title || 'Unknown'}</h3>
                        <div class="movie-meta">
                            ${movie.duration ? `<span><i class="fas fa-clock text-yellow-500 mr-1"></i> ${movie.duration}</span>` : ''}
                            ${movie.genre ? `<span><i class="fas fa-film text-yellow-500 mr-1"></i> ${movie.genre}</span>` : ''}
                        </div>
                        ${movie.description ? `<p class="text-xs text-gray-500 mt-2">${movie.description.substring(0, 100)}...</p>` : ''}
                    </div>
                    <button class="book-btn-mock" type="button">Select Movie</button>
                </div>
            </div>
        </label>
    `;
    }).join('');

    // Re-attach event listeners for movie selection
    document.querySelectorAll('input[name="movie"]').forEach(input => {
        input.addEventListener('change', (e) => {
            const index = parseInt(e.target.dataset.index);
            selectedMovie = movies[index];
            ticketPrice = parseFloat(e.target.dataset.price) || 85000;
            document.getElementById('summary-movie').textContent = selectedMovie.title;
            updateTotal();
            
            document.getElementById('datetime-section').scrollIntoView({ 
                behavior: 'smooth',
                block: 'center'
            });
        });
    });

    // Set first movie as selected if available
    if (movies.length > 0) {
        selectedMovie = movies[0];
        ticketPrice = movies[0].basePrice || 85000;
        document.getElementById('summary-movie').textContent = selectedMovie.title;
        updateTotal();
    }
}

// Initialize seat grid
function initializeSeats() {
    const container = document.getElementById('seats-container');
    container.innerHTML = '';
    
    // Pre-book some random seats for demo
    const bookedSeats = ['A4', 'A5'];
    
    seatRows.forEach(row => {
        for (let col = 1; col <= seatCols; col++) {
            const seatName = `${row}${col}`;
            const isBooked = bookedSeats.includes(seatName);
            const seat = document.createElement('div');
            seat.className = `seat ${isBooked ? 'booked' : 'available'}`;
            seat.dataset.seat = seatName;
            if (!isBooked) {
                seat.addEventListener('click', () => toggleSeat(seatName, seat));
            }
            container.appendChild(seat);
        }
    });
}

// Toggle seat selection
function toggleSeat(seatName, seatElement) {
    const index = selectedSeats.indexOf(seatName);
    
    if (index > -1) {
        selectedSeats.splice(index, 1);
        seatElement.classList.remove('selected');
    } else {
        selectedSeats.push(seatName);
        seatElement.classList.add('selected');
    }
    
    updateTotal();
}

// Setup event listeners
function setupEventListeners() {
    // Customer name
    document.getElementById('customer-name').addEventListener('input', (e) => {
        customerName = e.target.value;
        document.getElementById('summary-customer').textContent = customerName || '-';
    });

    // Date/Time chips
    setupChips(document.querySelectorAll('#date-container .chip'), 'summary-date');
    setupChips(document.querySelectorAll('#time-container .chip'), 'summary-time');

    // Refresh movies button
    document.getElementById('refresh-movies').addEventListener('click', () => {
        loadMovies();
    });
}

function setupChips(chips, summaryId) {
    chips.forEach(chip => {
        chip.addEventListener('click', () => {
            chips.forEach(c => c.classList.remove('selected'));
            chip.classList.add('selected');
            document.getElementById(summaryId).textContent = chip.textContent;
            if (summaryId === 'summary-date') selectedDate = chip.textContent;
            if (summaryId === 'summary-time') selectedTime = chip.textContent;
        });
    });
}

// Update total price
function updateTotal() {
    selectedSeats.sort();
    const seatsText = selectedSeats.length > 0 ? selectedSeats.join(', ') : 'None';
    document.getElementById('summary-seats').textContent = seatsText;
    
    const total = selectedSeats.length * ticketPrice;
    document.getElementById('total-price').textContent = total.toLocaleString('vi-VN');
    
    // Enable/disable checkout button
    const checkoutBtn = document.getElementById('checkout-btn');
    checkoutBtn.disabled = !customerName || !selectedMovie || selectedSeats.length === 0;
}

// Redirect to booking via client-server
function redirectToBooking() {
    if (!customerName) {
        alert('Please enter your name');
        return;
    }
    
    if (!selectedMovie) {
        alert('Please select a movie');
        return;
    }
    
    if (selectedSeats.length === 0) {
        alert('Please select at least one seat');
        return;
    }

    // Create booking data
    const bookingData = {
        customerName: customerName,
        movie: selectedMovie.title,
        date: selectedDate,
        time: selectedTime,
        seats: selectedSeats,
        totalPrice: selectedSeats.length * ticketPrice
    };

    // Show confirmation
    showBookingConfirmation(bookingData);

    // Show message about using client application
    alert(`Booking Details Saved!\n\nTo complete the booking, please:\n1. Open the Bai4 Client application\n2. Enter customer name: ${customerName}\n3. Select movie and complete booking\n\nBooking details:\nMovie: ${selectedMovie.title}\nSeats: ${selectedSeats.join(', ')}\nTotal: ${bookingData.totalPrice.toLocaleString('vi-VN')} VNĐ`);
}

// Show booking confirmation
function showBookingConfirmation(data) {
    const modal = document.getElementById('booking-confirmation');
    const details = document.getElementById('confirmation-details');
    
    details.innerHTML = `
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Customer Name</p>
            <p class="text-xl font-bold">${data.customerName}</p>
        </div>
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Movie</p>
            <p class="text-xl font-bold">${data.movie}</p>
        </div>
        <div class="grid grid-cols-2 gap-4 mb-4">
            <div>
                <p class="text-gray-400 mb-2">Date</p>
                <p class="font-semibold">${data.date}</p>
            </div>
            <div>
                <p class="text-gray-400 mb-2">Time</p>
                <p class="font-semibold">${data.time}</p>
            </div>
        </div>
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Selected Seats</p>
            <p class="text-xl font-bold text-[#fbbf24]">${data.seats.join(', ')}</p>
        </div>
        <div>
            <p class="text-gray-400 mb-2">Total Payment</p>
            <p class="text-3xl font-bold text-[#fbbf24]">${data.totalPrice.toLocaleString('vi-VN')} VNĐ</p>
        </div>
        <div class="mt-6 p-4 bg-yellow-900/20 rounded-lg border border-yellow-500/30">
            <p class="text-sm text-yellow-400">
                <i class="fas fa-info-circle mr-2"></i>
                Please complete the booking using the Client-Server application to finalize your reservation.
            </p>
        </div>
    `;
    
    modal.style.display = 'flex';
}

function closeConfirmation() {
    document.getElementById('booking-confirmation').style.display = 'none';
}

