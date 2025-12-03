using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Bai4
{
    public class CinemaWebDatabase
    {
        private string connectionString;
        private const string DB_NAME = "cinema_dataweb.db";

        public CinemaWebDatabase()
        {
            connectionString = $"Data Source={DB_NAME}";
            InitializeDatabase();
            LoadMoviesFromJson();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Tạo bảng Movies
                var createMoviesTable = @"
                    CREATE TABLE IF NOT EXISTS Movies (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE,
                        BasePrice REAL NOT NULL
                    )";

                // Tạo bảng Rooms (mỗi phòng thuộc về một phim)
                var createRoomsTable = @"
                    CREATE TABLE IF NOT EXISTS Rooms (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        MovieId INTEGER NOT NULL,
                        RoomNumber INTEGER NOT NULL,
                        Name TEXT NOT NULL,
                        FOREIGN KEY (MovieId) REFERENCES Movies(Id),
                        UNIQUE(MovieId, RoomNumber)
                    )";

                // Bảng MovieRooms không còn cần thiết vì mỗi phòng chỉ thuộc 1 phim
                // Giữ lại để tương thích với dữ liệu cũ (sẽ được migrate)
                var createMovieRoomsTable = @"
                    CREATE TABLE IF NOT EXISTS MovieRooms (
                        MovieId INTEGER NOT NULL,
                        RoomId INTEGER NOT NULL,
                        PRIMARY KEY (MovieId, RoomId),
                        FOREIGN KEY (MovieId) REFERENCES Movies(Id),
                        FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
                    )";

                // Tạo bảng Seats
                var createSeatsTable = @"
                    CREATE TABLE IF NOT EXISTS Seats (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        RoomId INTEGER NOT NULL,
                        SeatName TEXT NOT NULL,
                        SeatType TEXT NOT NULL,
                        IsBooked INTEGER NOT NULL DEFAULT 0,
                        FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
                        UNIQUE(RoomId, SeatName)
                    )";

                // Tạo bảng Bookings
                var createBookingsTable = @"
                    CREATE TABLE IF NOT EXISTS Bookings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerName TEXT NOT NULL,
                        MovieId INTEGER NOT NULL,
                        RoomId INTEGER NOT NULL,
                        BookingTime TEXT NOT NULL,
                        TotalPrice REAL NOT NULL,
                        FOREIGN KEY (MovieId) REFERENCES Movies(Id),
                        FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
                    )";

                // Tạo bảng BookingSeats
                var createBookingSeatsTable = @"
                    CREATE TABLE IF NOT EXISTS BookingSeats (
                        BookingId INTEGER NOT NULL,
                        SeatId INTEGER NOT NULL,
                        PRIMARY KEY (BookingId, SeatId),
                        FOREIGN KEY (BookingId) REFERENCES Bookings(Id),
                        FOREIGN KEY (SeatId) REFERENCES Seats(Id)
                    )";

                var command = connection.CreateCommand();
                command.CommandText = createMoviesTable;
                command.ExecuteNonQuery();

                command.CommandText = createRoomsTable;
                command.ExecuteNonQuery();

                // Migrate existing Rooms table if needed (add MovieId column if it doesn't exist)
                try
                {
                    var checkColumn = connection.CreateCommand();
                    checkColumn.CommandText = "PRAGMA table_info(Rooms)";
                    var columnInfo = new List<string>();
                    using (var reader = checkColumn.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columnInfo.Add(reader.GetString(1)); // Column name
                        }
                    }
                    
                    if (!columnInfo.Contains("MovieId"))
                    {
                        Console.WriteLine("⚠️ Migrating Rooms table: Old schema detected, recreating with MovieId...");
                        Console.WriteLine("   This will clear existing rooms - they will be recreated per movie.");
                        
                        // Drop existing tables that depend on Rooms
                        var dropDependent = connection.CreateCommand();
                        dropDependent.CommandText = @"
                            DROP TABLE IF EXISTS BookingSeats;
                            DROP TABLE IF EXISTS Bookings;
                            DROP TABLE IF EXISTS Seats;
                            DROP TABLE IF EXISTS MovieRooms;
                            DROP TABLE IF EXISTS Rooms;
                        ";
                        dropDependent.ExecuteNonQuery();
                        Console.WriteLine("   Dropped old Rooms and dependent tables");
                        
                        // Recreate Rooms table with new schema
                        command.CommandText = createRoomsTable;
                        command.ExecuteNonQuery();
                        Console.WriteLine("✅ Migration completed: Rooms table recreated with MovieId column");
                        Console.WriteLine("   Note: Rooms and seats will be created when movies are loaded");
                    }
                    else
                    {
                        Console.WriteLine("✅ Rooms table already has MovieId column - no migration needed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Migration error: {ex.Message}");
                    Console.WriteLine("   Attempting to recreate Rooms table...");
                    try
                    {
                        var dropAndRecreate = connection.CreateCommand();
                        dropAndRecreate.CommandText = @"
                            DROP TABLE IF EXISTS BookingSeats;
                            DROP TABLE IF EXISTS Bookings;
                            DROP TABLE IF EXISTS Seats;
                            DROP TABLE IF EXISTS MovieRooms;
                            DROP TABLE IF EXISTS Rooms;
                        ";
                        dropAndRecreate.ExecuteNonQuery();
                        command.CommandText = createRoomsTable;
                        command.ExecuteNonQuery();
                        Console.WriteLine("✅ Rooms table recreated successfully");
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine($"❌ Failed to recreate Rooms table: {ex2.Message}");
                    }
                }

                command.CommandText = createMovieRoomsTable;
                command.ExecuteNonQuery();

                command.CommandText = createSeatsTable;
                command.ExecuteNonQuery();

                command.CommandText = createBookingsTable;
                command.ExecuteNonQuery();

                command.CommandText = createBookingSeatsTable;
                command.ExecuteNonQuery();
            }
        }

        private void LoadMoviesFromJson()
        {
            try
            {
                var scraper = new MovieScraper();
                var movies = scraper.LoadMoviesFromJson();

                if (movies == null || movies.Count == 0)
                {
                    Console.WriteLine("No movies found in movies.json, using default data");
                    InitializeDefaultData();
                    return;
                }

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Clear existing movies and movie-room relationships (but keep rooms and seats)
                    var clearMovies = connection.CreateCommand();
                    clearMovies.CommandText = "DELETE FROM MovieRooms; DELETE FROM Movies;";
                    clearMovies.ExecuteNonQuery();
                    Console.WriteLine("Cleared existing movies and movie-room relationships");

                    // Insert movies from JSON
                    var insertMovie = connection.CreateCommand();
                    insertMovie.CommandText = @"
                        INSERT OR REPLACE INTO Movies (Name, BasePrice) VALUES 
                        (@name, @price)";
                    insertMovie.Parameters.Add("@name", SqliteType.Text);
                    insertMovie.Parameters.Add("@price", SqliteType.Real);

                    var movieIds = new Dictionary<string, long>();
                    var movieIndex = 0;
                    foreach (var movie in movies)
                    {
                        if (string.IsNullOrWhiteSpace(movie.title))
                            continue;

                        insertMovie.Parameters["@name"].Value = movie.title;
                        insertMovie.Parameters["@price"].Value = movie.basePrice > 0 ? movie.basePrice : 85000;
                        
                        try
                        {
                            insertMovie.ExecuteNonQuery();

                            // Get inserted movie ID
                            var getLastId = connection.CreateCommand();
                            getLastId.CommandText = "SELECT Id FROM Movies WHERE Name = @name";
                            getLastId.Parameters.AddWithValue("@name", movie.title);
                            var idObj = getLastId.ExecuteScalar();
                            if (idObj != null)
                            {
                                movieIds[movie.title] = Convert.ToInt64(idObj);
                                Console.WriteLine($"Movie {movieIndex + 1}: {movie.title} - ID: {idObj}");
                            }
                            movieIndex++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inserting movie {movie.title}: {ex.Message}");
                        }
                    }

                    // Ensure rooms exist
                    InitializeRooms(connection);

                    // Convert movieIds dictionary to ordered list (preserve order from JSON)
                    var moviesInOrder = new List<(string Name, long Id)>();
                    foreach (var movie in movies)
                    {
                        if (string.IsNullOrWhiteSpace(movie.title))
                            continue;
                        
                        if (movieIds.ContainsKey(movie.title))
                        {
                            moviesInOrder.Add((movie.title, movieIds[movie.title]));
                        }
                    }

                    // Create rooms and seats for each movie (each movie gets its own rooms)
                    CreateRoomsForMovies(connection, moviesInOrder);

                    Console.WriteLine($"Loaded {moviesInOrder.Count} movies from movies.json into database");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading movies from JSON: {ex.Message}");
                InitializeDefaultData();
            }
        }

        private void InitializeRooms(SqliteConnection connection)
        {
            // This method is deprecated - rooms are now created per movie
            // Keeping for backward compatibility but it won't be called
            Console.WriteLine("InitializeRooms: This method is deprecated. Rooms are now created per movie.");
        }

        private void InitializeSeatsForRooms(SqliteConnection connection, Dictionary<int, long> roomIds)
        {
            var seatTypes = new Dictionary<string, string>
            {
                { "A1", "Vé vớt" }, { "A5", "Vé vớt" }, { "C1", "Vé vớt" }, { "C5", "Vé vớt" },
                { "A2", "Vé thường" }, { "A3", "Vé thường" }, { "A4", "Vé thường" },
                { "C2", "Vé thường" }, { "C3", "Vé thường" }, { "C4", "Vé thường" },
                { "B1", "Vé VIP" }, { "B2", "Vé VIP" }, { "B3", "Vé VIP" },
                { "B4", "Vé VIP" }, { "B5", "Vé VIP" }
            };

            var insertSeat = connection.CreateCommand();
            insertSeat.CommandText = @"
                INSERT OR IGNORE INTO Seats (RoomId, SeatName, SeatType, IsBooked) VALUES 
                (@roomId, @seatName, @seatType, 0)";
            insertSeat.Parameters.Add("@roomId", SqliteType.Integer);
            insertSeat.Parameters.Add("@seatName", SqliteType.Text);
            insertSeat.Parameters.Add("@seatType", SqliteType.Text);

            char[] rows = { 'A', 'B', 'C' };
            int[] cols = { 1, 2, 3, 4, 5 };

            foreach (var roomId in roomIds.Values)
            {
                // Check if seats already exist
                var checkSeats = connection.CreateCommand();
                checkSeats.CommandText = "SELECT COUNT(*) FROM Seats WHERE RoomId = @roomId";
                checkSeats.Parameters.AddWithValue("@roomId", roomId);
                var seatCount = Convert.ToInt64(checkSeats.ExecuteScalar());
                
                if (seatCount > 0) continue; // Seats already exist

                foreach (char row in rows)
                {
                    foreach (int col in cols)
                    {
                        string seatName = $"{row}{col}";
                        string seatType = seatTypes.ContainsKey(seatName) ? seatTypes[seatName] : "Vé thường";
                        
                        insertSeat.Parameters["@roomId"].Value = roomId;
                        insertSeat.Parameters["@seatName"].Value = seatName;
                        insertSeat.Parameters["@seatType"].Value = seatType;
                        insertSeat.ExecuteNonQuery();
                    }
                }
            }
        }

        private void CreateRoomsForMovies(SqliteConnection connection, List<(string Name, long Id)> moviesInOrder)
        {
            // Clear existing rooms and seats (will recreate for each movie)
            var clearSeats = connection.CreateCommand();
            clearSeats.CommandText = "DELETE FROM Seats";
            clearSeats.ExecuteNonQuery();
            
            var clearRooms = connection.CreateCommand();
            clearRooms.CommandText = "DELETE FROM Rooms";
            clearRooms.ExecuteNonQuery();
            
            var clearMovieRooms = connection.CreateCommand();
            clearMovieRooms.CommandText = "DELETE FROM MovieRooms";
            clearMovieRooms.ExecuteNonQuery();
            
            Console.WriteLine("Cleared existing rooms, seats, and movie-room relationships");

            var insertRoom = connection.CreateCommand();
            insertRoom.CommandText = @"
                INSERT INTO Rooms (MovieId, RoomNumber, Name) VALUES 
                (@movieId, @number, @name)";
            insertRoom.Parameters.Add("@movieId", SqliteType.Integer);
            insertRoom.Parameters.Add("@number", SqliteType.Integer);
            insertRoom.Parameters.Add("@name", SqliteType.Text);

            // Create rooms and seats for each movie
            for (int i = 0; i < moviesInOrder.Count; i++)
            {
                var movie = moviesInOrder[i];
                long movieId = movie.Id;
                string movieName = movie.Name;
                
                // Top 3 movies get 3 rooms, others get 2 rooms
                int roomCount = (i < 3) ? 3 : 2;
                
                var roomIds = new List<long>();
                
                // Create rooms for this movie
                for (int roomNum = 1; roomNum <= roomCount; roomNum++)
                {
                    insertRoom.Parameters["@movieId"].Value = movieId;
                    insertRoom.Parameters["@number"].Value = roomNum;
                    insertRoom.Parameters["@name"].Value = $"Phòng {roomNum}";
                    insertRoom.ExecuteNonQuery();
                    
                    // Get the room ID
                    var getRoomId = connection.CreateCommand();
                    getRoomId.CommandText = "SELECT last_insert_rowid()";
                    long roomId = Convert.ToInt64(getRoomId.ExecuteScalar());
                    roomIds.Add(roomId);
                }
                
                Console.WriteLine($"Created {roomCount} rooms for movie '{movieName}' (ID: {movieId})");
                
                // Create seats for each room of this movie
                var roomIdsDict = new Dictionary<int, long>();
                for (int j = 0; j < roomIds.Count; j++)
                {
                    roomIdsDict[j + 1] = roomIds[j];
                }
                InitializeSeatsForRooms(connection, roomIdsDict);
            }
            
            Console.WriteLine($"Successfully created rooms and seats for {moviesInOrder.Count} movies");
        }

        private void InitializeDefaultData()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Check if data exists
                var checkData = connection.CreateCommand();
                checkData.CommandText = "SELECT COUNT(*) FROM Movies";
                var count = Convert.ToInt64(checkData.ExecuteScalar());
                
                if (count > 0) return; // Already has data

                // Add default movies
                var movies = new Dictionary<string, double>
                {
                    { "Đào, phở và piano", 45000 },
                    { "Mai", 100000 },
                    { "Gặp lại chị bầu", 70000 },
                    { "Tarot", 90000 }
                };

                var insertMovie = connection.CreateCommand();
                insertMovie.CommandText = @"
                    INSERT INTO Movies (Name, BasePrice) VALUES 
                    (@name, @price)";
                insertMovie.Parameters.Add("@name", SqliteType.Text);
                insertMovie.Parameters.Add("@price", SqliteType.Real);

                var moviesInOrder = new List<(string Name, long Id)>();
                foreach (var movie in movies)
                {
                    insertMovie.Parameters["@name"].Value = movie.Key;
                    insertMovie.Parameters["@price"].Value = movie.Value;
                    insertMovie.ExecuteNonQuery();
                    
                    var getLastId = connection.CreateCommand();
                    getLastId.CommandText = "SELECT last_insert_rowid()";
                    var movieId = Convert.ToInt64(getLastId.ExecuteScalar());
                    moviesInOrder.Add((movie.Key, movieId));
                }

                // Create rooms and seats for each movie
                CreateRoomsForMovies(connection, moviesInOrder);
            }
        }

        public List<MovieInfo> GetMovies()
        {
            var movies = new List<MovieInfo>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, BasePrice FROM Movies ORDER BY Name";
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        movies.Add(new MovieInfo
                        {
                            Id = reader.GetInt64(0),
                            Name = reader.GetString(1),
                            BasePrice = reader.GetDouble(2)
                        });
                    }
                }
            }
            return movies;
        }

        public List<RoomInfo> GetRoomsForMovie(string movieName)
        {
            Console.WriteLine($"=== GetRoomsForMovie called with: '{movieName}' ===");
            var rooms = new List<RoomInfo>();
            
            // Trim whitespace
            movieName = movieName?.Trim() ?? "";
            Console.WriteLine($"Movie name after trim: '{movieName}' (length: {movieName.Length})");
            
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                
                // First, check if movie exists (exact match)
                var checkMovie = connection.CreateCommand();
                checkMovie.CommandText = "SELECT Id, Name FROM Movies WHERE Name = @movieName";
                checkMovie.Parameters.AddWithValue("@movieName", movieName);
                
                long? movieId = null;
                string? actualMovieName = null;
                
                using (var reader = checkMovie.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        movieId = reader.GetInt64(0);
                        actualMovieName = reader.GetString(1);
                        Console.WriteLine($"✅ Movie found: ID={movieId}, Name='{actualMovieName}'");
                    }
                }
                
                // If not found, try case-insensitive search
                if (movieId == null)
                {
                    Console.WriteLine($"⚠️ Exact match not found, trying case-insensitive search...");
                    var checkMovieCaseInsensitive = connection.CreateCommand();
                    checkMovieCaseInsensitive.CommandText = "SELECT Id, Name FROM Movies WHERE LOWER(TRIM(Name)) = LOWER(TRIM(@movieName))";
                    checkMovieCaseInsensitive.Parameters.AddWithValue("@movieName", movieName);
                    
                    using (var reader = checkMovieCaseInsensitive.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            movieId = reader.GetInt64(0);
                            actualMovieName = reader.GetString(1);
                            Console.WriteLine($"✅ Movie found (case-insensitive): ID={movieId}, Name='{actualMovieName}'");
                            Console.WriteLine($"   Note: Using actual name '{actualMovieName}' instead of '{movieName}'");
                        }
                    }
                }
                
                if (movieId == null)
                {
                    Console.WriteLine($"❌ Movie '{movieName}' not found in database");
                    // List all available movies for debugging
                    var allMovies = connection.CreateCommand();
                    allMovies.CommandText = "SELECT Name FROM Movies ORDER BY Name";
                    using (var reader = allMovies.ExecuteReader())
                    {
                        var movieList = new List<string>();
                        while (reader.Read())
                        {
                            movieList.Add(reader.GetString(0));
                        }
                        Console.WriteLine($"Available movies in database ({movieList.Count} total):");
                        foreach (var m in movieList)
                        {
                            Console.WriteLine($"  - '{m}' (length: {m.Length})");
                        }
                    }
                    return rooms; // Return empty list
                }
                
                // Use actual movie name from database for query
                var actualName = actualMovieName ?? movieName;
                Console.WriteLine($"Querying rooms for movie ID: {movieId}, Name: '{actualName}'");
                
                // Query rooms directly from Rooms table using MovieId
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, RoomNumber, Name 
                    FROM Rooms
                    WHERE MovieId = @movieId
                    ORDER BY RoomNumber";
                command.Parameters.AddWithValue("@movieId", movieId.Value);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var room = new RoomInfo
                        {
                            Id = reader.GetInt64(0),
                            RoomNumber = reader.GetInt32(1),
                            Name = reader.GetString(2)
                        };
                        rooms.Add(room);
                        Console.WriteLine($"  Found room: {room.Name} (Room {room.RoomNumber}, ID: {room.Id})");
                    }
                }
                
                Console.WriteLine($"✅ GetRoomsForMovie('{movieName}') returned {rooms.Count} rooms");
            }
            return rooms;
        }

        public Dictionary<string, SeatInfo> GetSeatsForRoom(long roomId)
        {
            var seats = new Dictionary<string, SeatInfo>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, SeatName, SeatType, IsBooked 
                    FROM Seats 
                    WHERE RoomId = @roomId
                    ORDER BY SeatName";
                command.Parameters.AddWithValue("@roomId", roomId);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var seat = new SeatInfo
                        {
                            Id = reader.GetInt64(0),
                            SeatName = reader.GetString(1),
                            SeatType = reader.GetString(2),
                            IsBooked = reader.GetInt32(3) == 1
                        };
                        seats[seat.SeatName] = seat;
                    }
                }
            }
            return seats;
        }

        public bool BookSeats(string customerName, string movieName, string roomName, List<string> seatNames, out string errorMessage)
        {
            errorMessage = "";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                
                try
                {
                    // Get MovieId
                    var getMovieCmd = connection.CreateCommand();
                    getMovieCmd.CommandText = "SELECT Id, BasePrice FROM Movies WHERE Name = @name";
                    getMovieCmd.Parameters.AddWithValue("@name", movieName);
                    var movieReader = getMovieCmd.ExecuteReader();
                    if (!movieReader.Read())
                    {
                        errorMessage = "Phim không tồn tại";
                        movieReader.Close();
                        transaction.Rollback();
                        return false;
                    }
                    long movieId = movieReader.GetInt64(0);
                    double basePrice = movieReader.GetDouble(1);
                    movieReader.Close();

                    // Get RoomId - must match both movie and room name (since rooms are now per movie)
                    var getRoomCmd = connection.CreateCommand();
                    getRoomCmd.CommandText = @"
                        SELECT Id FROM Rooms 
                        WHERE MovieId = @movieId AND Name = @name";
                    getRoomCmd.Parameters.AddWithValue("@movieId", movieId);
                    getRoomCmd.Parameters.AddWithValue("@name", roomName);
                    var roomReader = getRoomCmd.ExecuteReader();
                    if (!roomReader.Read())
                    {
                        errorMessage = $"Phòng '{roomName}' không tồn tại cho phim này";
                        roomReader.Close();
                        transaction.Rollback();
                        return false;
                    }
                    long roomId = roomReader.GetInt64(0);
                    roomReader.Close();

                    // Check and get seat info
                    var priceMultipliers = new Dictionary<string, double>
                    {
                        { "Vé vớt", 0.25 },
                        { "Vé thường", 1.0 },
                        { "Vé VIP", 2.0 }
                    };

                    double totalPrice = 0;
                    var seatIds = new List<long>();
                    var validSeats = new List<string>();

                    foreach (var seatName in seatNames)
                    {
                        var checkSeatCmd = connection.CreateCommand();
                        checkSeatCmd.CommandText = @"
                            SELECT Id, SeatType, IsBooked 
                            FROM Seats 
                            WHERE RoomId = @roomId AND SeatName = @seatName";
                        checkSeatCmd.Parameters.AddWithValue("@roomId", roomId);
                        checkSeatCmd.Parameters.AddWithValue("@seatName", seatName);
                        var seatReader = checkSeatCmd.ExecuteReader();
                        
                        if (!seatReader.Read())
                        {
                            errorMessage = $"Ghế {seatName} không tồn tại";
                            seatReader.Close();
                            transaction.Rollback();
                            return false;
                        }

                        bool isBooked = seatReader.GetInt32(2) == 1;
                        if (isBooked)
                        {
                            errorMessage = $"Ghế {seatName} đã được đặt";
                            seatReader.Close();
                            transaction.Rollback();
                            return false;
                        }

                        long seatId = seatReader.GetInt64(0);
                        string seatType = seatReader.GetString(1);
                        seatIds.Add(seatId);
                        validSeats.Add(seatName);
                        totalPrice += basePrice * priceMultipliers[seatType];
                        seatReader.Close();
                    }

                    // Create booking
                    var insertBookingCmd = connection.CreateCommand();
                    insertBookingCmd.CommandText = @"
                        INSERT INTO Bookings (CustomerName, MovieId, RoomId, BookingTime, TotalPrice)
                        VALUES (@customerName, @movieId, @roomId, @bookingTime, @totalPrice)";
                    insertBookingCmd.Parameters.AddWithValue("@customerName", customerName);
                    insertBookingCmd.Parameters.AddWithValue("@movieId", movieId);
                    insertBookingCmd.Parameters.AddWithValue("@roomId", roomId);
                    insertBookingCmd.Parameters.AddWithValue("@bookingTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    insertBookingCmd.Parameters.AddWithValue("@totalPrice", totalPrice);
                    insertBookingCmd.ExecuteNonQuery();

                    // Get BookingId
                    var getBookingIdCmd = connection.CreateCommand();
                    getBookingIdCmd.CommandText = "SELECT last_insert_rowid()";
                    long bookingId = Convert.ToInt64(getBookingIdCmd.ExecuteScalar());

                    // Mark seats as booked and add to BookingSeats
                    var updateSeatCmd = connection.CreateCommand();
                    updateSeatCmd.CommandText = "UPDATE Seats SET IsBooked = 1 WHERE Id = @seatId";
                    updateSeatCmd.Parameters.Add("@seatId", SqliteType.Integer);

                    var insertBookingSeatCmd = connection.CreateCommand();
                    insertBookingSeatCmd.CommandText = "INSERT INTO BookingSeats (BookingId, SeatId) VALUES (@bookingId, @seatId)";
                    insertBookingSeatCmd.Parameters.Add("@bookingId", SqliteType.Integer);
                    insertBookingSeatCmd.Parameters.Add("@seatId", SqliteType.Integer);

                    foreach (long seatId in seatIds)
                    {
                        updateSeatCmd.Parameters["@seatId"].Value = seatId;
                        updateSeatCmd.ExecuteNonQuery();

                        insertBookingSeatCmd.Parameters["@bookingId"].Value = bookingId;
                        insertBookingSeatCmd.Parameters["@seatId"].Value = seatId;
                        insertBookingSeatCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    errorMessage = ex.Message;
                    return false;
                }
            }
        }
    }
}

