using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
        return Generator.Current.Solve().Description(Generator.Current.Solve().Individuals[0],"<b>", "</b>");
    }

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        Poll();
    }
}
