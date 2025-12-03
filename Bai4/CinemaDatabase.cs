using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Bai4
{
    public class CinemaDatabase
    {
        private string connectionString;
        private const string DB_NAME = "cinema_database.db";

        public CinemaDatabase()
        {
            connectionString = $"Data Source={DB_NAME}";
            InitializeDatabase();
            InitializeData();
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

                // Tạo bảng Rooms
                var createRoomsTable = @"
                    CREATE TABLE IF NOT EXISTS Rooms (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        RoomNumber INTEGER NOT NULL UNIQUE,
                        Name TEXT NOT NULL
                    )";

                // Tạo bảng MovieRooms (quan hệ many-to-many)
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

        private void InitializeData()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Kiểm tra xem đã có dữ liệu chưa
                var checkData = connection.CreateCommand();
                checkData.CommandText = "SELECT COUNT(*) FROM Movies";
                var count = Convert.ToInt64(checkData.ExecuteScalar());
                
                if (count > 0) return; // Đã có dữ liệu

                // Thêm phim
                var insertMovie = connection.CreateCommand();
                insertMovie.CommandText = @"
                    INSERT INTO Movies (Name, BasePrice) VALUES 
                    (@name, @price)";
                insertMovie.Parameters.Add("@name", SqliteType.Text);
                insertMovie.Parameters.Add("@price", SqliteType.Real);

                var movies = new Dictionary<string, double>
                {
                    { "Đào, phở và piano", 45000 },
                    { "Mai", 100000 },
                    { "Gặp lại chị bầu", 70000 },
                    { "Tarot", 90000 }
                };

                var movieIds = new Dictionary<string, long>();
                foreach (var movie in movies)
                {
                    insertMovie.Parameters["@name"].Value = movie.Key;
                    insertMovie.Parameters["@price"].Value = movie.Value;
                    insertMovie.ExecuteNonQuery();
                    
                    // Lấy ID vừa insert
                    var getLastId = connection.CreateCommand();
                    getLastId.CommandText = "SELECT last_insert_rowid()";
                    movieIds[movie.Key] = Convert.ToInt64(getLastId.ExecuteScalar());
                }

                // Thêm phòng
                var insertRoom = connection.CreateCommand();
                insertRoom.CommandText = @"
                    INSERT INTO Rooms (RoomNumber, Name) VALUES 
                    (@number, @name)";
                insertRoom.Parameters.Add("@number", SqliteType.Integer);
                insertRoom.Parameters.Add("@name", SqliteType.Text);

                var roomIds = new Dictionary<int, long>();
                for (int i = 1; i <= 3; i++)
                {
                    insertRoom.Parameters["@number"].Value = i;
                    insertRoom.Parameters["@name"].Value = $"Phòng {i}";
                    insertRoom.ExecuteNonQuery();
                    
                    var getLastId = connection.CreateCommand();
                    getLastId.CommandText = "SELECT last_insert_rowid()";
                    roomIds[i] = Convert.ToInt64(getLastId.ExecuteScalar());
                }

                // Thêm quan hệ Movie-Room
                var movieRooms = new Dictionary<string, List<int>>
                {
                    { "Đào, phở và piano", new List<int> { 1, 2, 3 } },
                    { "Mai", new List<int> { 2, 3 } },
                    { "Gặp lại chị bầu", new List<int> { 1 } },
                    { "Tarot", new List<int> { 3 } }
                };

                var insertMovieRoom = connection.CreateCommand();
                insertMovieRoom.CommandText = @"
                    INSERT INTO MovieRooms (MovieId, RoomId) VALUES 
                    (@movieId, @roomId)";
                insertMovieRoom.Parameters.Add("@movieId", SqliteType.Integer);
                insertMovieRoom.Parameters.Add("@roomId", SqliteType.Integer);

                foreach (var mr in movieRooms)
                {
                    var movieId = movieIds[mr.Key];
                    foreach (var roomNum in mr.Value)
                    {
                        insertMovieRoom.Parameters["@movieId"].Value = movieId;
                        insertMovieRoom.Parameters["@roomId"].Value = roomIds[roomNum];
                        insertMovieRoom.ExecuteNonQuery();
                    }
                }

                // Thêm ghế cho mỗi phòng
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
                    INSERT INTO Seats (RoomId, SeatName, SeatType, IsBooked) VALUES 
                    (@roomId, @seatName, @seatType, 0)";
                insertSeat.Parameters.Add("@roomId", SqliteType.Integer);
                insertSeat.Parameters.Add("@seatName", SqliteType.Text);
                insertSeat.Parameters.Add("@seatType", SqliteType.Text);

                char[] rows = { 'A', 'B', 'C' };
                int[] cols = { 1, 2, 3, 4, 5 };

                foreach (var roomId in roomIds.Values)
                {
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
            var rooms = new List<RoomInfo>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT DISTINCT r.Id, r.RoomNumber, r.Name 
                    FROM Rooms r
                    INNER JOIN MovieRooms mr ON r.Id = mr.RoomId
                    INNER JOIN Movies m ON mr.MovieId = m.Id
                    WHERE m.Name = @movieName
                    ORDER BY r.RoomNumber";
                command.Parameters.AddWithValue("@movieName", movieName);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rooms.Add(new RoomInfo
                        {
                            Id = reader.GetInt64(0),
                            RoomNumber = reader.GetInt32(1),
                            Name = reader.GetString(2)
                        });
                    }
                }
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
                    // Lấy MovieId
                    var getMovieCmd = connection.CreateCommand();
                    getMovieCmd.CommandText = "SELECT Id, BasePrice FROM Movies WHERE Name = @name";
                    getMovieCmd.Parameters.AddWithValue("@name", movieName);
                    var movieReader = getMovieCmd.ExecuteReader();
                    if (!movieReader.Read())
                    {
                        errorMessage = "Phim không tồn tại";
                        transaction.Rollback();
                        return false;
                    }
                    long movieId = movieReader.GetInt64(0);
                    double basePrice = movieReader.GetDouble(1);
                    movieReader.Close();

                    // Lấy RoomId
                    var getRoomCmd = connection.CreateCommand();
                    getRoomCmd.CommandText = "SELECT Id FROM Rooms WHERE Name = @name";
                    getRoomCmd.Parameters.AddWithValue("@name", roomName);
                    var roomReader = getRoomCmd.ExecuteReader();
                    if (!roomReader.Read())
                    {
                        errorMessage = "Phòng không tồn tại";
                        transaction.Rollback();
                        return false;
                    }
                    long roomId = roomReader.GetInt64(0);
                    roomReader.Close();

                    // Kiểm tra và lấy thông tin ghế
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

                    // Tạo booking
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

                    // Lấy BookingId
                    var getBookingIdCmd = connection.CreateCommand();
                    getBookingIdCmd.CommandText = "SELECT last_insert_rowid()";
                    long bookingId = Convert.ToInt64(getBookingIdCmd.ExecuteScalar());

                    // Đánh dấu ghế đã đặt và thêm vào BookingSeats
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

    public class MovieInfo
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public double BasePrice { get; set; }
    }

    public class RoomInfo
    {
        public long Id { get; set; }
        public int RoomNumber { get; set; }
        public string Name { get; set; } = "";
    }

    public class SeatInfo
    {
        public long Id { get; set; }
        public string SeatName { get; set; } = "";
        public string SeatType { get; set; } = "";
        public bool IsBooked { get; set; }
    }
}

