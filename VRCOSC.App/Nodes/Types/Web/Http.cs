// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.Web;

[Node("HTTP GET", "Web")]
public sealed class HttpGetNode : Node, IFlowInput
{
    private readonly HttpClient client = new();

    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> URL = new();
    public ValueInput<Dictionary<string, string>> Headers = new();
    public ValueOutput<HttpStatusCode> StatusCode = new("Status Code");
    public ValueOutput<string> Content = new();

    protected override async Task Process(PulseContext c)
    {
        var url = URL.Read(c);
        var headers = Headers.Read(c);

        if (string.IsNullOrEmpty(url))
        {
            await OnFail.Execute(c);
            return;
        }

        headers ??= new Dictionary<string, string>();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

            foreach (var header in headers) request.Headers.Add(header.Key, header.Value);

            var result = await client.SendAsync(request, c.Token);
            StatusCode.Write(result.StatusCode, c);

            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync(c.Token);
            Content.Write(content, c);

            await OnSuccess.Execute(c);
        }
        catch
        {
            await OnFail.Execute(c);
        }
    }
}

[Node("HTTP POST", "Web")]
public sealed class HttpPostNode : Node, IFlowInput
{
    private readonly HttpClient client = new();

    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> URL = new();
    public ValueInput<Dictionary<string, string>> Headers = new();
    public ValueInput<string> Content = new();
    public ValueOutput<HttpStatusCode> StatusCode = new("Status Code");

    protected override async Task Process(PulseContext c)
    {
        var url = URL.Read(c);
        var content = Content.Read(c);
        var headers = Headers.Read(c);

        if (string.IsNullOrEmpty(url))
        {
            await OnFail.Execute(c);
            return;
        }

        headers ??= new Dictionary<string, string>();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            request.Content = new StringContent(content);

            foreach (var header in headers) request.Headers.Add(header.Key, header.Value);

            var result = await client.SendAsync(request, c.Token);
            StatusCode.Write(result.StatusCode, c);

            result.EnsureSuccessStatusCode();

            await OnSuccess.Execute(c);
        }
        catch
        {
            await OnFail.Execute(c);
        }
    }
}