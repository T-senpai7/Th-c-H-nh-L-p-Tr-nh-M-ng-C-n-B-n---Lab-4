using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Bai4
{
    public class SimpleHttpServer
    {
        private TcpListener? listener;
        private bool isRunning = false;
        private Thread? serverThread;
        private int port;
        private string baseDirectory;
        private CinemaWebDatabase? webDatabase;

        public SimpleHttpServer(int port, string baseDirectory)
        {
            this.port = port;
            this.baseDirectory = baseDirectory;
            // Initialize web database
            try
            {
                webDatabase = new CinemaWebDatabase();
                Console.WriteLine("Web database initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to initialize web database: {ex.Message}");
            }
        }

        public void Start()
        {
            if (isRunning) return;

            isRunning = true;
            // Listen on all network interfaces to allow connections from other machines
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            
            serverThread = new Thread(RunServer);
            serverThread.IsBackground = true;
            serverThread.Start();

            Console.WriteLine($"HTTP Server started at http://0.0.0.0:{port}");
            Console.WriteLine($"Server accessible from localhost: http://localhost:{port}");
            Console.WriteLine($"Server accessible from network: http://<your-ip>:{port}");
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
            Console.WriteLine("HTTP Server stopped");
        }

        private void RunServer()
        {
            while (isRunning)
            {
                try
                {
                    var client = listener?.AcceptTcpClient();
                    if (client != null)
                    {
                        Task.Run(() => HandleClient(client));
                    }
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Console.WriteLine($"Error accepting client: {ex.Message}");
                    }
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8, false, 4096, true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true) { AutoFlush = true })
                {
                    var request = reader.ReadLine();
                    if (string.IsNullOrEmpty(request)) return;

                    var parts = request.Split(' ');
                    if (parts.Length < 2) return;

                    var method = parts[0];
                    var fullPath = parts[1];
                    var pathParts = fullPath.Split('?');
                    var path = pathParts[0];
                    var queryString = pathParts.Length > 1 ? pathParts[1] : "";

                    // Handle CORS preflight
                    if (method == "OPTIONS")
                    {
                        writer.WriteLine("HTTP/1.1 200 OK");
                        SendCorsHeadersOnly(writer);
                        writer.WriteLine();
                        return;
                    }

                    // Serve static files or API endpoints
                    if (path.StartsWith("/api/"))
                    {
                        HandleApiRequest(path, method, writer, stream, reader, queryString);
                    }
                    else
                    {
                        ServeFile(path, writer, stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                try { client.Close(); } catch { }
            }
        }

        private void HandleApiRequest(string path, string method, StreamWriter writer, Stream stream, StreamReader reader, string queryString = "")
        {
            if (path == "/api/movies")
            {
                var scraper = new MovieScraper();
                var movies = scraper.LoadMoviesFromJson();
                
                var json = System.Text.Json.JsonSerializer.Serialize(movies, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
            else if (path.StartsWith("/api/tcp-connect") && method == "GET")
            {
                HandleTcpConnect(path, writer);
            }
            else if (path == "/api/tcp-send" && method == "POST")
            {
                HandleTcpSend(writer, stream, reader);
            }
            else if (path == "/api/web/movies" && method == "GET")
            {
                HandleWebMovies(writer);
            }
            else if (path == "/api/web/rooms" && method == "GET")
            {
                HandleWebRooms(queryString, writer);
            }
            else if (path == "/api/web/seats" && method == "GET")
            {
                HandleWebSeats(queryString, writer);
            }
            else if (path == "/api/web/booking" && method == "POST")
            {
                HandleWebBooking(writer, stream, reader);
            }
            else if (path == "/api/booking" && method == "POST")
            {
                HandleBookingRequest(writer, stream, reader);
            }
            else if (path == "/api/proxy-image" && method == "GET")
            {
                HandleProxyImage(queryString, writer, stream);
            }
            else
            {
                writer.WriteLine("HTTP/1.1 404 Not Found");
                writer.WriteLine();
            }
        }

        private void HandleTcpConnect(string path, StreamWriter writer)
        {
            // Parse query parameters
            var queryParts = path.Split('?');
            string serverIP = "127.0.0.1";
            int serverPort = 8080;

            if (queryParts.Length > 1)
            {
                var paramsStr = queryParts[1];
                var params_ = paramsStr.Split('&');
                foreach (var param in params_)
                {
                    var parts = param.Split('=');
                    if (parts.Length == 2)
                    {
                        if (parts[0] == "ip")
                            serverIP = Uri.UnescapeDataString(parts[1]);
                        else if (parts[0] == "port")
                            int.TryParse(parts[1], out serverPort);
                    }
                }
            }

            try
            {
                // Test connection by sending a simple message
                var testResult = TcpProxyManager.SendMessage(serverIP, serverPort, "GET_MOVIES|");
                bool connected = !testResult.StartsWith("ERROR");

                var result = new
                {
                    success = connected,
                    error = connected ? null : "Không thể kết nối đến server"
                };

                var json = System.Text.Json.JsonSerializer.Serialize(result);
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
            catch (Exception ex)
            {
                var result = new { success = false, error = ex.Message };
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
        }

        private void HandleTcpSend(StreamWriter writer, Stream stream, StreamReader reader)
        {
            // Read request body
            string? line;
            int contentLength = 0;
            while ((line = reader.ReadLine()) != null && !string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(line.Substring(15).Trim(), out contentLength);
                }
            }

            string requestBody = "";
            if (contentLength > 0)
            {
                var buffer = new char[contentLength];
                reader.Read(buffer, 0, contentLength);
                requestBody = new string(buffer);
            }

            try
            {
                using var jsonDoc = JsonDocument.Parse(requestBody);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("message", out var messageElem))
                {
                    throw new Exception("Missing 'message' in request");
                }

                string message = messageElem.GetString() ?? "";
                string serverIP = "127.0.0.1";
                int serverPort = 8080;

                if (root.TryGetProperty("serverIP", out var ipElem))
                {
                    serverIP = ipElem.GetString() ?? "127.0.0.1";
                }

                if (root.TryGetProperty("serverPort", out var portElem))
                {
                    if (portElem.ValueKind == JsonValueKind.Number)
                        serverPort = portElem.GetInt32();
                    else if (portElem.ValueKind == JsonValueKind.String)
                        int.TryParse(portElem.GetString(), out serverPort);
                }

                string response = TcpProxyManager.SendMessage(serverIP, serverPort, message);

                var result = new
                {
                    success = !response.StartsWith("ERROR"),
                    response = response
                };

                var json = System.Text.Json.JsonSerializer.Serialize(result);
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
            catch (Exception ex)
            {
                var result = new { success = false, error = ex.Message };
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
        }

        private void HandleBookingRequest(StreamWriter writer, Stream stream, StreamReader reader)
        {
            // Skip headers to read body
            string? line;
            int contentLength = 0;
            while ((line = reader.ReadLine()) != null && !string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(line.Substring(15).Trim(), out contentLength))
                    {
                        // Got content length
                    }
                }
            }

            // Read request body if available
            string requestBody = "";
            if (contentLength > 0)
            {
                var buffer = new char[contentLength];
                reader.Read(buffer, 0, contentLength);
                requestBody = new string(buffer);
            }

            SendCorsHeaders(writer);
            writer.WriteLine("Content-Type: application/json; charset=utf-8");
            writer.WriteLine();
            writer.Write("{\"status\":\"success\",\"message\":\"Booking request received. Please complete booking in the client application.\"}");
        }

        private void ServeFile(string path, StreamWriter writer, Stream stream)
        {
            if (path == "/") path = "/Viewing.html";
            if (path == "/index.html") path = "/Viewing.html";
            
            // Handle different file requests
            var fileName = path.TrimStart('/');
            var filePath = Path.Combine(baseDirectory, fileName);
            
            // Try to find file with different names
            if (!File.Exists(filePath))
            {
                // Try alternative file names
                if (fileName.EndsWith(".html"))
                {
                    var altPath = Path.Combine(baseDirectory, "index.html");
                    if (File.Exists(altPath))
                    {
                        filePath = altPath;
                    }
                    else
                    {
                        altPath = Path.Combine(baseDirectory, "Viewing.html");
                        if (File.Exists(altPath))
                        {
                            filePath = altPath;
                        }
                    }
                }
            }
            
            if (!File.Exists(filePath))
            {
                writer.WriteLine("HTTP/1.1 404 Not Found");
                writer.WriteLine("Content-Type: text/plain; charset=utf-8");
                writer.WriteLine();
                writer.Write($"404 - File not found\n\nRequested: {path}\nSearched in: {baseDirectory}\nFile path: {filePath}");
                Console.WriteLine($"404: {path} -> {filePath} (Base: {baseDirectory})");
                return;
            }
            
            Console.WriteLine($"Serving: {path} -> {filePath}");

            var content = File.ReadAllText(filePath, Encoding.UTF8);
            var contentType = GetContentType(filePath);

            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine($"Content-Type: {contentType}; charset=utf-8");
            writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(content)}");
            writer.WriteLine();
            writer.Write(content);
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        private void SendCorsHeaders(StreamWriter writer)
        {
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine("Access-Control-Allow-Origin: *");
            writer.WriteLine("Access-Control-Allow-Methods: GET, POST, OPTIONS");
            writer.WriteLine("Access-Control-Allow-Headers: Content-Type");
        }
        
        private void SendCorsHeadersOnly(StreamWriter writer)
        {
            writer.WriteLine("Access-Control-Allow-Origin: *");
            writer.WriteLine("Access-Control-Allow-Methods: GET, POST, OPTIONS");
            writer.WriteLine("Access-Control-Allow-Headers: Content-Type");
        }

        // Web Database API handlers
        private void HandleWebMovies(StreamWriter writer)
        {
            try
            {
                if (webDatabase == null)
                {
                    throw new Exception("Database not initialized");
                }

                var movies = webDatabase.GetMovies();
                var movieList = movies.Select(m => $"{m.Name}:{m.BasePrice}").ToArray();
                var response = "MOVIES|" + string.Join(";", movieList);

                var result = new { success = true, response = response };
                var json = JsonSerializer.Serialize(result);
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
            catch (Exception ex)
            {
                var result = new { success = false, error = ex.Message };
                var json = JsonSerializer.Serialize(result);
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
        }

        private void HandleWebRooms(string queryString, StreamWriter writer)
        {
            try
            {
                Console.WriteLine("=== HandleWebRooms: Starting ===");
                Console.WriteLine($"Query string: '{queryString}'");
                
                if (webDatabase == null)
                {
                    Console.WriteLine("ERROR: Database not initialized");
                    throw new Exception("Database not initialized");
                }

                // Parse query: movie=MovieName
                string movieName = "";
                if (!string.IsNullOrEmpty(queryString))
                {
                    var params_ = queryString.Split('&');
                    Console.WriteLine($"Split query into {params_.Length} parameters");
                    
                    foreach (var param in params_)
                    {
                        Console.WriteLine($"  Processing parameter: '{param}'");
                        var parts = param.Split('=');
                        if (parts.Length == 2 && parts[0] == "movie")
                        {
                            movieName = Uri.UnescapeDataString(parts[1]);
                            Console.WriteLine($"  Found movie parameter: '{movieName}'");
                        }
                    }
                }

                if (string.IsNullOrEmpty(movieName))
                {
                    Console.WriteLine($"ERROR: Movie name is required. Query string: '{queryString}'");
                    throw new Exception("Movie name is required");
                }

                // Trim whitespace from movie name
                movieName = movieName.Trim();
                Console.WriteLine($"Getting rooms for movie (trimmed): '{movieName}'");
                Console.WriteLine($"Movie name length: {movieName.Length}");
                Console.WriteLine($"Movie name bytes: {Encoding.UTF8.GetBytes(movieName).Length}");
                
                var rooms = webDatabase.GetRoomsForMovie(movieName);
                Console.WriteLine($"GetRoomsForMovie returned {rooms.Count} rooms");
                
                if (rooms.Count == 0)
                {
                    Console.WriteLine($"WARNING: No rooms found for movie '{movieName}'");
                    Console.WriteLine("This could mean:");
                    Console.WriteLine("  1. Movie doesn't exist in database");
                    Console.WriteLine("  2. Movie exists but has no rooms assigned");
                    Console.WriteLine("  3. Movie name doesn't match exactly (case/whitespace)");
                } else {
                    Console.WriteLine($"Found rooms:");
                    foreach (var room in rooms)
                    {
                        Console.WriteLine($"  - {room.Name} (Room {room.RoomNumber}, ID: {room.Id})");
                    }
                }
                
                var roomList = rooms.Select(r => r.Name).ToArray();
                var response = "ROOMS|" + string.Join(";", roomList);
                
                Console.WriteLine($"Response string: '{response}'");
                Console.WriteLine($"Response length: {response.Length}");
                Console.WriteLine($"Room list: [{string.Join(", ", roomList)}]");

                var result = new { success = true, response = response };
                var json = JsonSerializer.Serialize(result);
                Console.WriteLine($"JSON response: {json}");
                
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
                writer.Flush();
                Console.WriteLine("✅ Response sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in HandleWebRooms: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                var result = new { success = false, error = ex.Message };
                var json = JsonSerializer.Serialize(result);
                
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
                writer.Flush();
            }
        }

        private void HandleWebSeats(string queryString, StreamWriter writer)
        {
            try
            {
                if (webDatabase == null)
                {
                    throw new Exception("Database not initialized");
                }

                // Parse query: movie=MovieName&room=RoomName
                string movieName = "";
                string roomName = "";
                if (!string.IsNullOrEmpty(queryString))
                {
                    var params_ = queryString.Split('&');
                    foreach (var param in params_)
                    {
                        var parts = param.Split('=');
                        if (parts.Length == 2)
                        {
                            if (parts[0] == "movie")
                                movieName = Uri.UnescapeDataString(parts[1]);
                            else if (parts[0] == "room")
                                roomName = Uri.UnescapeDataString(parts[1]);
                        }
                    }
                }

                if (string.IsNullOrEmpty(movieName) || string.IsNullOrEmpty(roomName))
                {
                    throw new Exception("Movie name and room name are required");
                }

                var rooms = webDatabase.GetRoomsForMovie(movieName);
                var room = rooms.FirstOrDefault(r => r.Name == roomName);
                if (room == null)
                {
                    throw new Exception("Room not found");
                }

                var seats = webDatabase.GetSeatsForRoom(room.Id);
                var seatList = seats.Select(s => $"{s.Value.SeatName}:{s.Value.SeatType}:{(s.Value.IsBooked ? 1 : 0)}").ToArray();
                var response = "SEATS|" + string.Join(";", seatList);

                var result = new { success = true, response = response };
                var json = JsonSerializer.Serialize(result);
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
            catch (Exception ex)
            {
                var result = new { success = false, error = ex.Message };
                var json = JsonSerializer.Serialize(result);
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
            }
        }

        private void HandleWebBooking(StreamWriter writer, Stream stream, StreamReader reader)
        {
            try
            {
                if (webDatabase == null)
                {
                    throw new Exception("Database not initialized");
                }

                Console.WriteLine("=== HandleWebBooking: Starting request processing ===");
                
                // Read request headers
                string? line;
                int contentLength = 0;
                while ((line = reader.ReadLine()) != null && !string.IsNullOrEmpty(line))
                {
                    if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(line.Substring(15).Trim(), out contentLength);
                        Console.WriteLine($"Content-Length: {contentLength}");
                    }
                }

                // Read request body - use same simple approach as HandleTcpSend
                string requestBody = "";
                if (contentLength > 0)
                {
                    Console.WriteLine($"Reading {contentLength} characters from reader...");
                    var buffer = new char[contentLength];
                    int charsRead = reader.Read(buffer, 0, contentLength);
                    requestBody = new string(buffer, 0, charsRead);
                    Console.WriteLine($"Read {charsRead} characters, request body length: {requestBody.Length}");
                    
                    if (charsRead < contentLength)
                    {
                        Console.WriteLine($"Warning: Only read {charsRead} of {contentLength} characters");
                    }
                } else {
                    Console.WriteLine("Warning: Content-Length is 0 or not found");
                }

                if (string.IsNullOrEmpty(requestBody))
                {
                    Console.WriteLine("ERROR: Request body is empty!");
                    throw new Exception("Request body is empty");
                } else {
                    Console.WriteLine($"Booking request body (first 200 chars): {requestBody.Substring(0, Math.Min(200, requestBody.Length))}");
                }

                // Parse booking request
                using var jsonDoc = JsonDocument.Parse(requestBody);
                var root = jsonDoc.RootElement;

                string customerName = root.GetProperty("customerName").GetString() ?? "";
                string movieName = root.GetProperty("movieName").GetString() ?? "";
                string roomName = root.GetProperty("roomName").GetString() ?? "";
                var seatNamesJson = root.GetProperty("seatNames").EnumerateArray();
                var seatNames = new List<string>();
                foreach (var seat in seatNamesJson)
                {
                    seatNames.Add(seat.GetString() ?? "");
                }

                Console.WriteLine($"Booking: Customer={customerName}, Movie={movieName}, Room={roomName}, Seats={string.Join(",", seatNames)}");

                if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(movieName) || 
                    string.IsNullOrEmpty(roomName) || seatNames.Count == 0)
                {
                    throw new Exception("Invalid booking request: Missing required fields");
                }

                // Book seats
                Console.WriteLine("Calling webDatabase.BookSeats...");
                bool success = webDatabase.BookSeats(customerName, movieName, roomName, seatNames, out string errorMessage);

                Console.WriteLine($"Booking result: Success={success}, Error={errorMessage}");

                // If booking successful, save to output_booking.json
                if (success)
                {
                    try
                    {
                        Console.WriteLine("=== Starting to save booking to JSON ===");
                        Console.WriteLine($"Base directory: {baseDirectory}");
                        
                        // Get movie base price to calculate total
                        var movies = webDatabase.GetMovies();
                        var movie = movies.FirstOrDefault(m => m.Name == movieName);
                        double basePrice = movie?.BasePrice ?? 0;
                        Console.WriteLine($"Movie: {movieName}, Base price: {basePrice}");

                        // Calculate total price based on seat types
                        double totalPrice = CalculateTotalPrice(basePrice, seatNames);
                        Console.WriteLine($"Calculated total price: {totalPrice}");

                        // Save booking to JSON file
                        SaveBookingToJson(customerName, movieName, roomName, seatNames, totalPrice);
                        Console.WriteLine("=== Booking saved to JSON successfully ===");
                    }
                    catch (Exception jsonEx)
                    {
                        Console.WriteLine($"❌❌❌ ERROR: Failed to save booking to JSON: {jsonEx.Message}");
                        Console.WriteLine($"Stack trace: {jsonEx.StackTrace}");
                        // Don't fail the booking if JSON save fails
                    }
                }

                var result = new
                {
                    success = success,
                    response = success ? "BOOK_SUCCESS" : $"BOOK_ERROR|{errorMessage}"
                };

                var json = JsonSerializer.Serialize(result);
                Console.WriteLine($"Sending booking response: {json}");
                Console.WriteLine($"Response JSON length: {json.Length} bytes");
                
                try
                {
                    SendCorsHeaders(writer);
                    writer.WriteLine("Content-Type: application/json; charset=utf-8");
                    writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                    writer.WriteLine();
                    writer.Write(json);
                    writer.Flush();
                    Console.WriteLine("✅ Response sent successfully");
                }
                catch (Exception flushEx)
                {
                    Console.WriteLine($"❌ Error sending response: {flushEx.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Booking error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                var result = new { success = false, error = ex.Message };
                var json = JsonSerializer.Serialize(result);
                
                SendCorsHeaders(writer);
                writer.WriteLine("Content-Type: application/json; charset=utf-8");
                writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(json)}");
                writer.WriteLine();
                writer.Write(json);
                writer.Flush();
            }
        }

        private string FindBai4Directory()
        {
            // Method to find Bai4 directory (where movies.json is located)
            // This ensures output_booking.json is created in Bai4 folder, not in bin\Debug
            
            // First, check if baseDirectory contains movies.json
            if (File.Exists(Path.Combine(baseDirectory, "movies.json")))
            {
                Console.WriteLine($"✅ Found Bai4 directory (baseDirectory): {baseDirectory}");
                return baseDirectory;
            }
            
            // Try current working directory
            var cwd = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(cwd, "movies.json")))
            {
                Console.WriteLine($"✅ Found Bai4 directory (current working directory): {cwd}");
                return cwd;
            }
            
            // Try to find by going up from current directory
            var currentDir = new DirectoryInfo(cwd);
            for (int i = 0; i < 10 && currentDir != null; i++)
            {
                var moviesJsonPath = Path.Combine(currentDir.FullName, "movies.json");
                if (File.Exists(moviesJsonPath))
                {
                    Console.WriteLine($"✅ Found Bai4 directory (searched up {i} levels): {currentDir.FullName}");
                    return currentDir.FullName;
                }
                currentDir = currentDir.Parent;
            }
            
            // Try going up from baseDirectory
            var baseDirInfo = new DirectoryInfo(baseDirectory);
            for (int i = 0; i < 10 && baseDirInfo != null; i++)
            {
                var moviesJsonPath = Path.Combine(baseDirInfo.FullName, "movies.json");
                if (File.Exists(moviesJsonPath))
                {
                    Console.WriteLine($"✅ Found Bai4 directory (from baseDirectory, up {i} levels): {baseDirInfo.FullName}");
                    return baseDirInfo.FullName;
                }
                baseDirInfo = baseDirInfo.Parent;
            }
            
            Console.WriteLine($"❌ Could not find Bai4 directory (movies.json not found)");
            Console.WriteLine($"   Searched from: {cwd}");
            Console.WriteLine($"   And from: {baseDirectory}");
            return null;
        }

        private double CalculateTotalPrice(double basePrice, List<string> seatNames)
        {
            // Seat types and price multipliers (matching booking.js)
            var priceMultipliers = new Dictionary<string, double>
            {
                { "Vé vớt", 0.25 },
                { "Vé thường", 1.0 },
                { "Vé VIP", 2.0 }
            };

            // Seat type mapping (matching booking.js)
            var seatTypes = new Dictionary<string, string>
            {
                { "A1", "Vé vớt" }, { "A5", "Vé vớt" },
                { "C1", "Vé vớt" }, { "C5", "Vé vớt" },
                { "A2", "Vé thường" }, { "A3", "Vé thường" }, { "A4", "Vé thường" },
                { "C2", "Vé thường" }, { "C3", "Vé thường" }, { "C4", "Vé thường" },
                { "B1", "Vé VIP" }, { "B2", "Vé VIP" }, { "B3", "Vé VIP" },
                { "B4", "Vé VIP" }, { "B5", "Vé VIP" }
            };

            double total = 0;
            foreach (var seatName in seatNames)
            {
                string seatType = seatTypes.ContainsKey(seatName) ? seatTypes[seatName] : "Vé thường";
                double multiplier = priceMultipliers.ContainsKey(seatType) ? priceMultipliers[seatType] : 1.0;
                total += basePrice * multiplier;
            }

            return total;
        }

        private void SaveBookingToJson(string customerName, string movieName, string roomName, List<string> seatNames, double totalPrice)
        {
            try
            {
                // Always find Bai4 directory (where movies.json is located)
                // This ensures output_booking.json is created in Bai4 folder, not in bin\Debug
                string targetDirectory = FindBai4Directory();
                
                if (string.IsNullOrEmpty(targetDirectory))
                {
                    throw new Exception("Could not find Bai4 directory (movies.json not found)");
                }
                
                // Define the output file path (in Bai4 directory)
                string outputFile = Path.Combine(targetDirectory, "output_booking.json");
                string absolutePath = Path.GetFullPath(outputFile);
                
                Console.WriteLine($"=== SaveBookingToJson called ===");
                Console.WriteLine($"Base directory (server): {baseDirectory}");
                Console.WriteLine($"Target directory (Bai4): {targetDirectory}");
                Console.WriteLine($"Output file: {outputFile}");
                Console.WriteLine($"Output file (absolute): {absolutePath}");
                Console.WriteLine($"Target directory exists: {Directory.Exists(targetDirectory)}");
                Console.WriteLine($"movies.json exists in target: {File.Exists(Path.Combine(targetDirectory, "movies.json"))}");
                Console.WriteLine($"File exists: {File.Exists(outputFile)}");

                // Booking entry
                var bookingEntry = new Dictionary<string, object>
                {
                    { "TenNguoiDat", customerName },
                    { "TenPhim", movieName },
                    { "Phong", roomName },
                    { "Ghe", string.Join(", ", seatNames) }, // Join seats with comma and space
                    { "Gia", totalPrice },
                    { "NoteForMore", "" } // Empty for now, will be enhanced later
                };

                Console.WriteLine($"Booking entry created: {JsonSerializer.Serialize(bookingEntry)}");

                // Read existing bookings or create new list
                List<Dictionary<string, object>> bookings = new List<Dictionary<string, object>>();
                if (File.Exists(outputFile))
                {
                    try
                    {
                        Console.WriteLine("Reading existing bookings from file...");
                        string existingJson = File.ReadAllText(outputFile, Encoding.UTF8);
                        Console.WriteLine($"Existing JSON length: {existingJson.Length} characters");
                        
                        if (!string.IsNullOrWhiteSpace(existingJson))
                        {
                            using var doc = JsonDocument.Parse(existingJson);
                            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                int count = 0;
                                foreach (var element in doc.RootElement.EnumerateArray())
                                {
                                    var booking = new Dictionary<string, object>();
                                    foreach (var prop in element.EnumerateObject())
                                    {
                                        // Convert JsonElement to appropriate type
                                        object value = prop.Value.ValueKind switch
                                        {
                                            JsonValueKind.String => prop.Value.GetString() ?? "",
                                            JsonValueKind.Number => prop.Value.GetDouble(),
                                            JsonValueKind.True => true,
                                            JsonValueKind.False => false,
                                            _ => prop.Value.GetRawText()
                                        };
                                        booking[prop.Name] = value;
                                    }
                                    bookings.Add(booking);
                                    count++;
                                }
                                Console.WriteLine($"Loaded {count} existing bookings");
                            }
                            else
                            {
                                Console.WriteLine("Warning: Existing JSON is not an array, starting fresh");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Existing file is empty, starting fresh");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Warning: Failed to read existing bookings from JSON: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        // Continue with empty list
                    }
                }
                else
                {
                    Console.WriteLine("Output file does not exist, will create new file");
                }

                // Add new booking
                bookings.Add(bookingEntry);
                Console.WriteLine($"Total bookings to save: {bookings.Count}");

                // Write back to file with pretty formatting
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string json = JsonSerializer.Serialize(bookings, options);
                Console.WriteLine($"JSON to write (first 200 chars): {json.Substring(0, Math.Min(200, json.Length))}...");
                Console.WriteLine($"JSON total length: {json.Length} characters");
                
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Console.WriteLine($"Creating directory: {directory}");
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(absolutePath, json, Encoding.UTF8);
                Console.WriteLine($"✅ File written successfully to: {absolutePath}");
                
                // Verify file was written
                if (File.Exists(absolutePath))
                {
                    var fileInfo = new FileInfo(absolutePath);
                    Console.WriteLine($"✅ File verified: Size = {fileInfo.Length} bytes, LastWrite = {fileInfo.LastWriteTime}");
                }
                else
                {
                    Console.WriteLine($"❌ ERROR: File was not created at {absolutePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌❌❌ ERROR in SaveBookingToJson: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to be caught by caller
            }
        }

        private void HandleProxyImage(string queryString, StreamWriter writer, Stream stream)
        {
            try
            {
                // Parse query parameters
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(queryString))
                {
                    foreach (var param in queryString.Split('&'))
                    {
                        var parts = param.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            queryParams[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1]);
                        }
                    }
                }

                if (!queryParams.ContainsKey("url"))
                {
                    writer.WriteLine("HTTP/1.1 400 Bad Request");
                    SendCorsHeadersOnly(writer);
                    writer.WriteLine();
                    return;
                }

                var imageUrl = queryParams["url"];
                
                // Decode URL nếu đã bị encode nhiều lần
                string decodedUrl = imageUrl;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        var temp = Uri.UnescapeDataString(decodedUrl);
                        if (temp == decodedUrl || (!temp.Contains("%2f") && !temp.Contains("%2F")))
                        {
                            decodedUrl = temp;
                            break;
                        }
                        decodedUrl = temp;
                    }
                    catch
                    {
                        break;
                    }
                }

                Console.WriteLine($"Proxying image: {decodedUrl}");

                // Fetch image using HttpClient
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://www.betacinemas.vn/");
                    httpClient.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");

                    var response = httpClient.GetAsync(decodedUrl).Result;
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        writer.WriteLine($"HTTP/1.1 {response.StatusCode}");
                        SendCorsHeadersOnly(writer);
                        writer.WriteLine();
                        return;
                    }

                    var imageBytes = response.Content.ReadAsByteArrayAsync().Result;
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

                    SendCorsHeaders(writer);
                    writer.WriteLine($"Content-Type: {contentType}");
                    writer.WriteLine("Cache-Control: public, max-age=86400");
                    writer.WriteLine($"Content-Length: {imageBytes.Length}");
                    writer.WriteLine();
                    writer.Flush();

                    // Write image bytes to stream
                    stream.Write(imageBytes, 0, imageBytes.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error proxying image: {ex.Message}");
                writer.WriteLine("HTTP/1.1 500 Internal Server Error");
                SendCorsHeadersOnly(writer);
                writer.WriteLine();
            }
        }
    }
}

