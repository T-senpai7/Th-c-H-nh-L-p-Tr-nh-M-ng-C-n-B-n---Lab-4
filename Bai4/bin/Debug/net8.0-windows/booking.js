// Booking logic based on Bai4Client.cs
// Seat configuration: ROWS = 3, COLS = 5
const ROWS = 3;
const COLS = 5;

// Seat types and price multipliers (from Bai4Client.cs)
const seatTypes = {
    // Vé vớt (0.25x)
    'A1': 'Vé vớt', 'A5': 'Vé vớt',
    'C1': 'Vé vớt', 'C5': 'Vé vớt',
    // Vé thường (1x)
    'A2': 'Vé thường', 'A3': 'Vé thường', 'A4': 'Vé thường',
    'C2': 'Vé thường', 'C3': 'Vé thường', 'C4': 'Vé thường',
    // Vé VIP (2x)
    'B1': 'Vé VIP', 'B2': 'Vé VIP', 'B3': 'Vé VIP',
    'B4': 'Vé VIP', 'B5': 'Vé VIP'
};

const priceMultipliers = {
    'Vé vớt': 0.25,
    'Vé thường': 1.0,
    'Vé VIP': 2.0
};

// State
let isConnected = false;
let ws = null;
let movies = {};
let selectedMovie = '';
let selectedRoom = '';
let selectedSeats = [];
let seatBookedStatus = {};
let customerName = '';
let currentBasePrice = 0;
let isBookingInProgress = false; // Prevent double-booking

// TCP Connection state
let tcpServerIP = '127.0.0.1';
let tcpServerPort = 8080;
let useTcpMode = false; // false = HTTP mode, true = TCP mode

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    console.log('=== DOMContentLoaded: Initializing booking page ===');
    
    initializeSeats();
    setupEventListeners();
    
    // Also add direct event listener to booking button as backup
    const btnBook = document.getElementById('btn-book');
    if (btnBook) {
        console.log('Booking button found:', {
            id: btnBook.id,
            disabled: btnBook.disabled,
            hasOnclick: btnBook.hasAttribute('onclick')
        });
        
        // Remove onclick attribute and use event listener instead
        const onclickAttr = btnBook.getAttribute('onclick');
        if (onclickAttr) {
            console.log('Removing onclick attribute:', onclickAttr);
            btnBook.removeAttribute('onclick');
        }
        
        btnBook.addEventListener('click', (e) => {
            console.log('=== Button clicked via event listener ===');
            console.log('Button state:', {
                disabled: btnBook.disabled,
                customerName,
                selectedMovie,
                selectedRoom,
                selectedSeats: selectedSeats.length,
                isConnected
            });
            
            if (btnBook.disabled) {
                console.warn('⚠️ Button is disabled, ignoring click');
                alert('Vui lòng điền đầy đủ thông tin và kết nối server!');
                return;
            }
            
            e.preventDefault();
            e.stopPropagation();
            bookTickets();
        });
        console.log('✅ Booking button event listener added');
    } else {
        console.error('❌ Booking button not found!');
    }
    
    // Make bookTickets available globally for debugging
    window.bookTickets = bookTickets;
    console.log('✅ bookTickets function available globally');
});

// Initialize seat grid (3 rows x 5 cols: A1-A5, B1-B5, C1-C5)
function initializeSeats() {
    const grid = document.getElementById('seat-grid');
    grid.innerHTML = '';

    for (let row = 0; row < ROWS; row++) {
        for (let col = 0; col < COLS; col++) {
            const seatName = getSeatName(row, col);
            const seatType = seatTypes[seatName] || 'Vé thường';
            const seatClass = getSeatClass(seatType);

            const seat = document.createElement('div');
            seat.className = `seat ${seatClass}`;
            seat.textContent = seatName;
            seat.dataset.seat = seatName;
            seat.dataset.type = seatType;
            seat.onclick = () => toggleSeat(seatName, seat);

            seatBookedStatus[seatName] = false;
            grid.appendChild(seat);
        }
    }
}

function getSeatName(row, col) {
    const rowChar = String.fromCharCode('A'.charCodeAt(0) + row);
    return `${rowChar}${col + 1}`;
}

function getSeatClass(seatType) {
    switch(seatType) {
        case 'Vé vớt': return 'vat';
        case 'Vé VIP': return 'vip';
        default: return 'normal';
    }
}

// Auto-connect debounce timer
let autoConnectTimer = null;

// Setup event listeners
function setupEventListeners() {
    document.getElementById('customer-name').addEventListener('input', (e) => {
        customerName = e.target.value.trim();
        updateBookingButton();
    });

    // Auto-connect when IP or Port changes
    const serverIpInput = document.getElementById('server-ip');
    const serverPortInput = document.getElementById('server-port');
    
    // Function to trigger auto-connect
    const triggerAutoConnect = () => {
        // Clear existing timer
        if (autoConnectTimer) {
            clearTimeout(autoConnectTimer);
        }
        
        // Only auto-connect if not already connected
        if (isConnected) {
            return;
        }
        
        // Get current values
        const ip = serverIpInput.value.trim();
        const port = serverPortInput.value.trim();
        
        // Validate inputs
        if (!ip || !port) {
            return;
        }
        
        // Check if port is a valid number
        const portNum = parseInt(port);
        if (isNaN(portNum) || portNum <= 0 || portNum > 65535) {
            return;
        }
        
        // Validate IP format (basic check)
        const ipPattern = /^(\d{1,3}\.){3}\d{1,3}$|^localhost$/;
        if (!ipPattern.test(ip) && ip !== 'localhost') {
            // Allow hostnames too (for flexibility)
            if (ip.length < 3) {
                return;
            }
        }
        
        // Debounce: wait 1 second after user stops typing
        autoConnectTimer = setTimeout(async () => {
            console.log('Auto-connecting to server...', { ip, port: portNum });
            try {
                await connectToServer(true); // Silent mode - no alerts
            } catch (error) {
                console.log('Auto-connect failed (silent):', error.message);
                // Silent failure - user can manually connect if needed
            }
        }, 1000);
    };
    
    // Listen for input changes on IP and Port fields
    serverIpInput.addEventListener('input', triggerAutoConnect);
    serverIpInput.addEventListener('blur', () => {
        // Also try to connect when user leaves the field
        if (autoConnectTimer) {
            clearTimeout(autoConnectTimer);
        }
        if (!isConnected) {
            const ip = serverIpInput.value.trim();
            const port = serverPortInput.value.trim();
            if (ip && port && parseInt(port) > 0) {
                connectToServer(true).catch(err => { // Silent mode
                    console.log('Auto-connect on blur failed:', err.message);
                });
            }
        }
    });
    
    serverPortInput.addEventListener('input', triggerAutoConnect);
    serverPortInput.addEventListener('blur', () => {
        // Also try to connect when user leaves the field
        if (autoConnectTimer) {
            clearTimeout(autoConnectTimer);
        }
        if (!isConnected) {
            const ip = serverIpInput.value.trim();
            const port = serverPortInput.value.trim();
            if (ip && port && parseInt(port) > 0) {
                connectToServer(true).catch(err => { // Silent mode
                    console.log('Auto-connect on blur failed:', err.message);
                });
            }
        }
    });
    
    // Also connect on Enter key (not silent - user explicitly pressed Enter)
    serverIpInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !isConnected) {
            e.preventDefault();
            if (autoConnectTimer) {
                clearTimeout(autoConnectTimer);
            }
            connectToServer(false).catch(err => { // Show alerts on Enter
                console.log('Connect on Enter failed:', err.message);
            });
        }
    });
    
    serverPortInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !isConnected) {
            e.preventDefault();
            if (autoConnectTimer) {
                clearTimeout(autoConnectTimer);
            }
            connectToServer(false).catch(err => { // Show alerts on Enter
                console.log('Connect on Enter failed:', err.message);
            });
        }
    });

    document.getElementById('movie-select').addEventListener('change', (e) => {
        selectedMovie = e.target.value;
        
        // Clear room selection
        const roomSelect = document.getElementById('room-select');
        roomSelect.innerHTML = '<option value="">-- Chọn phòng --</option>';
        roomSelect.disabled = true;
        roomSelect.classList.add('opacity-50', 'cursor-not-allowed');
        roomSelect.classList.remove('cursor-pointer');
        selectedRoom = '';
        
        // Clear seat selection
        selectedSeats = [];
        updateSeatDisplay();
        
        if (selectedMovie && isConnected) {
            requestRooms(selectedMovie);
        } else if (!isConnected) {
            console.warn('Not connected to server. Please connect first.');
        }
        updateSummary();
        updateBookingButton();
    });

    document.getElementById('room-select').addEventListener('change', (e) => {
        selectedRoom = e.target.value;
        
        // Clear seat selection when changing room (since each room has its own seats)
        selectedSeats = [];
        updateSeatDisplay();
        
        if (selectedRoom && selectedMovie && isConnected) {
            requestSeats(selectedMovie, selectedRoom);
        }
        updateSummary();
        updateBookingButton();
    });
}

// Connect to server - supports both HTTP and TCP mode
async function connectToServer(silent = false) {
    try {
        // Lấy IP và Port từ form
        tcpServerIP = document.getElementById('server-ip').value.trim() || '127.0.0.1';
        const portValue = document.getElementById('server-port').value.trim() || '8080';
        tcpServerPort = parseInt(portValue) || 8080;
        
        // Kiểm tra nếu là localhost hoặc IP của Web Server hiện tại -> dùng HTTP mode
        const currentHost = window.location.hostname;
        if (tcpServerIP === '127.0.0.1' || tcpServerIP === 'localhost' || tcpServerIP === currentHost) {
            useTcpMode = false;
            console.log('Using HTTP mode (local connection)');
            
            // Dùng HTTP API trực tiếp
            const response = await fetch('/api/web/movies');
            const result = await response.json();
            
            if (result.success) {
                isConnected = true;
                updateConnectionStatus(true);
                const btn = document.getElementById('btn-connect');
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-check mr-2"></i>Đã kết nối (HTTP)';
                await requestMovies();
                if (!silent) {
                    console.log('✅ Kết nối thành công (HTTP mode)');
                }
            } else {
                throw new Error(result.error || 'Không thể kết nối đến database');
            }
        } else {
            // Dùng TCP mode qua proxy
            useTcpMode = true;
            console.log(`Using TCP mode (remote server: ${tcpServerIP}:${tcpServerPort})`);
            
            const connectUrl = `/api/tcp-connect?ip=${encodeURIComponent(tcpServerIP)}&port=${tcpServerPort}`;
            const response = await fetch(connectUrl);
            const result = await response.json();
            
            if (result.success) {
                isConnected = true;
                updateConnectionStatus(true);
                const btn = document.getElementById('btn-connect');
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-check mr-2"></i>Đã kết nối (TCP)';
                await requestMoviesTcp();
                if (!silent) {
                    console.log('✅ Kết nối thành công (TCP mode)');
                }
            } else {
                throw new Error(result.error || 'Không thể kết nối đến TCP server');
            }
        }
    } catch (error) {
        isConnected = false;
        updateConnectionStatus(false);
        
        // Only show alert if not in silent mode
        if (!silent) {
            alert(`Lỗi kết nối: ${error.message}\n\nVui lòng đảm bảo:\n1. Web Server đang chạy (cho HTTP mode)\n2. TCP Server đang chạy và đã click "Listen" (cho TCP mode)`);
        } else {
            console.log('Auto-connect failed (silent):', error.message);
        }
        throw error; // Re-throw so caller can handle it
    }
}


function updateConnectionStatus(connected) {
    const status = document.getElementById('connection-status');
    if (connected) {
        status.innerHTML = '<i class="fas fa-circle text-green-500 mr-2"></i>Đã kết nối';
        status.className = 'inline-block px-4 py-2 bg-green-700 rounded';
    } else {
        status.innerHTML = '<i class="fas fa-circle text-red-500 mr-2"></i>Chưa kết nối';
        status.className = 'inline-block px-4 py-2 bg-gray-700 rounded';
    }
}

// Request movies from web database
async function requestMovies() {
    try {
        const response = await fetch('/api/web/movies');
        const result = await response.json();
        
        if (result.success && result.response) {
            parseMovies(result.response);
        } else {
            throw new Error(result.error || 'Không thể tải danh sách phim');
        }
    } catch (error) {
        console.error('Error loading movies:', error);
        alert(`Lỗi tải phim: ${error.message}`);
    }
}

// Request movies via TCP proxy
async function requestMoviesTcp() {
    try {
        const message = 'GET_MOVIES|';
        const response = await fetch('/api/tcp-send', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                message: message,
                serverIP: tcpServerIP,
                serverPort: tcpServerPort
            })
        });
        
        const result = await response.json();
        
        if (result.success && result.response) {
            parseMovies(result.response);
        } else {
            throw new Error(result.error || 'Không thể tải danh sách phim qua TCP');
        }
    } catch (error) {
        console.error('Error loading movies via TCP:', error);
        alert(`Lỗi tải phim: ${error.message}`);
    }
}

function parseMovies(data) {
    // Format: MOVIES|Tên phim1:Giá1;Tên phim2:Giá2;...
    if (data.startsWith('MOVIES|')) {
        const moviesStr = data.substring(7);
        const movieList = moviesStr.split(';');
        
        const select = document.getElementById('movie-select');
        select.innerHTML = '<option value="">-- Chọn phim --</option>';
        movies = {};

        movieList.forEach(movie => {
            if (!movie) return;
            const parts = movie.split(':');
            if (parts.length === 2) {
                const name = parts[0];
                const price = parseFloat(parts[1]);
                movies[name] = price;
                
                const option = document.createElement('option');
                option.value = name;
                option.textContent = `${name} - ${price.toLocaleString('vi-VN')} VNĐ`;
                select.appendChild(option);
            }
        });

        select.disabled = false;
        select.classList.remove('opacity-50', 'cursor-not-allowed');
        select.classList.add('cursor-pointer');
    }
}

// Request rooms for selected movie
async function requestRooms(movieName) {
    console.log('=== requestRooms called ===');
    console.log('Movie name:', movieName);
    
    if (!movieName) {
        console.error('❌ Movie name is empty');
        return;
    }
    
    const select = document.getElementById('room-select');
    if (!select) {
        console.error('❌ Room select element not found!');
        return;
    }
    
    // Show loading state
    select.innerHTML = '<option value="">-- Đang tải phòng... --</option>';
    select.disabled = true;
    
    try {
        const url = `/api/web/rooms?movie=${encodeURIComponent(movieName)}`;
        console.log('Fetching URL:', url);
        
        const response = await fetch(url);
        console.log('Response status:', response.status);
        console.log('Response ok:', response.ok);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const result = await response.json();
        console.log('Rooms API response:', result);
        console.log('Response success:', result.success);
        console.log('Response data:', result.response);
        
        if (result.success) {
            if (result.response) {
                console.log('Calling parseRooms with response:', result.response);
                parseRooms(result.response);
            } else {
                console.warn('⚠️ Response success but no response data');
                select.innerHTML = '<option value="">-- Không có dữ liệu --</option>';
                select.disabled = true;
                select.classList.add('opacity-50', 'cursor-not-allowed');
                select.classList.remove('cursor-pointer');
            }
        } else {
            console.error('❌ API returned error:', result.error || 'Unknown error');
            select.innerHTML = `<option value="">-- Lỗi: ${result.error || 'Không tải được'} --</option>`;
            select.disabled = true;
            select.classList.add('opacity-50', 'cursor-not-allowed');
            select.classList.remove('cursor-pointer');
        }
    } catch (error) {
        console.error('❌ Exception in requestRooms:', error);
        console.error('Error details:', error.message, error.stack);
        select.innerHTML = '<option value="">-- Lỗi kết nối --</option>';
        select.disabled = true;
        select.classList.add('opacity-50', 'cursor-not-allowed');
        select.classList.remove('cursor-pointer');
    }
}

function parseRooms(data) {
    // Format: ROOMS|Phòng 1;Phòng 2;...
    console.log('=== parseRooms called ===');
    console.log('Input data:', data);
    console.log('Data type:', typeof data);
    console.log('Data length:', data ? data.length : 'null');
    
    const select = document.getElementById('room-select');
    if (!select) {
        console.error('Room select element not found!');
        return;
    }
    
    select.innerHTML = '<option value="">-- Chọn phòng --</option>';
    
    if (!data) {
        console.error('Rooms data is null or undefined');
        select.disabled = true;
        select.classList.add('opacity-50', 'cursor-not-allowed');
        select.classList.remove('cursor-pointer');
        return;
    }
    
    // Ensure data is a string
    const dataStr = String(data);
    console.log('Data as string:', dataStr);
    
    if (dataStr.startsWith('ROOMS|')) {
        const roomsStr = dataStr.substring(6); // Remove "ROOMS|" prefix
        console.log('Rooms string after removing prefix:', roomsStr);
        console.log('Rooms string length:', roomsStr.length);
        
        // Split by semicolon and filter out empty strings
        const roomList = roomsStr.split(';').filter(r => r && r.trim());
        console.log('Room list after split and filter:', roomList);
        console.log('Number of rooms:', roomList.length);

        if (roomList.length === 0) {
            console.warn('⚠️ No rooms found for movie. Rooms string was empty or only contained empty values.');
            select.innerHTML = '<option value="">-- Không có phòng --</option>';
            select.disabled = true;
            select.classList.add('opacity-50', 'cursor-not-allowed');
            select.classList.remove('cursor-pointer');
            return;
        }

        // Add each room to dropdown
        roomList.forEach((room, index) => {
            const roomName = room.trim();
            if (roomName) {
                const option = document.createElement('option');
                option.value = roomName;
                option.textContent = roomName;
                select.appendChild(option);
                console.log(`Added room ${index + 1}: ${roomName}`);
            }
        });

        // Enable the dropdown and remove disabled styling
        select.disabled = false;
        select.classList.remove('opacity-50', 'cursor-not-allowed');
        select.classList.add('cursor-pointer');
        selectedRoom = '';
        console.log(`✅ Successfully loaded ${roomList.length} rooms: ${roomList.join(', ')}`);
    } else {
        console.error('❌ Invalid rooms data format. Expected string starting with "ROOMS|" but got:', dataStr);
        console.error('First 50 characters:', dataStr.substring(0, 50));
        select.innerHTML = '<option value="">-- Lỗi định dạng --</option>';
        select.disabled = true;
        select.classList.add('opacity-50', 'cursor-not-allowed');
        select.classList.remove('cursor-pointer');
    }
}

// Request seats for selected room
async function requestSeats(movieName, roomName) {
    console.log(`Requesting seats for movie: ${movieName}, room: ${roomName}`);
    try {
        const response = await fetch(`/api/web/seats?movie=${encodeURIComponent(movieName)}&room=${encodeURIComponent(roomName)}`);
        const result = await response.json();
        
        if (result.success && result.response) {
            console.log('Seats data received, parsing...');
            parseSeats(result.response);
            console.log('Seats updated successfully');
        } else {
            console.warn('Failed to get seats:', result.error);
        }
    } catch (error) {
        console.error('Error loading seats:', error);
    }
}

function parseSeats(data) {
    // Format: SEATS|A1:Vé thường:0;A2:Vé VIP:1;...
    if (data.startsWith('SEATS|')) {
        const seatsStr = data.substring(6);
        const seatList = seatsStr.split(';');

        // Reset seat status
        Object.keys(seatBookedStatus).forEach(seat => {
            seatBookedStatus[seat] = false;
        });

        seatList.forEach(seat => {
            if (!seat) return;
            const parts = seat.split(':');
            if (parts.length === 3) {
                const seatName = parts[0];
                const isBooked = parts[2] === '1';
                seatBookedStatus[seatName] = isBooked;
            }
        });

        updateSeatDisplay();
    }
}

function updateSeatDisplay() {
    const seats = document.querySelectorAll('.seat');
    seats.forEach(seatEl => {
        const seatName = seatEl.dataset.seat;
        const isBooked = seatBookedStatus[seatName] || false;
        const isSelected = selectedSeats.includes(seatName);

        seatEl.classList.remove('booked', 'selected');
        
        if (isBooked) {
            seatEl.classList.add('booked');
            seatEl.onclick = null;
        } else {
            seatEl.onclick = () => toggleSeat(seatName, seatEl);
        }

        if (isSelected) {
            seatEl.classList.add('selected');
        }
    });
}

// Toggle seat selection
function toggleSeat(seatName, seatElement) {
    if (!isConnected || !selectedRoom) {
        alert('Vui lòng kết nối server và chọn phòng trước!');
        return;
    }

    if (seatBookedStatus[seatName]) {
        alert(`Ghế ${seatName} đã được đặt!`);
        return;
    }

    const index = selectedSeats.indexOf(seatName);
    if (index > -1) {
        selectedSeats.splice(index, 1);
        seatElement.classList.remove('selected');
    } else {
        selectedSeats.push(seatName);
        seatElement.classList.add('selected');
    }

    updateSummary();
    updateBookingButton();
}

// Update summary and total price
function updateSummary() {
    document.getElementById('summary-customer').textContent = customerName || '-';
    document.getElementById('summary-movie').textContent = selectedMovie || '-';
    document.getElementById('summary-room').textContent = selectedRoom || '-';

    if (selectedMovie && movies[selectedMovie]) {
        currentBasePrice = movies[selectedMovie];
    }

    // Calculate total
    let total = 0;
    let priceDetails = [];

    selectedSeats.forEach(seatName => {
        const seatType = seatTypes[seatName] || 'Vé thường';
        const multiplier = priceMultipliers[seatType] || 1.0;
        const seatPrice = currentBasePrice * multiplier;
        total += seatPrice;
        priceDetails.push(`${seatName} (${seatType}): ${seatPrice.toLocaleString('vi-VN')} VNĐ`);
    });

    document.getElementById('summary-seats').textContent = selectedSeats.length > 0 ? selectedSeats.join(', ') : 'Chưa chọn';
    document.getElementById('summary-price-detail').innerHTML = priceDetails.join('<br>') || '-';
    document.getElementById('total-price').textContent = total.toLocaleString('vi-VN') + ' VNĐ';
}

function updateBookingButton() {
    const btn = document.getElementById('btn-book');
    const canBook = isConnected && customerName && selectedMovie && selectedRoom && selectedSeats.length > 0;
    btn.disabled = !canBook;
    
    console.log('updateBookingButton() called:', {
        isConnected,
        customerName: !!customerName,
        selectedMovie: !!selectedMovie,
        selectedRoom: !!selectedRoom,
        selectedSeatsCount: selectedSeats.length,
        canBook,
        buttonDisabled: btn.disabled
    });
}

// Book tickets
async function bookTickets() {
    console.log('=== bookTickets() called ===');
    
    // Prevent double-booking
    if (isBookingInProgress) {
        console.warn('⚠️ Booking already in progress, ignoring duplicate request');
        return;
    }
    
    console.log('State check:', {
        customerName,
        selectedMovie,
        selectedRoom,
        selectedSeats,
        isConnected,
        currentBasePrice
    });

    if (!customerName) {
        console.warn('Validation failed: customerName is empty');
        alert('Vui lòng nhập họ và tên!');
        return;
    }

    if (!selectedMovie) {
        console.warn('Validation failed: selectedMovie is empty');
        alert('Vui lòng chọn phim!');
        return;
    }

    if (!selectedRoom) {
        console.warn('Validation failed: selectedRoom is empty');
        alert('Vui lòng chọn phòng!');
        return;
    }

    if (selectedSeats.length === 0) {
        console.warn('Validation failed: selectedSeats is empty');
        alert('Vui lòng chọn ít nhất 1 ghế!');
        return;
    }

    if (!isConnected) {
        console.warn('Validation failed: not connected to server');
        alert('Vui lòng kết nối server trước!');
        return;
    }
    
    // Set booking in progress flag
    isBookingInProgress = true;
    const btnBook = document.getElementById('btn-book');
    if (btnBook) {
        btnBook.disabled = true;
        btnBook.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Đang xử lý...';
    }

    // Calculate total price (same logic as Bai4Client.cs)
    let total = 0;
    selectedSeats.forEach(seatName => {
        const seatType = seatTypes[seatName] || 'Vé thường';
        const multiplier = priceMultipliers[seatType] || 1.0;
        total += currentBasePrice * multiplier;
    });

    console.log('Calculated total:', total);

    // Prepare booking data
    const bookingData = {
        customerName: customerName,
        movieName: selectedMovie,
        roomName: selectedRoom,
        seatNames: selectedSeats
    };

    console.log('Sending booking request:', bookingData);
    console.log('Request URL: /api/web/booking');

    // Send booking request via web database API
    let controller = null;
    let timeoutId = null;
    
    try {
        console.log('Starting fetch request...');
        console.log('Request payload size:', JSON.stringify(bookingData).length, 'bytes');
        
        // Create abort controller with timeout (increased to 120 seconds for database operations)
        controller = new AbortController();
        timeoutId = setTimeout(() => {
            console.error('⚠️ Request timeout after 120 seconds - aborting');
            controller.abort();
        }, 60000); // 60 second timeout for database operations
        
        const startTime = Date.now();
        console.log('Fetching to /api/web/booking at', new Date().toISOString());
        
        const response = await fetch('/api/web/booking', {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(bookingData),
            signal: controller.signal
        });

        const elapsedTime = Date.now() - startTime;
        console.log(`✅ Fetch completed in ${elapsedTime}ms`);

        if (timeoutId) {
            clearTimeout(timeoutId);
            timeoutId = null;
        }
        
        console.log('✅ Fetch completed');
        console.log('Response status:', response.status);
        console.log('Response statusText:', response.statusText);
        console.log('Response headers:', Object.fromEntries(response.headers.entries()));

        if (!response.ok) {
            const errorText = await response.text();
            console.error('❌ HTTP error response:', errorText);
            throw new Error(`HTTP ${response.status}: ${errorText || response.statusText}`);
        }

        const responseText = await response.text();
        console.log('Response text received:', responseText);
        console.log('Response text length:', responseText.length);

        if (!responseText || responseText.trim() === '') {
            throw new Error('Empty response from server');
        }

        let result;
        try {
            result = JSON.parse(responseText);
            console.log('✅ Parsed response:', result);
        } catch (parseError) {
            console.error('❌ JSON parse error:', parseError);
            console.error('Response text that failed to parse:', responseText);
            console.error('Response text (first 200 chars):', responseText.substring(0, 200));
            throw new Error(`Invalid JSON response: ${parseError.message}`);
        }
        
        if (result.success) {
            console.log('✅ Server returned success');
            
            if (result.response && result.response.startsWith('BOOK_SUCCESS')) {
                console.log('✅✅✅ BOOKING SUCCESSFUL! ✅✅✅');
                
                // Save booking info for success popup before clearing
                const bookedSeats = [...selectedSeats];
                const bookedCustomer = customerName;
                const bookedMovie = selectedMovie;
                const bookedRoom = selectedRoom;
                const bookedTotal = total;
                
                // Clear selected seats first (so they won't show as selected anymore)
                selectedSeats = [];
                updateSeatDisplay();
                
                // Refresh seat status after successful booking to show booked seats
                console.log('Refreshing seat status...');
                await requestSeats(bookedMovie, bookedRoom);
                
                // Show success popup with saved booking info
                console.log('Showing success popup...');
                showBookingSuccess(bookedCustomer, bookedMovie, bookedRoom, bookedSeats, bookedTotal);
                
                console.log('✅ Booking workflow completed successfully');
            } else if (result.response && result.response.startsWith('BOOK_ERROR')) {
                const errorMsg = result.response.substring(11);
                console.error('❌ Booking error from server:', errorMsg);
                alert(`Lỗi đặt vé: ${errorMsg}`);
                
                // Refresh seat status to get latest booking state
                await requestSeats(selectedMovie, selectedRoom);
            } else {
                console.warn('⚠️ Unexpected response format:', result.response);
                // If response is success but format is unexpected, still treat as success
                console.log('Treating as success anyway...');
                await requestSeats(selectedMovie, selectedRoom);
                showBookingSuccess(customerName, selectedMovie, selectedRoom, selectedSeats, total);
            }
        } else {
            console.error('❌ Server returned failure:', result.error);
            throw new Error(result.error || 'Đặt vé thất bại');
        }
    } catch (error) {
        console.error('❌❌❌ Exception in bookTickets():', error);
        console.error('Error name:', error.name);
        console.error('Error message:', error.message);
        console.error('Error stack:', error.stack);
        
        if (error.name === 'AbortError') {
            console.error('Request was aborted. Possible causes: timeout, network issue, or duplicate request.');
            alert('Lỗi: Request bị hủy. Có thể do timeout hoặc lỗi mạng. Vui lòng thử lại!');
        } else if (error.name === 'TypeError' && error.message.includes('fetch')) {
            alert('Lỗi: Không thể kết nối đến server. Vui lòng kiểm tra server đang chạy!');
        } else {
            alert(`Lỗi đặt vé: ${error.message}\n\nVui lòng kiểm tra console để biết thêm chi tiết.`);
        }
    } finally {
        // Reset booking in progress flag
        isBookingInProgress = false;
        if (btnBook) {
            btnBook.disabled = !(isConnected && customerName && selectedMovie && selectedRoom && selectedSeats.length > 0);
            btnBook.innerHTML = '<i class="fas fa-ticket-alt mr-2"></i>Đặt Vé';
        }
        if (timeoutId) {
            clearTimeout(timeoutId);
        }
    }
}

function showBookingSuccess(customerName, movie, room, seats, total) {
    console.log('showBookingSuccess called:', { customerName, movie, room, seats, total });
    
    const modal = document.getElementById('booking-success');
    const details = document.getElementById('success-details');

    if (!modal) {
        console.error('❌ Booking success modal not found!');
        alert(`Đặt vé thành công!\n\nKhách hàng: ${customerName}\nPhim: ${movie}\nPhòng: ${room}\nGhế: ${seats.join(', ')}\nTổng tiền: ${total.toLocaleString('vi-VN')} VNĐ`);
        return;
    }

    let priceDetailsHtml = '';
    seats.forEach(seatName => {
        const seatType = seatTypes[seatName] || 'Vé thường';
        const multiplier = priceMultipliers[seatType] || 1.0;
        const seatPrice = currentBasePrice * multiplier;
        priceDetailsHtml += `
            <div class="flex justify-between text-sm">
                <span class="text-gray-400">${seatName} (${seatType})</span>
                <span class="text-white">${seatPrice.toLocaleString('vi-VN')} VNĐ</span>
            </div>
        `;
    });

    details.innerHTML = `
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Khách hàng</p>
            <p class="text-xl font-bold">${customerName}</p>
        </div>
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Phim</p>
            <p class="text-xl font-bold">${movie}</p>
        </div>
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Phòng</p>
            <p class="text-xl font-bold">${room}</p>
        </div>
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Ghế đã chọn</p>
            <p class="text-xl font-bold text-[#fbbf24]">${seats.join(', ')}</p>
        </div>
        <div class="mb-4">
            <p class="text-gray-400 mb-2">Chi tiết giá</p>
            ${priceDetailsHtml}
        </div>
        <div class="border-t border-dashed border-white/20 pt-4">
            <div class="flex justify-between items-end">
                <span class="text-gray-400 text-lg">Tổng thanh toán</span>
                <span class="text-3xl font-bold text-[#fbbf24]">${total.toLocaleString('vi-VN')} VNĐ</span>
            </div>
        </div>
    `;

    modal.style.display = 'flex';
    console.log('✅ Success modal displayed');
}

function closeSuccess() {
    console.log('closeSuccess called');
    const modal = document.getElementById('booking-success');
    if (modal) {
        modal.style.display = 'none';
    }
    // Clear selection after closing success modal
    clearSelection();
}

function clearSelection() {
    selectedSeats = [];
    document.getElementById('customer-name').value = '';
    customerName = '';
    document.getElementById('movie-select').selectedIndex = 0;
    selectedMovie = '';
    document.getElementById('room-select').selectedIndex = 0;
    selectedRoom = '';
    updateSeatDisplay();
    updateSummary();
    updateBookingButton();
}

