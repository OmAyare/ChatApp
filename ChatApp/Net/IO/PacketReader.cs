using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
            /*.................................................................................................................*/
            var msgLength = ReadInt32(); // Read message length
            byte[] msgBytes = ReadBytes(msgLength); // Read the actual message
            return Encoding.UTF8.GetString(msgBytes); // Convert to string
        }
    }
}
