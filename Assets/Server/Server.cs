#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Server.cs" company="Ian Horswill">
// Copyright (C) 2019, 2020 Ian Horswill
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Imaginarium.Generator;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Server : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Header;
    private HttpListener listener;
    private TcpListener tcpListener;

    private Task<HttpListenerContext> nextRequest;

    // Start is called before the first frame update
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        listener = new HttpListener();

        var header = "Unknown IP address";
        var hostname = Dns.GetHostName();
        var hostAddresses = Dns.GetHostAddresses(hostname);
        if (hostAddresses.Length > 0)
        {
            var b = new StringBuilder();
            b.Append("Connect to ");
            var firstOne = true;
            foreach (var address in hostAddresses)
            {
                if (address.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                if (firstOne)
                    firstOne = false;
                else 
                    b.Append("Or: ");

                var ip4Address = address.MapToIPv4().ToString();
                var url = $"http://{ip4Address}:1608";
                b.Append($"<b>{url}<b>\n");
            }

            header = b.ToString();
        }

        Header.text = header;

        if (!HttpListener.IsSupported)
            Debug.Log("Http listeners are not supported on this OS.");
        listener.Prefixes.Add("http://*:1608/");
        listener.Start();
        Poll();
    }

    private void Poll()
    {
        if (nextRequest == null)
            nextRequest = listener.GetContextAsync();
        else if (nextRequest.IsCompleted)
        {
            ProcessRequest(nextRequest.Result);
            nextRequest = listener.GetContextAsync();
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        Debug.Log("Got request");
        // Construct a response.
        var response = context.Response;
        string responseString = $"<HTML><BODY>{Response(context.Request.Url.AbsolutePath)}</BODY></HTML>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer,0,buffer.Length);
        // You must close the output stream.
        output.Close();
    }

    private static string Response(string path)
    {
        Debug.Log(path);
        return Generator.Current.Generate().Description(Generator.Current.Generate().Individuals[0],"<b>", "</b>");
    }

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        Poll();
    }
}
