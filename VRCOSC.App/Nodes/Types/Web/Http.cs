// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.Web;

[Node("HTTP GET", "Web")]
public sealed class HTTPGETNode : Node, IFlowInput
{
    private readonly HttpClient client = new();

    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> URL = new();
    public ValueOutput<HttpStatusCode> StatusCode = new("Status Code");
    public ValueOutput<string> Content = new();

    protected override async Task Process(PulseContext c)
    {
        var url = URL.Read(c);

        if (string.IsNullOrEmpty(url))
        {
            await OnFail.Execute(c);
            return;
        }

        try
        {
            var result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri(url)), c.Token);
            StatusCode.Write(result.StatusCode, c);

            result.EnsureSuccessStatusCode();

            var content = result.Content.ReadAsStringAsync(c.Token).Result;
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
public sealed class HTTPPOSTNode : Node, IFlowInput
{
    private readonly HttpClient client = new();

    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> URL = new();
    public ValueOutput<HttpStatusCode> StatusCode = new("Status Code");

    protected override async Task Process(PulseContext c)
    {
        var url = URL.Read(c);

        if (string.IsNullOrEmpty(url))
        {
            await OnFail.Execute(c);
            return;
        }

        try
        {
            var result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, new Uri(url)), c.Token);
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