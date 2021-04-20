namespace tcp_com
{
    public class Message
    {
        public string MessageString { get; set; }
        public string User { get; set; }

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