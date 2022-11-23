using Newtonsoft.Json;
using System.Net;

namespace Poker.Lib.Http
{
    public class Response
    {
        public bool success;
        public string status;
        public object response;
        protected bool binary;
        protected bool pureResponse = false;
        protected string contentType;
        protected string location;
        private readonly HttpListenerContext context;

        public Response(ref HttpListenerContext context)
        {
            this.success = false;
            this.binary = false;
            this.status = "error";
            this.contentType = "application/json";
            this.response = "Something went wrong.";
            this.context = context;
        }

        public void SetResponse(object data)
        {
            this.response = data;
        }

        public void SetContentType(string data)
        {
            this.contentType = data;
        }

        public void SetLocation(string url)
        {
            this.location = url;
        }

        public void SetStatus(bool success)
        {
            if (success)
            {
                this.success = true;
                this.status = "success";
            }
        }

        public void SendAsOctet()
        {
            this.contentType = "application/octet-stream";
            this.binary = true;
        }

        public void SendPureResponse()
        {
            this.pureResponse = true;
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(pureResponse ? response : this);
        }

        public void Send(HttpStatusCode status = HttpStatusCode.NotImplemented)
        {
            context.Response.StatusCode = (int)status;
            context.Response.KeepAlive = false;

            if (this.location == null)
            {
                context.Response.ContentType = contentType;

                if (this.binary)
                {
                    ((System.IO.MemoryStream)response).WriteTo(context.Response.OutputStream);
                    // context.Response.OutputStream.Write((byte[])response, 0, (int)((System.IO.MemoryStream)response).Length);
                }
                else
                {
                    byte[] output = System.Text.Encoding.UTF8.GetBytes(ToJSON());

                    context.Response.ContentLength64 = output.Length;

                    System.IO.Stream OutputStream = context.Response.OutputStream;
                    OutputStream.Write(output, 0, output.Length);
                }

                if (context.Response.OutputStream.CanRead || context.Response.OutputStream.CanWrite)
                    context.Response.OutputStream.Close();
            }
            else
            {
                context.Response.Redirect(this.location);
                context.Response.Close();
            }
        }
    }
}
