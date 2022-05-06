using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;

public class SimpleGoDotRpcClient : ClientBase
{
    private readonly Uri _baseUrl;
    private readonly CanvasItem _canvasItem;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public SimpleGoDotRpcClient(Uri baseUrl, CanvasItem canvasItem,
        JsonSerializerSettings jsonSerializerSettings = null)
    {
        if (jsonSerializerSettings == null)
            jsonSerializerSettings = DefaultJsonSerializerSettingsFactory.BuildDefaultJsonSerializerSettings();
        _baseUrl = baseUrl;
        _canvasItem = canvasItem;
        _jsonSerializerSettings = jsonSerializerSettings;
    }


    protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage request, string route = null)
    {
        
        var rpcRequestJson = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
        Error err; 
        var http = new HTTPClient(); // Create the client.

        err = http.ConnectToHost( _baseUrl.Host, _baseUrl.Port, _baseUrl.Scheme == "https"); // Connect to host/port.

        while (http.GetStatus() == HTTPClient.Status.Connecting || http.GetStatus() == HTTPClient.Status.Resolving)
        {
            http.Poll();
            await AwaitResponse();
        }
        string[] headers = new[] { "Content-Type: application/json" };
  
        err = http.Request(HTTPClient.Method.Post, _baseUrl.PathAndQuery, headers, rpcRequestJson); // Request a page from the site.

        // Keep polling for as long as the request is being processed.
        while (http.GetStatus() == HTTPClient.Status.Requesting)
        {
            http.Poll();
            await AwaitResponse();
        }

        // If there is a response...
        if (http.HasResponse())
        {
            
            var rb = new List<byte>();

            while (http.GetStatus() == HTTPClient.Status.Body)
            {
                http.Poll();
                byte[] chunk = http.ReadResponseBodyChunk(); // Read a chunk.
                if (chunk.Length == 0)
                {
                    await AwaitResponse();
                }
                else
                {
                    rb.AddRange(chunk);
                }
            }

            string text = Encoding.UTF8.GetString(rb.ToArray());
                
            var message = JsonConvert.DeserializeObject<RpcResponseMessage>(text, _jsonSerializerSettings);

            return message;
        }

        throw new Exception("Nothing returned");

    }

    private async Task AwaitResponse()
    {
        // If nothing was read, wait for the buffer to fill.
        if (OS.HasFeature("web"))
        {
            // Synchronous HTTP requests are not supported on the web,
            // so wait for the next main loop iteration.
            await _canvasItem.ToSignal(Engine.GetMainLoop(), "idle_frame");
        }
        else
        {
            await Task.Delay(100);
        }
    }
}
