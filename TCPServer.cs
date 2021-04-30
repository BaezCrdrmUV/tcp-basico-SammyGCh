using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace tcp_com
{
    public class TCPServer
    {
        public TcpListener listener { get; set; }
        public bool acceptFlag { get; set; }

        public List<Message> Mensajes { get; set; }
        public List<int> IdHilos { get; set; }

        public bool _tieneHilosAbiertos;

        public TCPServer(string ip, int port, bool start = false)
        {
            Mensajes = new List<Message>();
            IdHilos = new List<int>();
            _tieneHilosAbiertos = false;

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
                int id = 0;

                Thread watch = new Thread(new ThreadStart(VerificarHilosAbiertos));

                watch.Start();
                while(true)
                {
                    Console.WriteLine("Esperando conexión del cliente...");
                    if (_tieneHilosAbiertos && IdHilos.Count == 0)
                        break;
                    
                    try
                    {
                        var clientSocket = listener.AcceptSocket();
                        Console.WriteLine("Cliente aceptado");

                        Thread hilo = new Thread(new ParameterizedThreadStart(ControlarComunicacion));

                        hilo.Start(new ThreadParams(clientSocket, id));
                        IdHilos.Add(id);
                        id++;
                        _tieneHilosAbiertos = true;
                    }
                    catch (System.Exception)
                    {
                        
                        
                    }

                    
                }

                watch.Interrupt();

                return;
            }
        }

        public void ControlarComunicacion(Object obj)
        {
            ThreadParams param = (ThreadParams)obj;
            Socket client = param.Obj;

            if (client != null)
            {
                Console.WriteLine("Cliente conectado. Esperando datos");
                string msg = "";
                byte[] data;

                Message mensaje = new Message();

                while (mensaje != null && !mensaje.MessageString.Equals("bye"))
                {
                    try
                    {

                        if (mensaje.Tipo == TipoMensaje.Crear)
                        {
                            Mensajes.Add(mensaje);
                            MostrarMensaje(mensaje);

                            // Enviar un mensaje al cliente
                            data = Encoding.UTF8.GetBytes("\n--OPCIONES--\n-Envía \"read\" para consultar tus mensajes" +
                            "\n-Envía \"update id_mensaje nuevo_mensaje\" para actualizar un mensaje con el id específicado" +
                            "\n-Envía \"delete id_mensaje\" para eliminar un mensaje con el id específicado" +
                            "\n-Envía \"bye\" para terminar\n---------------------");
                            client.Send(data);
                        }
                        else if (mensaje.Tipo == TipoMensaje.Read)
                        {
                            string mensajesCliente = ObtenerMensajes(mensaje.User);
                            data = Encoding.UTF8.GetBytes(mensajesCliente);
                            client.Send(data);
                        }
                        else if (mensaje.Tipo == TipoMensaje.Update)
                        {
                            string respuesta = "";
                            string[] updateInfo = mensaje.MessageString.Split(" ");
                            string idMensaje = "";
                            int idMensajeEntero = 0;

                            try
                            {
                                idMensaje = updateInfo[1];
                                idMensajeEntero = int.Parse(idMensaje);
                            }
                            catch (System.Exception ex)
                            {
                                respuesta = "El id no fue específicado correctamente";
                                data = Encoding.UTF8.GetBytes(respuesta);
                                client.Send(data);

                                Console.WriteLine(ex.Message);
                            }

                            string mensajeActualizado = "";                            

                            try
                            {
                                int startIndex = 8 + idMensaje.Length;
                                int largoMensajeNuevo = mensaje.MessageString.Length - startIndex;
                                mensajeActualizado = mensaje.MessageString.Substring(startIndex, largoMensajeNuevo);
                            }
                            catch (System.Exception)
                            {
                                respuesta = "El nuevo mensaje no fue específicado correctamente";
                                data = Encoding.UTF8.GetBytes(respuesta);
                                client.Send(data);
                            }

                            if (ExisteMensaje(idMensajeEntero)) 
                            {
                                ActualizarMensaje(idMensajeEntero, mensajeActualizado);

                                respuesta = "---Mensaje actualizado---";
                                data = Encoding.UTF8.GetBytes(respuesta);
                                client.Send(data);
                            }
                            else
                            {
                                respuesta = "---No existe el mensaje con el id específicado---";
                                data = Encoding.UTF8.GetBytes(respuesta);
                                client.Send(data);
                            }
                        }
                        else if (mensaje.Tipo == TipoMensaje.Delete)
                        {
                            string respuesta = "";
                            string[] updateInfo = mensaje.MessageString.Split(" ");
                            string idMensaje = "";
                            int idMensajeEntero = 0;

                            try
                            {
                                idMensaje = updateInfo[1];
                                idMensajeEntero = int.Parse(idMensaje);
                            }
                            catch (System.Exception ex)
                            {
                                respuesta = "El id no fue específicado correctamente";
                                data = Encoding.UTF8.GetBytes(respuesta);
                                client.Send(data);

                                Console.WriteLine(ex.Message);
                            }

                            if (ExisteMensaje(idMensajeEntero))
                            {
                                if (EliminarMensaje(idMensajeEntero))
                                {
                                    respuesta = "---Mensaje eliminado---";
                                }
                                else
                                {
                                    respuesta = "---El mensaje no pudo ser eliminado. Intente más tarde.---";
                                }

                                
                                data = Encoding.UTF8.GetBytes(respuesta);
                                client.Send(data);
                            }
                            else
                            {
                                respuesta = "--No existe mensaje con el id específicado--";
                                data = Encoding.UTF8.GetBytes(respuesta);
                                client.Send(data);
                            }
                        }

                        

                        // Escucha por nuevos mensajes
                        byte[] buffer = new byte[1024];
                        client.Receive(buffer);

                        msg = Encoding.UTF8.GetString(buffer);

                        mensaje = JsonConvert.DeserializeObject<Message>(msg);
                        
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        break;
                    }

                }

                Console.WriteLine("Cerrando conexión");
                client.Dispose();

                IdHilos.Remove(param.Id);

                Thread.CurrentThread.Join();
            }
        }

        private bool EliminarMensaje(int idMensaje)
        {
            return Mensajes.RemoveAll(mensaje => mensaje.Id == idMensaje) > 0;
        }

        private void ActualizarMensaje(int idMensaje, string mensajeActualizado)
        {
            Message mensajeAActualizar = Mensajes.FirstOrDefault(mensaje => mensaje.Id == idMensaje);

            mensajeAActualizar.MessageString = mensajeActualizado;
        }

        private bool ExisteMensaje(int idMensaje)
        {
            return Mensajes.Any(mensaje => mensaje.Id == idMensaje);
        }

        private string ObtenerMensajes(string user)
        {
            List<Message> mensajesDeCliente = Mensajes.Where(mensaje => mensaje.User.Equals(user)).ToList();

            string mensajesJson = JsonConvert.SerializeObject(mensajesDeCliente);

            return mensajesJson;
        }

        private void VerificarHilosAbiertos()
        {
            while (true)
            {
                if (_tieneHilosAbiertos && IdHilos.Count == 0)
                {
                    listener.Stop();
                    listener = null;

                    break;
                }
            }

            MostrarMensajes();

            Thread.CurrentThread.Join();
        }

        public void MostrarMensajes()
        {
            Console.WriteLine("\nMensajes en la colección");

            foreach (Message mensaje in Mensajes)
            {
                Console.WriteLine($"{mensaje.User} ({mensaje.Hora.ToString("HH:mm")}) >> {mensaje.MessageString}");
            }
        }

        private void MostrarMensaje(Message mensaje)
        {
            Console.WriteLine($"{mensaje.User} ({mensaje.Hora.ToString("HH:mm")}) > {mensaje.MessageString}");
        }
    }
}