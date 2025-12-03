# Web Server & Movie Booking System

## Overview

This system integrates a beautiful HTML booking interface with the existing client-server booking system. It includes:

1. **HTTP Server**: Serves the HTML booking page on localhost
2. **Movie Scraper**: Crawls movie data from betacinemas.vn and saves to JSON
3. **Enhanced Booking UI**: Modern booking interface with customer info, seat selection, and booking confirmation
4. **Integration**: Connects to the TCP client-server booking system

## Features

### 1. Web Server
- Simple HTTP server running on port 8888
- Serves HTML, CSS, JavaScript files
- Provides API endpoints for movie data
- Supports CORS for cross-origin requests

### 2. Movie Scraper
- Crawls movie data from https://betacinemas.vn/phim.htm
- Extracts movie title, poster, description, duration, genre
- Saves data to `movies.json` file
- Falls back to sample data if scraping fails

### 3. Enhanced Booking Interface
- Customer information input
- Dynamic movie loading from API
- Date and time selection
- Interactive seat selection
- Real-time price calculation
- Booking confirmation modal
- Redirect to client-server for final booking

## How to Use

### Step 1: Start the Web Server

1. Run the application: `dotnet run` in the `Bai4` folder
2. Select **"Web Server"** from the menu
3. Click **"Start HTTP Server"** button
4. Server will start on `http://localhost:8888`

### Step 2: Crawl Movies (Optional)

1. Click **"Crawl Movies from BetaCinemas"** button
2. Wait for the scraping to complete
3. Movies will be saved to `movies.json`
4. If scraping fails, sample movies will be used

### Step 3: Open Booking Page

1. Click **"Open in Browser"** button
2. Or manually navigate to `http://localhost:8888/Viewing.html` or `http://localhost:8888/index.html`
3. The booking interface will load

### Step 4: Make a Booking

1. **Enter Customer Name**: Fill in the customer name field
2. **Select Movie**: Choose a movie from the loaded list
3. **Select Date & Time**: Pick your preferred date and showtime
4. **Select Seats**: Click on available seats to select them
5. **Review Summary**: Check the booking summary on the right
6. **Complete Booking**: Click "Complete Booking via Client" button
7. **Finalize**: Use the TCP Client application to complete the booking

## File Structure

```
Bai4/
├── Viewing.html          # Original booking template
├── index.html            # Enhanced booking interface with API integration
├── app.js                # JavaScript for movie loading and booking logic
├── movies.json           # Scraped movie data (generated)
├── SimpleHttpServer.cs   # HTTP server implementation
├── MovieScraper.cs       # Web scraping functionality
├── WebServerForm.cs      # UI form for managing web server
└── ...
```

## API Endpoints

### GET /api/movies
Returns the list of movies from `movies.json`

**Response:**
```json
[
  {
    "title": "Movie Title",
    "poster": "https://...",
    "description": "Movie description...",
    "duration": "2h 30m",
    "genre": "Drama",
    "basePrice": 85000
  }
]
```

### POST /api/booking
Accepts booking request (currently shows confirmation message)

## Booking Flow

1. **Web Interface** → Customer enters info and selects movie/seats
2. **Booking Confirmation** → Shows customer info, ticket details, and total
3. **Client-Server** → Customer completes final booking using TCP Client application

## Integration with Client-Server

The HTML interface is designed to work alongside the existing TCP client-server system:

1. Customer browses and selects tickets on the web interface
2. Booking details are confirmed and displayed
3. Customer is directed to use the TCP Client application for final booking
4. TCP Client connects to TCP Server to complete the transaction

## Notes

- The HTTP server runs on port 8888 (can be changed in `WebServerForm.cs`)
- Movie data is cached in `movies.json` file
- If scraping fails, sample movie data is used
- The booking interface works with both `Viewing.html` and `index.html`
- Make sure TCP Server is running before completing bookings via Client

## Troubleshooting

1. **Server won't start**: Check if port 8888 is already in use
2. **Movies not loading**: Make sure you've crawled movies or `movies.json` exists
3. **Scraping fails**: The system will use sample data automatically
4. **Booking button disabled**: Make sure customer name, movie, and seats are selected

