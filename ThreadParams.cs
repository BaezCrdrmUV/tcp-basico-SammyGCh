using System.Net.Sockets;

namespace tcp_com
{
    public class ThreadParams
    {
        public Socket Obj { get; set; }
        public int Id { get; set; }

        public ThreadParams(Socket obj, int id)
        {
            this.Obj = obj;
            this.Id = id;
        }
    }
}