using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Bai4
{
    // Simple TCP proxy - creates new connection for each request
    public static class TcpProxyManager
    {
        public static string SendMessage(string serverIP, int serverPort, string message)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(serverIP, serverPort);
                    client.ReceiveTimeout = 5000;
                    client.SendTimeout = 5000;

                    using (var stream = client.GetStream())
                    {
                        // Send message
                        byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                        stream.Write(data, 0, data.Length);
                        stream.Flush();

                        // Read response with timeout
                        stream.ReadTimeout = 5000;
                        
                        // Read complete response
                        var responseBytes = new List<byte>();
                        var buffer = new byte[4096];
                        int totalBytesRead = 0;
                        
                        do
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;
                            
                            for (int i = 0; i < bytesRead; i++)
                            {
                                if (buffer[i] != 0) // Skip null bytes
                                {
                                    responseBytes.Add(buffer[i]);
                                }
                            }
                            totalBytesRead += bytesRead;
                        } while (stream.DataAvailable && totalBytesRead < 8192); // Max 8KB
                        
                        if (responseBytes.Count == 0)
                        {
                            return "ERROR|Không nhận được phản hồi từ server";
                        }
                        
                        string response = Encoding.UTF8.GetString(responseBytes.ToArray()).Trim();
                        
                        // Clean up response - remove any remaining null chars and control chars
                        response = new string(response.Where(c => c != '\0' && !char.IsControl(c) || c == '\n' || c == '\r').ToArray()).Trim();
                        
                        return response;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"TCP Connection error: {ex.Message}");
                return $"ERROR|Không thể kết nối đến server: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP Send error: {ex.Message}");
                return $"ERROR|{ex.Message}";
            }
        }
    }
}
