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
        //var me = IPAddress.Any;

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

                b.Append($"<b>{$"http://{address}:{Port}"}<b>\n");
            }

            header = b.ToString();
        }

        Header.text = header;

        listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Poll();
    }

    private Thread currentThread;

    private bool ThreadRunning => currentThread != null && currentThread.IsAlive;

    private void Poll()
    {
        if (currentThread != null && !currentThread.IsAlive)
        {
            Debug.Log("completed");
            currentThread = null;
        }

        if (!ThreadRunning && listener.Pending())
        {
            var client = listener.AcceptTcpClient();
            Debug.Log($"Connect {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            currentThread = new Thread(() => ProcessRequest(client));
            currentThread.Start();
        }
    }

    private void ProcessRequest(TcpClient client)
    {
        //Debug.Log("Got request");
        var inStream = new StreamReader(client.GetStream());
        var outStream = new StreamWriter(client.GetStream(), Encoding.UTF8);
        string line;
        var url = "";
        while ((line = inStream.ReadLine()) != null)
        {
            if (line == "")
                break;

            //Debug.Log(line);
            if (line.StartsWith("GET"))
            {
                var slash = line.IndexOf('/');
                var request = line.Substring(slash);
                var space = request.IndexOf(' ');
                url = space < 0 ? request : request.Substring(0, space);
            }
        }

        //Debug.Log(url);

        // Generate response
        var response = Response(url);
        var bits = Encoding.UTF8.GetBytes(response);

        // Write headers
        outStream.Write("HTTP/1.1 200 OK\r\n");
        outStream.Write("Connection: close\r\n");
        //outStream.Write("Transfer-Encoding: identity\r\n");
        outStream.Write("Content-Type: text/html\r\n");
        //outStream.Write($"Content-Length: {bits.Length}\r\n");
        outStream.Write("\r\n");

        // Write content
        //client.GetStream().Write(bits, 0, bits.Length);
        //client.GetStream().Flush();
        //Thread.Sleep(5000);
        outStream.Write(response);
        outStream.Flush();

        // Close everything
        //inStream.Close();
        //outStream.Close();
        client.Close();
    }

    private static string Response(string path)
    {
        //Debug.Log(path);
        if (Generator.Current == null)
            return "<HTML><body>No generator selected</body></html>";

        var invention = Generator.Current.Solve();
        var buffer = new StringBuilder();
        buffer.Append("<html><body><ul>");

        foreach (var i in invention.Individuals)
        {
            buffer.Append("<li>");
            buffer.Append(invention.Description(i,"<b>", "</b>"));
            buffer.Append("</li>");
        }

        buffer.Append("</ul></body></html>");
        return buffer.ToString();
    }

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        Poll();
    }

    private void OnDestroy()
    {
        listener.Stop();
        if (ThreadRunning)
            currentThread.Abort();
    }
}
