using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Bai4
{
    public partial class Bai4Server : Form
    {
        private ListView listViewLog = null!;
        private Button btnListen = null!;
        private Button btnStop = null!;
        private Socket? listenerSocket;
        private Thread? serverThread;
        private bool isListening = false;
        private List<ClientHandler> clients = new List<ClientHandler>();
        private CinemaDatabase database;
        private const int PORT = 8080;

        public Bai4Server()
        {
            database = new CinemaDatabase();
            InitializeComponent();
            btnStop.Enabled = false;
        }

        private void InitializeComponent()
        {
            this.Text = "Bai4 - TCP Server (Quản lý phòng vé)";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            listViewLog = new ListView();
            listViewLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewLog.Location = new Point(12, 12);
            listViewLog.Size = new Size(876, 510);
            listViewLog.View = View.Details;
            listViewLog.FullRowSelect = true;
            listViewLog.GridLines = true;
            listViewLog.Columns.Add("Thời gian", 150);
            listViewLog.Columns.Add("Log", 726);

            btnListen = new Button();
            btnListen.Text = "Listen";
            btnListen.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnListen.Location = new Point(12, 530);
            btnListen.Size = new Size(120, 35);
            btnListen.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnListen.BackColor = Color.FromArgb(40, 167, 69);
            btnListen.ForeColor = Color.White;
            btnListen.FlatStyle = FlatStyle.Flat;
            btnListen.Click += BtnListen_Click;

            btnStop = new Button();
            btnStop.Text = "Stop";
            btnStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnStop.Location = new Point(142, 530);
            btnStop.Size = new Size(120, 35);
            btnStop.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStop.BackColor = Color.FromArgb(220, 53, 69);
            btnStop.ForeColor = Color.White;
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.Click += BtnStop_Click;

            this.Controls.Add(listViewLog);
            this.Controls.Add(btnListen);
            this.Controls.Add(btnStop);

            this.FormClosing += (s, e) =>
            {
                if (isListening)
                {
                    DialogResult result = MessageBox.Show(
                        "Server đang chạy. Bạn có muốn dừng server và đóng cửa sổ?",
                        "Xác nhận đóng Server",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        StopServer();
                    }
                    else
                    {
                        e.Cancel = true; // Hủy đóng form
                    }
                }
            };
        }

        private void BtnListen_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            isListening = true;
            btnListen.Enabled = false;
            btnStop.Enabled = true;

            serverThread = new Thread(StartServer);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void StopServer()
        {
            isListening = false;
            
            // Đóng tất cả clients
            foreach (var client in clients.ToList())
            {
                client.Close();
            }
            clients.Clear();

            // Đóng listener
            try
            {
                if (listenerSocket != null)
                {
                    listenerSocket.Close();
                }
            }
            catch { }

            btnListen.Enabled = true;
            btnStop.Enabled = false;
            AddLog("Server stopped");
        }

        private void StartServer()
        {
            try
            {
                listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipepServer = new IPEndPoint(IPAddress.Any, PORT);
                
                try
                {
                    listenerSocket.Bind(ipepServer);
                    listenerSocket.Listen(10);
                    AddLog($"Server started on port {PORT}");
                    AddLog($"Server IP: {ipepServer.Address} (Listening on all interfaces)");
                }
                catch (SocketException se) when (se.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    AddLog($"Lỗi: Port {PORT} đã được sử dụng bởi Server khác hoặc ứng dụng khác!");
                    AddLog("Vui lòng đóng Server đang chạy hoặc đổi port.");
                    isListening = false;
                    Invoke(new Action(() =>
                    {
                        btnListen.Enabled = true;
                        btnStop.Enabled = false;
                    }));
                    return;
                }

                while (isListening)
                {
                    try
                    {
                        Socket clientSocket = listenerSocket.Accept();
                        string clientInfo = clientSocket.RemoteEndPoint?.ToString() ?? "Unknown";
                        AddLog($"New client connected: {clientInfo}");
                        AddLog($"Total clients: {clients.Count + 1}");

                        ClientHandler handler = new ClientHandler(clientSocket, this, database);
                        clients.Add(handler);
                        handler.Start();
                    }
                    catch (Exception ex)
                    {
                        if (isListening)
                        {
                            AddLog($"Error accepting client: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"Server error: {ex.Message}");
                isListening = false;
                Invoke(new Action(() =>
                {
                    btnListen.Enabled = true;
                    btnStop.Enabled = false;
                }));
            }
        }

        public void AddLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddLog), message);
                return;
            }

            ListViewItem item = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
            item.SubItems.Add(message);
            listViewLog.Items.Insert(0, item);
        }

        public void RemoveClient(ClientHandler client)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<ClientHandler>(RemoveClient), client);
                return;
            }

            if (clients.Contains(client))
            {
                clients.Remove(client);
                AddLog($"Client disconnected. Total clients: {clients.Count}");
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

        public class ClientHandler
        {
        private Socket clientSocket = null!;
        private Bai4Server server = null!;
        private CinemaDatabase database = null!;
        private Thread? clientThread;
        private bool isConnected = true;

        public ClientHandler(Socket socket, Bai4Server server, CinemaDatabase database)
        {
            this.clientSocket = socket;
            this.server = server;
            this.database = database;
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

                    // Xử lý khi nhận được message hoàn chỉnh (kết thúc bằng \n)
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
                server.AddLog($"Client error: {ex.Message}");
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
                server.AddLog($"Received: {command}");

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
                server.AddLog($"Error processing message: {ex.Message}");
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
                // Broadcast update cho tất cả clients
                server.BroadcastSeatUpdate(roomName, seatNames);
                server.AddLog($"Booking successful: {customerName} - {movieName} - {roomName} - {string.Join(",", seatNames)}");
            }
            else
            {
                SendMessage($"BOOK_ERROR|{errorMessage}");
                server.AddLog($"Booking failed: {errorMessage}");
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
                server.AddLog($"Error sending message: {ex.Message}");
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

