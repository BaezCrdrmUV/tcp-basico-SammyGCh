using System;

namespace tcp_com
{
    public class Message
    {
        public int Id { get; set; }
        public string MessageString { get; set; }
        public string User { get; set; }

        public DateTime Hora { get; set; }

        public TipoMensaje Tipo { get; set; }

        public Message()
        {
            MessageString = "";
            User = "Default";
        }

        public Message(string messageString, string user)
        {
            this.MessageString = messageString;
            this.User = user;
        }
    }
}