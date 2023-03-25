using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace SkinSniper.Services.Skinport.Http
{
    public class HttpsHandler
    {
        private readonly TcpClient _client;
        private readonly SslStream _stream;
        private readonly Uri _uri;
        private readonly WebHeaderCollection _header;

        public HttpsHandler(string domain, string cookie)
        {
            _uri = new Uri(domain);
            _client = new TcpClient(_uri.Host, 443);

            _header = new WebHeaderCollection();
            //_header["Origin"] = _uri.Scheme + "://" + _uri.Host;
            _header[HttpRequestHeader.Host] = _uri.Host;
            _header[HttpRequestHeader.Referer] = _uri.Scheme + "://" + _uri.Host + "/market";
            _header[HttpRequestHeader.Accept] = "application/json, text/plain, */*";
            _header[HttpRequestHeader.AcceptEncoding] = "identity";//"gzip, deflate, br";
            _header[HttpRequestHeader.AcceptLanguage] = "en-GB,en-US;q=0.9,en;q=0.8";
            _header[HttpRequestHeader.Connection] = "keep-alive";
            _header[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36";
            _header[HttpRequestHeader.Cookie] = cookie;

            _stream = new SslStream(_client.GetStream());
            _stream.AuthenticateAsClient(_uri.Host);
        }

        public Task<string> SendPostAsync(string endpoint, string content)
        {
            return Task.Run(() =>
            {
                return SendPost(endpoint, content);
            });
        }

        public string SendPost(string endpoint, string content)
        {
            // skinport posts all their data with encoded form data
            _header[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            _header[HttpRequestHeader.ContentLength] = content.Length.ToString();

            var request = "POST " + endpoint + " HTTP/1.0\r\n" + _header + content;
            var bytes = Encoding.ASCII.GetBytes(request);

            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();

            var builder = new StringBuilder();
            var buffer = new byte[4096];
            var length = _stream.Read(buffer, 0, buffer.Length);

            Console.WriteLine(request);
            Console.WriteLine(length);

            // skinport returns json objects, so we can expect every response from them to end with 0x7D
            builder.Append(Encoding.ASCII.GetString(buffer, 0, length));
            Console.WriteLine(builder.ToString());

            while (builder.ToString().Last() != '\x7D')
            {
                length = _stream.Read(buffer, 0, buffer.Length);
                builder.Append(Encoding.ASCII.GetString(buffer, 0, length));
            }

            return builder.ToString();
        }
    }
}

