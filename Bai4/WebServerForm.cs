using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bai4
{
    public class WebServerForm : Form
    {
        private SimpleHttpServer? httpServer;
        private WebTcpServer? tcpServer;
        private MovieScraper scraper;
        private Button btnStartServer;
        private Button btnStopServer;
        public Button btnStartTcpServer;
        public Button btnStopTcpServer;
        private Button btnCrawlMovies;
        private Button btnOpenBrowser;
        private Label lblStatus;
        private Label lblTcpStatus;
        private TextBox txtLog;
        private const int HTTP_PORT = 8888;
        private const int TCP_PORT = 8889;

        public WebServerForm()
        {
            scraper = new MovieScraper();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Web Server & Movie Scraper";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // HTTP Server Status label
            lblStatus = new Label();
            lblStatus.Text = "HTTP Server Status: Stopped";
            lblStatus.Location = new Point(20, 20);
            lblStatus.Size = new Size(320, 30);
            lblStatus.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(220, 53, 69);

            // TCP Server Status label
            lblTcpStatus = new Label();
            lblTcpStatus.Text = "TCP Server Status: Stopped";
            lblTcpStatus.Location = new Point(360, 20);
            lblTcpStatus.Size = new Size(320, 30);
            lblTcpStatus.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTcpStatus.ForeColor = Color.FromArgb(220, 53, 69);

            // Start HTTP Server button
            btnStartServer = new Button();
            btnStartServer.Text = "Start HTTP Server";
            btnStartServer.Location = new Point(20, 60);
            btnStartServer.Size = new Size(140, 40);
            btnStartServer.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStartServer.BackColor = Color.FromArgb(40, 167, 69);
            btnStartServer.ForeColor = Color.White;
            btnStartServer.FlatStyle = FlatStyle.Flat;
            btnStartServer.Click += BtnStartServer_Click;

            // Stop HTTP Server button
            btnStopServer = new Button();
            btnStopServer.Text = "Stop HTTP Server";
            btnStopServer.Location = new Point(170, 60);
            btnStopServer.Size = new Size(140, 40);
            btnStopServer.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStopServer.BackColor = Color.FromArgb(220, 53, 69);
            btnStopServer.ForeColor = Color.White;
            btnStopServer.FlatStyle = FlatStyle.Flat;
            btnStopServer.Enabled = false;
            btnStopServer.Click += BtnStopServer_Click;

            // Start TCP Server button
            btnStartTcpServer = new Button();
            btnStartTcpServer.Text = "Start TCP Server";
            btnStartTcpServer.Location = new Point(360, 60);
            btnStartTcpServer.Size = new Size(140, 40);
            btnStartTcpServer.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStartTcpServer.BackColor = Color.FromArgb(13, 110, 253);
            btnStartTcpServer.ForeColor = Color.White;
            btnStartTcpServer.FlatStyle = FlatStyle.Flat;
            btnStartTcpServer.Click += BtnStartTcpServer_Click;

            // Stop TCP Server button
            btnStopTcpServer = new Button();
            btnStopTcpServer.Text = "Stop TCP Server";
            btnStopTcpServer.Location = new Point(510, 60);
            btnStopTcpServer.Size = new Size(140, 40);
            btnStopTcpServer.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStopTcpServer.BackColor = Color.FromArgb(220, 53, 69);
            btnStopTcpServer.ForeColor = Color.White;
            btnStopTcpServer.FlatStyle = FlatStyle.Flat;
            btnStopTcpServer.Enabled = false;
            btnStopTcpServer.Click += BtnStopTcpServer_Click;

            // Crawl Movies button
            btnCrawlMovies = new Button();
            btnCrawlMovies.Text = "Crawl Movies";
            btnCrawlMovies.Location = new Point(20, 110);
            btnCrawlMovies.Size = new Size(150, 35);
            btnCrawlMovies.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCrawlMovies.BackColor = Color.FromArgb(255, 193, 7);
            btnCrawlMovies.ForeColor = Color.Black;
            btnCrawlMovies.FlatStyle = FlatStyle.Flat;
            btnCrawlMovies.Click += BtnCrawlMovies_Click;

            // Open Browser button
            btnOpenBrowser = new Button();
            btnOpenBrowser.Text = "Open in Browser";
            btnOpenBrowser.Location = new Point(180, 110);
            btnOpenBrowser.Size = new Size(130, 35);
            btnOpenBrowser.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOpenBrowser.BackColor = Color.FromArgb(255, 193, 7);
            btnOpenBrowser.ForeColor = Color.Black;
            btnOpenBrowser.FlatStyle = FlatStyle.Flat;
            btnOpenBrowser.Enabled = false;
            btnOpenBrowser.Click += BtnOpenBrowser_Click;

            // Log textbox
            txtLog = new TextBox();
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Location = new Point(20, 160);
            txtLog.Size = new Size(660, 460);
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.BackColor = Color.FromArgb(30, 30, 30);
            txtLog.ForeColor = Color.LightGreen;

            this.Controls.Add(lblStatus);
            this.Controls.Add(lblTcpStatus);
            this.Controls.Add(btnStartServer);
            this.Controls.Add(btnStopServer);
            this.Controls.Add(btnStartTcpServer);
            this.Controls.Add(btnStopTcpServer);
            this.Controls.Add(btnCrawlMovies);
            this.Controls.Add(btnOpenBrowser);
            this.Controls.Add(txtLog);

            AddLog("Application started. Click 'Start HTTP Server' or 'Start TCP Server' to begin.");
        }

        private void BtnStartServer_Click(object? sender, EventArgs e)
        {
            try
            {
                // Find the Bai4 directory containing Viewing.html
                // Start from executable directory and search up
                var exePath = Application.ExecutablePath;
                var exeDir = Path.GetDirectoryName(exePath) ?? ".";
                var baseDir = exeDir;
                var currentDir = new DirectoryInfo(exeDir);
                
                // Search up to find Bai4 directory
                bool found = false;
                for (int i = 0; i < 5 && currentDir != null; i++)
                {
                    var viewingPath = Path.Combine(currentDir.FullName, "Viewing.html");
                    if (File.Exists(viewingPath))
                    {
                        baseDir = currentDir.FullName;
                        found = true;
                        AddLog($"✓ Found Viewing.html in: {baseDir}");
                        break;
                    }
                    currentDir = currentDir.Parent;
                }
                
                if (!found)
                {
                    // Try current working directory
                    var cwd = Directory.GetCurrentDirectory();
                    if (File.Exists(Path.Combine(cwd, "Viewing.html")))
                    {
                        baseDir = cwd;
                        found = true;
                        AddLog($"✓ Found Viewing.html in current directory: {baseDir}");
                    }
                }
                
                if (!found)
                {
                    var errorMsg = $"Viewing.html not found!\nSearched from: {exeDir}\nPlease ensure Viewing.html is in the Bai4 folder.";
                    AddLog($"✗ {errorMsg}");
                    throw new FileNotFoundException(errorMsg);
                }
                
                AddLog($"Using base directory: {baseDir}");
                
                // Verify other files
                var filesToCheck = new[] { "Viewing.html", "index.html", "app.js" };
                foreach (var file in filesToCheck)
                {
                    var filePath = Path.Combine(baseDir, file);
                    if (File.Exists(filePath))
                    {
                        AddLog($"  ✓ {file}");
                    }
                    else
                    {
                        AddLog($"  ✗ {file} (not found)");
                    }
                }
                
                httpServer = new SimpleHttpServer(HTTP_PORT, baseDir);
                httpServer.Start();

                btnStartServer.Enabled = false;
                btnStopServer.Enabled = true;
                btnOpenBrowser.Enabled = true;
                lblStatus.Text = $"HTTP Server: Running on port {HTTP_PORT}";
                lblStatus.ForeColor = Color.FromArgb(40, 167, 69);

                AddLog($"✓ HTTP Server started on port {HTTP_PORT}");
                AddLog($"  Open http://localhost:{HTTP_PORT}/Viewing.html in your browser");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLog($"Error: {ex.Message}");
            }
        }

        private void BtnStopServer_Click(object? sender, EventArgs e)
        {
            try
            {
                httpServer?.Stop();
                httpServer = null;

                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;
                btnOpenBrowser.Enabled = false;
                lblStatus.Text = "HTTP Server Status: Stopped";
                lblStatus.ForeColor = Color.FromArgb(220, 53, 69);

                AddLog("HTTP Server stopped");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLog($"Error: {ex.Message}");
            }
        }

        private async void BtnCrawlMovies_Click(object? sender, EventArgs e)
        {
            btnCrawlMovies.Enabled = false;
            AddLog("Starting to crawl movies from betacinemas.vn...");
            
            try
            {
                var movies = await scraper.CrawlMoviesAsync();
                AddLog($"Crawled {movies.Count} movies from betacinemas.vn");
                
                await scraper.SaveMoviesToJsonAsync(movies);
                AddLog($"Movies saved to movies.json");
                
                MessageBox.Show($"Successfully crawled {movies.Count} movies!\nMovies saved to movies.json", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AddLog($"Error crawling movies: {ex.Message}");
                MessageBox.Show($"Error crawling movies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCrawlMovies.Enabled = true;
            }
        }

        private void BtnOpenBrowser_Click(object? sender, EventArgs e)
        {
            try
            {
                var url = $"http://localhost:{HTTP_PORT}/Viewing.html";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                AddLog($"Opening browser: {url}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLog($"Error: {ex.Message}");
            }
        }

        public void AddLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(AddLog), message);
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void BtnStartTcpServer_Click(object? sender, EventArgs e)
        {
            try
            {
                tcpServer = new WebTcpServer(TCP_PORT, this);
                tcpServer.Start();

                btnStartTcpServer.Enabled = false;
                btnStopTcpServer.Enabled = true;
                lblTcpStatus.Text = $"TCP Server: Running on port {TCP_PORT}";
                lblTcpStatus.ForeColor = Color.FromArgb(40, 167, 69);

                AddLog($"✓ TCP Server started on port {TCP_PORT}");
                AddLog($"  Web Client can connect to 127.0.0.1:{TCP_PORT}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting TCP server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLog($"✗ Error starting TCP server: {ex.Message}");
            }
        }

        private void BtnStopTcpServer_Click(object? sender, EventArgs e)
        {
            try
            {
                tcpServer?.Stop();
                tcpServer = null;

                btnStartTcpServer.Enabled = true;
                btnStopTcpServer.Enabled = false;
                lblTcpStatus.Text = "TCP Server Status: Stopped";
                lblTcpStatus.ForeColor = Color.FromArgb(220, 53, 69);

                AddLog("TCP Server stopped");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping TCP server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLog($"Error: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            httpServer?.Stop();
            tcpServer?.Stop();
            base.OnFormClosing(e);
        }
    }

    // TCP Server for Web Client connections
    public class WebTcpServer
    {
        private Socket? listenerSocket;
        private Thread? serverThread;
        private bool isListening = false;
        private List<WebClientHandler> clients = new List<WebClientHandler>();
        private CinemaWebDatabase database;
        private int port;
        private WebServerForm form;

        public WebTcpServer(int port, WebServerForm form)
        {
            this.port = port;
            this.form = form;
            database = new CinemaWebDatabase();
        }

        public void Start()
        {
            if (isListening) return;

            isListening = true;
            serverThread = new Thread(StartServer);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        public void Stop()
        {
            isListening = false;

            foreach (var client in clients.ToList())
            {
                client.Close();
            }
            clients.Clear();

            try
            {
                if (listenerSocket != null)
                {
                    listenerSocket.Close();
                }
            }
            catch { }

            form.AddLog("TCP Server stopped");
        }

        private void StartServer()
        {
            try
            {
                listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipepServer = new IPEndPoint(IPAddress.Any, port);

                try
                {
                    listenerSocket.Bind(ipepServer);
                    listenerSocket.Listen(10);
                    form.AddLog($"TCP Server listening on port {port}");
                }
                catch (SocketException se) when (se.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    form.AddLog($"✗ Lỗi: Port {port} đã được sử dụng!");
                    isListening = false;
                    if (form.InvokeRequired)
                    {
                        form.Invoke(new Action(() =>
                        {
                            form.btnStartTcpServer.Enabled = true;
                            form.btnStopTcpServer.Enabled = false;
                        }));
                    }
                    else
                    {
                        form.btnStartTcpServer.Enabled = true;
                        form.btnStopTcpServer.Enabled = false;
                    }
                    return;
                }

                while (isListening)
                {
                    try
                    {
                        Socket clientSocket = listenerSocket.Accept();
                        string clientInfo = clientSocket.RemoteEndPoint?.ToString() ?? "Unknown";
                        form.AddLog($"New TCP client connected: {clientInfo}");

                        WebClientHandler handler = new WebClientHandler(clientSocket, this, database, form);
                        clients.Add(handler);
                        handler.Start();
                    }
                    catch (Exception ex)
                    {
                        if (isListening)
                        {
                            form.AddLog($"Error accepting TCP client: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                form.AddLog($"TCP Server error: {ex.Message}");
                isListening = false;
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(() =>
                    {
                        form.btnStartTcpServer.Enabled = true;
                        form.btnStopTcpServer.Enabled = false;
                    }));
                }
                else
                {
                    form.btnStartTcpServer.Enabled = true;
                    form.btnStopTcpServer.Enabled = false;
                }
            }
        }

        public void RemoveClient(WebClientHandler client)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action<WebClientHandler>(RemoveClient), client);
                return;
            }

            if (clients.Contains(client))
            {
                clients.Remove(client);
                form.AddLog($"TCP Client disconnected. Total clients: {clients.Count}");
            }
        }

        public void BroadcastSeatUpdate(string roomName, List<string> seatNames)
        {
            string message = $"UPDATE_SEATS|{roomName}|{string.Join(",", seatNames)}";
            foreach (var client in clients.ToList())
            {
                try
                {
                    client.SendMessage(message);
                }
                catch
                {
                    // Client đã disconnect
                }
            }
        }
    }

    public class WebClientHandler
    {
        private Socket clientSocket = null!;
        private WebTcpServer server = null!;
        private CinemaWebDatabase database = null!;
        private WebServerForm form = null!;
        private Thread? clientThread;
        private bool isConnected = true;

        public WebClientHandler(Socket socket, WebTcpServer server, CinemaWebDatabase database, WebServerForm form)
        {
            this.clientSocket = socket;
            this.server = server;
            this.database = database;
            this.form = form;
        }

        public void Start()
        {
            clientThread = new Thread(HandleClient);
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        private void HandleClient()
        {
            try
            {
                byte[] buffer = new byte[4096];
                StringBuilder messageBuilder = new StringBuilder();

                while (isConnected && clientSocket.Connected)
                {
                    int bytesReceived = clientSocket.Receive(buffer);
                    if (bytesReceived == 0)
                    {
                        break;
                    }

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    messageBuilder.Append(data);

                    while (messageBuilder.ToString().Contains("\n"))
                    {
                        int newlineIndex = messageBuilder.ToString().IndexOf("\n");
                        string message = messageBuilder.ToString().Substring(0, newlineIndex);
                        messageBuilder.Remove(0, newlineIndex + 1);

                        ProcessMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                form.AddLog($"TCP Client error: {ex.Message}");
            }
            finally
            {
                Close();
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                string[] parts = message.Split('|');
                if (parts.Length == 0) return;

                string command = parts[0];
                form.AddLog($"TCP Received: {command}");

                switch (command)
                {
                    case "GET_MOVIES":
                        SendMovies();
                        break;

                    case "GET_ROOMS":
                        if (parts.Length > 1)
                        {
                            SendRooms(parts[1]);
                        }
                        break;

                    case "GET_SEATS":
                        if (parts.Length > 2)
                        {
                            SendSeats(parts[1], parts[2]);
                        }
                        break;

                    case "BOOK_SEATS":
                        if (parts.Length > 5)
                        {
                            BookSeats(parts[1], parts[2], parts[3], parts[4], parts[5]);
                        }
                        break;

                    default:
                        SendMessage("ERROR|Unknown command");
                        break;
                }
            }
            catch (Exception ex)
            {
                form.AddLog($"Error processing TCP message: {ex.Message}");
                SendMessage($"ERROR|{ex.Message}");
            }
        }

        private void SendMovies()
        {
            var movies = database.GetMovies();
            StringBuilder response = new StringBuilder("MOVIES|");
            foreach (var movie in movies)
            {
                response.Append($"{movie.Name}:{movie.BasePrice};");
            }
            SendMessage(response.ToString());
        }

        private void SendRooms(string movieName)
        {
            var rooms = database.GetRoomsForMovie(movieName);
            StringBuilder response = new StringBuilder("ROOMS|");
            foreach (var room in rooms)
            {
                response.Append($"{room.Name};");
            }
            SendMessage(response.ToString());
        }

        private void SendSeats(string movieName, string roomName)
        {
            var rooms = database.GetRoomsForMovie(movieName);
            var room = rooms.FirstOrDefault(r => r.Name == roomName);
            if (room == null)
            {
                SendMessage("ERROR|Room not found");
                return;
            }

            var seats = database.GetSeatsForRoom(room.Id);
            StringBuilder response = new StringBuilder("SEATS|");
            foreach (var seat in seats.Values)
            {
                response.Append($"{seat.SeatName}:{seat.SeatType}:{(seat.IsBooked ? "1" : "0")};");
            }
            SendMessage(response.ToString());
        }

        private void BookSeats(string customerName, string movieName, string roomName, string seatNamesStr, string totalPriceStr)
        {
            List<string> seatNames = seatNamesStr.Split(',').ToList();
            string errorMessage;

            bool success = database.BookSeats(customerName, movieName, roomName, seatNames, out errorMessage);

            if (success)
            {
                SendMessage("BOOK_SUCCESS|");
                server.BroadcastSeatUpdate(roomName, seatNames);
                form.AddLog($"✓ TCP Booking successful: {customerName} - {movieName} - {roomName} - {string.Join(",", seatNames)}");
            }
            else
            {
                SendMessage($"BOOK_ERROR|{errorMessage}");
                form.AddLog($"✗ TCP Booking failed: {errorMessage}");
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                if (clientSocket.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    clientSocket.Send(data);
                }
            }
            catch (Exception ex)
            {
                form.AddLog($"Error sending TCP message: {ex.Message}");
            }
        }

        public void Close()
        {
            isConnected = false;
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch { }
            server.RemoveClient(this);
        }
    }
}

