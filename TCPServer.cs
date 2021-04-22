using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace tcp_com
{
    public class TCPServer
    {
        public TcpListener listener { get; set; }
        public bool acceptFlag { get; set; }

        public TCPServer(string ip, int port, bool start = false)
        {
            IPAddress address = IPAddress.Parse(ip);
            this.listener = new TcpListener(address, port);

            if(start == true)
            {
                listener.Start();
                Console.WriteLine("Servidor iniciado en la dirección {0}:{1}",
                    address.MapToIPv4().ToString(), port.ToString());
                acceptFlag = true;
            }
        }

        public void Listen()
        {
            if(listener != null && acceptFlag == true)
            {
                while(true)
                {
                    Console.WriteLine("Esperando conexión del cliente...");
                    // Empieza a escuchar
                    var clientTask = listener.AcceptSocketAsync();

                    if(clientTask.Result != null)
                    {
                        Console.WriteLine("Cliente conectado. Esperando datos");
                        var client = clientTask.Result;
                        string msg = "";
                        byte[] data;

                        while(msg != null && !msg.StartsWith("bye"))
                        {
                            // Enviar un mensaje al cliente
                            data = Encoding.UTF8.GetBytes("Envía datos. Envía \"bye\" para terminar");
                            client.Send(data);

                            // Escucha por nuevos mensajes
                            byte[] buffer = new byte[1024];
                            client.Receive(buffer);

                            msg = Encoding.UTF8.GetString(buffer);

                            Message mensaje = JsonConvert.DeserializeObject<Message>(msg);
                            msg = mensaje.MessageString;

                            Console.WriteLine($"{mensaje.User} ({mensaje.Hora.ToString("HH:mm")}) > {mensaje.MessageString}");
                        }

                        data = Encoding.UTF8.GetBytes("Chat terminado");
                        client.Send(data);

                        Console.WriteLine("Cerrando conexión");
                        client.Dispose();
                    }
                }
            }
        }
    }
}