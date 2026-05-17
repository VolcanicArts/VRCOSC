// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.Web;

public abstract class HttpNode(HttpMethod method) : TryActionAsyncNode
{
    private readonly HttpClient client = new();

    public ValueInput<string> URL = new();
    public ValueInput<Dictionary<string, string>> Headers = new();
    public ValueInput<TimeSpan> Timeout = new(defaultValue: TimeSpan.FromMilliseconds(1000));
    public ValueOutput<HttpStatusCode> StatusCode = new();
    public ValueOutput<string> ErrorMessage = new();
    public ValueOutput<Dictionary<string, string>> ResponseHeaders = new("Headers");

    protected override async Task<bool> TryActionAsync(IPulseContext c)
    {
        var url = URL.Read(c);
        var headers = Headers.Read(c);

        if (string.IsNullOrEmpty(url)) return false;

        headers ??= new Dictionary<string, string>();

        try
        {
            using var request = new HttpRequestMessage(method, new Uri(url));
            await ModifyRequest(request, c);

            request.Headers.Add("User-Agent", AppManager.APP_NAME);

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

            var response = await c.Run(client.SendAsync(request).WaitAsync(Timeout.Read(c)));
            StatusCode.Write(response.StatusCode, c);

            var responseHeaders = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
            ResponseHeaders.Write(responseHeaders, c);

            response.EnsureSuccessStatusCode();

            await HandleResponse(response, c);
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage.Write(ex.Message, c);
            return false;
        }
    }

    protected virtual Task ModifyRequest(HttpRequestMessage request, IPulseContext c) => Task.CompletedTask;
    protected virtual Task HandleResponse(HttpResponseMessage response, IPulseContext c) => Task.CompletedTask;
}

public abstract class HttpReadNode(HttpMethod method) : HttpNode(method)
{
    public ValueOutput<string> ResponseContentType = new("Content Type");
    public ValueOutput<string> ResponseBody = new("Body");

    protected override async Task HandleResponse(HttpResponseMessage response, IPulseContext c)
    {
        var body = await c.Run(response.Content.ReadAsStringAsync());
        ResponseBody.Write(body, c);
        ResponseContentType.Write(response.Content.Headers.ContentType?.MediaType ?? string.Empty, c);
    }
}

public abstract class HttpWriteNode(HttpMethod method) : HttpReadNode(method)
{
    public ValueInput<string> ContentType = new(defaultValue: "text/plain");
    public ValueInput<string> RequestBody = new("Body");

    protected override Task ModifyRequest(HttpRequestMessage request, IPulseContext c)
    {
        request.Content = new StringContent(RequestBody.Read(c), Encoding.UTF8, ContentType.Read(c));
        return Task.CompletedTask;
    }
}

[Node("HTTP GET", "Web")]
public sealed class HttpGetNode() : HttpReadNode(HttpMethod.Get);

[Node("HTTP POST", "Web")]
public sealed class HttpPostNode() : HttpWriteNode(HttpMethod.Post);

[Node("HTTP PUT", "Web")]
public sealed class HttpPutNode() : HttpWriteNode(HttpMethod.Put);

[Node("HTTP PATCH", "Web")]
public sealed class HttpPatchNode() : HttpWriteNode(HttpMethod.Patch);

[Node("HTTP DELETE", "Web")]
public sealed class HttpDeleteNode() : HttpReadNode(HttpMethod.Delete);

[Node("HTTP HEAD", "Web")]
public sealed class HttpHeadNode() : HttpNode(HttpMethod.Head);

[Node("HTTP OPTIONS", "Web")]
public sealed class HttpOptionsNode() : HttpNode(HttpMethod.Options)
{
    public ValueOutput<string> Allow = new();

    protected override Task HandleResponse(HttpResponseMessage response, IPulseContext c)
    {
        if (response.Content.Headers.TryGetValues("Allow", out var values))
            Allow.Write(string.Join(", ", values), c);
        return Task.CompletedTask;
    }
}