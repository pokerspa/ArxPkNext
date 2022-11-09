using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Collections.Generic;

namespace Poker.Lib.Http
{
    internal class Part
    {
        private Match match;
        public string Name { get; private set; }
        public string FileName { get; private set; }
        public string Type { get; private set; }
        public string Contents { get; private set; }

        public Part(string source) => this.ParseBoundary(source);

        public string GetData(int i) => this.match.Groups[i].Value;

        private void ParseBoundary(string searchText)
        {
            this.match = Regex.Match(searchText, @"(Content-Disposition: (.+)\s+)?(Content-Length: (.+)\s+)?(Content-Type: (.+))?\s+");

            if (this.match.Success)
            {
                string[] ContentDisposition = Regex.Split(this.GetData(1).Trim(), @";\s?");

                // Parse Content-Disposition params
                foreach (string part in ContentDisposition)
                {
                    string[] kv = Regex.Split(part, "=");

                    // Not a key-value pair
                    if (kv.Length < 2) continue;

                    // Trim quotes from value
                    kv[1] = kv[1].TrimStart('"').TrimEnd('"');

                    switch (kv[0])
                    {
                        case "name":
                            Name = kv[1];
                            break;
                        case "filename":
                            FileName = kv[1];
                            break;
                    }
                }

                this.Type = this.GetData(3).Trim();

                // Read contents from Content-Disposition end to the end of the searchText
                this.Contents = searchText.Substring(this.match.Captures[0].Length).TrimEnd("\r\n--".ToCharArray());
            }
        }
    }

    public class MultipartParser
    {
        public bool Success { get; private set; }
        public string ContentType { get; private set; }
        public string FileContents { get; private set; }
        public string boundary;
        public Dictionary<string, string> fields = new Dictionary<string, string>() { };

        public MultipartParser(HttpListenerRequest request)
        {
            this.FindBoundary(request.ContentType);
            this.Parse(request.InputStream, Encoding.UTF8);
        }

        public MultipartParser(HttpListenerRequest request, Encoding encoding)
        {
            this.FindBoundary(request.ContentType);
            this.Parse(request.InputStream, encoding);
        }

        public MultipartParser(string contentType, Stream body)
        {
            this.FindBoundary(contentType);
            this.Parse(body, Encoding.UTF8);
        }

        private void FindBoundary(string contentType)
        {
            if (contentType == null) return;

            try
            {
                var boundary = contentType.Split(';').LastOrDefault().Trim();
                this.boundary = Regex.Split(boundary, "boundary=").LastOrDefault().Trim();
            }
            catch (Exception)
            {
                this.boundary = null;
            }
        }

        private void Parse(Stream stream, Encoding encoding)
        {
            this.Success = false;

            if (this.boundary == null) return;

            // Read the stream into a byte array
            byte[] data = ToByteArray(stream);

            // Copy to a string for header parsing
            string content = encoding.GetString(data);

            // Clean boundary blocks and create parts
            var blocks = Regex.Split(content, boundary)
                .Select(e => e.Trim())
                .Where(e => Regex.Replace(e, "--", "").Length > 0)
                .Select(e => new Part(e))
                .ToArray();

            // Read parts and fill array by type
            foreach (Part block in blocks)
            {
                try
                {
                    fields.Add(block.Name, block.Contents);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        private byte[] ToByteArray(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public static byte[] StringToByteArray(string chars)
        {
            return chars.ToCharArray().Select(c => (byte)c).ToArray();
        }
    }
}
