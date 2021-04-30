using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace tcp_com
{
    public class TCPClient
    {
        TcpClient client;
        string IP;
        int Port;
        string Username;

        string serverMessage;

        public TCPClient(string ip, int port, string username)
        {
            try
            {
                client = new TcpClient();
                this.IP = ip;
                this.Port = port;
                this.Username = username;
            }
            catch (System.Exception)
            {
                
            }
        }

        public void Chat()
        {
            client.Connect(IP, Port);   
            Console.WriteLine("Conectado\n");
            serverMessage = "";
            int idMensaje = 1;

            MostrarOpcionesDeServer();

            while(true)
            {     
                try
                {
                    string msg = Console.ReadLine();

                    Message newMessage = new Message 
                    {
                        Id = idMensaje ++,
                        MessageString = msg,
                        User = Username,
                        Hora = DateTime.Now
                    };

                    if (msg.StartsWith("read"))
                    {
                        newMessage.Tipo = TipoMensaje.Read;
                    }
                    else if (msg.StartsWith("update"))
                    {
                        newMessage.Tipo = TipoMensaje.Update;
                    }
                    else if (msg.StartsWith("delete"))
                    {
                        newMessage.Tipo = TipoMensaje.Delete;
                    }
                    else
                    {
                        newMessage.Tipo = TipoMensaje.Crear;
                    }

                    string jsonMessage = JsonConvert.SerializeObject(newMessage);

                    // Envío de datos
                    var stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                    Console.WriteLine("Enviando datos...");
                    stream.Write(data, 0, data.Length);

                    byte[] package = new byte[1024];

                    if (msg.Equals("bye"))
                        break;
                    else if (newMessage.Tipo == TipoMensaje.Read)
                    {
                        stream.Read(package);
                        serverMessage = Encoding.UTF8.GetString(package);
                        List<Message> misMensajes = JsonConvert.DeserializeObject<List<Message>>(serverMessage);

                        MostrarMisMensajes(misMensajes);

                        MostrarOpcionesDeServer();
                    }
                    else if (newMessage.Tipo == TipoMensaje.Update || newMessage.Tipo == TipoMensaje.Delete)
                    {
                        stream.Read(package);
                        serverMessage = Encoding.UTF8.GetString(package);
                        Console.WriteLine(serverMessage);

                        MostrarOpcionesDeServer();
                    }
                    else
                    {
                        // Recepción de mensajes
                        stream.Read(package);
                        serverMessage = Encoding.UTF8.GetString(package);
                        Console.WriteLine(serverMessage);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error {0}", ex.Message);
                    break;
                }
            }
            Console.WriteLine("Desconectado");
            return;
        }

        private void MostrarOpcionesDeServer()
        {
            Console.WriteLine("\n--OPCIONES--\n-Envía \"read\" para consultar tus mensajes" +
                            "\n-Envía \"update id_mensaje nuevo_mensaje\" para actualizar un mensaje con el id específicado" +
                            "\n-Envía \"delete id_mensaje\" para eliminar un mensaje con el id específicado" +
                            "\n-Envía \"bye\" para terminar\n---------------------");
        }

        private void MostrarMisMensajes(List<Message> misMensajes)
        {
            if (misMensajes != null && misMensajes.Count > 0)
            {
                Console.WriteLine("---Mis mensajes enviados---");
                misMensajes.ForEach(mensaje =>
                    Console.WriteLine($"ID: {mensaje.Id} | Mensaje: {mensaje.MessageString} | Hora: {mensaje.Hora.ToString("HH:mm")}")
                );
            }
            else
            {
                Console.WriteLine("---No has enviado ningún mensaje aún---");
            }
        }
    }
}