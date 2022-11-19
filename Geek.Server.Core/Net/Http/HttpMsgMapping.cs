namespace Geek.Server.Core.Net.Http
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpMsgMapping : Attribute
    {
        public string Cmd;
        public HttpMsgMapping(string cmd)
        {
            this.Cmd = cmd;
        }
    }
}
