using System;

namespace tcp_com
{
    class Program
    {
        static void Main(string[] args)
        {
            if(String.IsNullOrEmpty(args[0]) || args[0] == "client")
            {
                string name = "Cliente";
                try
                {
                    name = args[1];
                }
                catch (Exception) { }

                // Ejecución de cliente
                TCPClient client = new TCPClient("127.0.0.1", 8090, name);
                client.Chat();
            }
            else if(args[0] == "server")
            {
                TCPServer server = new TCPServer("127.0.0.1", 8090, true);
                server.Listen();
            }
        }
    }
}
