using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Net.IO;
using System.IO;
using ClosedXML.Excel;

namespace ChatApp.Net
{
    public class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectEvent;

        public Server()
        {
            _client = new TcpClient();
        }
        public void ConnectToServer(string username)
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 5001);
                PacketReader = new PacketReader(_client.GetStream());

                if (!string.IsNullOrEmpty(username))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpcode(0);
                    connectPacket.WriteMessage(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPackets();
            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var opcode = PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 5:
                            msgReceivedEvent?.Invoke();

                            break;
                        case 10:
                            userDisconnectEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("Connected...");
                            break;
                    }

                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpcode(5);
            messagePacket.WriteMessage(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        public async Task SendExcelDataToServer(string filePath)
        {
            var rows = ReadExcelData(filePath);
            int currentRowIndex = 0; // Track the current row being sent

            while (currentRowIndex < rows.Count)
            {
                var currentRow = rows[currentRowIndex];
                var formattedData = FormatData(currentRow);
                bool ackReceived = false;

                // Retry sending the current row until acknowledgment is received
                while (!ackReceived)
                {
                    // Send the current row to the server
                    SendMessageToServer(formattedData);
                    Console.WriteLine($"Sending row {currentRowIndex + 1}: {formattedData}");

                    // Wait for acknowledgment
                    ackReceived = await WaitForAcknowledgmentAsync();

                    // If acknowledgment not received, log it for debugging
                    if (!ackReceived)
                    {
                        Console.WriteLine($"Acknowledgment not received for row {currentRowIndex + 1}, retrying...");
                    }
                }

                // Once acknowledgment is received, move to the next row
                Console.WriteLine($"Row {currentRowIndex + 1} sent successfully!");
                currentRowIndex++; // Move to the next row
            }

            Console.WriteLine("All rows have been sent successfully.");
        }

        private List<Dictionary<string, string>> ReadExcelData(string filePath)
        {
            var data = new List<Dictionary<string, string>>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    var rowData = new Dictionary<string, string>
                    {
                        { "Name", row.Cell(1).GetString() },
                        { "Age", row.Cell(2).GetString() },
                        { "Gender", row.Cell(3).GetString() },
                        { "City", row.Cell(4).GetString() }
                    };

                    data.Add(rowData);
                }
            }

            return data;
        }

        private string FormatData(Dictionary<string, string> row)
        {

            return $"Name={row["Name"]};Age={row["Age"]};Gender={row["Gender"]};City={row["City"]}";
        }


        public event Action<string> acknowledgmentReceivedEvent;

        private async Task<bool> WaitForAcknowledgmentAsync()
        {
            while (true)
            {
                var acknowledgment = await Task.Run(() => PacketReader.ReadMessage());

                Console.WriteLine($"Received acknowledgment: {acknowledgment}");

                // Trigger the acknowledgment event so that the UI can be updated
                acknowledgmentReceivedEvent?.Invoke(acknowledgment);

                // Modify this condition to accept any acknowledgment format you desire
                if (acknowledgment.StartsWith("ACK") && acknowledgment.EndsWith("\r\n"))
                {
                    Console.WriteLine("ACK received, proceeding to next row.");
                    return true; // Acknowledgment is valid, return true
                }
                else if (acknowledgment.StartsWith("CK")) // Handle different acknowledgment
                {
                    Console.WriteLine("Received alternative acknowledgment (CK), proceeding to next row.");
                    return true; // Proceed even if it’s CK
                }

                // Log unexpected acknowledgment for debugging
                Console.WriteLine("Unexpected acknowledgment received, retrying...");
                await Task.Delay(50); // Wait a bit before retrying
            }
        }



    }
}