using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ServerTcp : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Header;
    private TcpListener listener;

    public int Port = 1608;

    private Task<TcpClient> nextClient;

    // Start is called before the first frame update
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        listener = new TcpListener(IPAddress.Any, Port);

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
                var url = $"http://{ip4Address}:{Port}";
                b.Append($"<b>{url}<b>\n");
            }

            header = b.ToString();
        }

        Header.text = header;

        listener.Start();
        Poll();
    }

    private void Poll()
    {
        if (nextClient == null)
            nextClient = listener.AcceptTcpClientAsync();
        else if (nextClient.IsCompleted)
        {
            ProcessRequest(nextClient.Result);
            nextClient = listener.AcceptTcpClientAsync();
        }
    }

    private void ProcessRequest(TcpClient client)
    {
        Debug.Log("Got request");
        var inStream = new StreamReader(client.GetStream());
        var outStream = new StreamWriter(client.GetStream());
        string line;
        var url = "";
        while ((line = inStream.ReadLine()) != null)
        {
            if (line == "")
                break;

            Debug.Log(line);
            if (line.StartsWith("GET"))
            {
                var slash = line.IndexOf('/');
                var request = line.Substring(slash);
                var space = request.IndexOf(' ');
                url = space < 0 ? request : request.Substring(0, space);
            }
        }
        // Construct a response.
        //var response = context.Response;
        //string responseString = $"<HTML><BODY>{Response(context.Request.Url.AbsolutePath)}</BODY></HTML>";
        //byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        //// Get a response stream and write the response to it.
        //response.ContentLength64 = buffer.Length;
        //System.IO.Stream output = response.OutputStream;
        //output.Write(buffer,0,buffer.Length);
        //// You must close the output stream.
        //output.Close();
        Debug.Log(url);
        outStream.WriteLine(Response(url));
        outStream.Flush();
        inStream.Close();
        outStream.Close();
        client.Close();
    }

    private static string Response(string path)
    {
        Debug.Log(path);
        return Generator.Current.Solve().Description(Generator.Current.Solve().Individuals[0],"<b>", "</b>");
    }

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        Poll();
    }

    private void OnDestroy()
    {
        listener.Stop();
    }
}
