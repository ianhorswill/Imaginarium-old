using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;

public class Server : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Header;
    private HttpListener listener;

    private Task<HttpListenerContext> nextRequest;

    // Start is called before the first frame update
    void Start()
    {
        if (Generator.Current == null)
        {
            Header.text = "Please select a generator before starting server";
            return;
        }

        var header = "Unknown IP address";
        var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
        if (hostAddresses.Length > 0)
        {
            var b = new StringBuilder();
            b.Append("Connect to ");
            var firstOne = true;
            foreach (var address in hostAddresses)
            {
                if (firstOne)
                    firstOne = false;
                else 
                    b.Append("Or: ");

                var addressString = address.MapToIPv4().ToString();
                b.Append($"<b>http://{addressString}:1608<b>\n");
            }

            header = b.ToString();
        }

        Header.text = header;

        if (!HttpListener.IsSupported)
            Debug.Log("Http listeners are not supported on this OS.");

        listener = new HttpListener();
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
        var invention = Generator.Current.Solve();
        string responseString = $"<HTML><BODY>{invention.Description(invention.Individuals[0],"<b>", "</b>")}</BODY></HTML>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer,0,buffer.Length);
        // You must close the output stream.
        output.Close();
    }

    void Update()
    {
        Poll();
    }
}
