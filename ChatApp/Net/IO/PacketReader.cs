using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Net.IO
{
    public class PacketReader : BinaryReader
    {
        private NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }
        public string ReadMessage()
        {
            //StringBuilder message = new StringBuilder();
            //char c;

            //while ((c = (char)_ns.ReadByte()) != '\n')  // Read until newline
            //{
            //    if (c != '\r') // Ignore carriage return '\r'
            //    {
            //        message.Append(c);
            //    }
            //}

            //return message.ToString();

            byte[] buffer = new byte[1024];
            int read = _ns.Read(buffer, 0, buffer.Length);
            string response = string.Empty;
            if (read > 0)
            {
                response = Encoding.ASCII.GetString(buffer, 0, read);
            }
            return response;

        }

    }
}
