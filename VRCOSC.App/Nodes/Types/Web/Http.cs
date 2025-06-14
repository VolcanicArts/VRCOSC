// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net.Http;
using VRCOSC.App.SDK.Nodes;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.Web;

[Node("HTTP GET", "Web")]
public sealed class HTTPGETNode : Node, IFlowInput
{
    private readonly HttpClient client = new();

    public FlowContinuation OnComplete = new("On Complete");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> URL = new();
    public ValueOutput<string> Content = new();

    protected override void Process(PulseContext c)
    {
        var url = URL.Read(c);

        if (string.IsNullOrEmpty(url))
        {
            OnFail.Execute(c);
            return;
        }

        try
        {
            var result = client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead, c.Token).Result;
            var content = result.Content.ReadAsStringAsync(c.Token).Result;
            Content.Write(content, c);
            OnComplete.Execute(c);
        }
        catch
        {
            OnFail.Execute(c);
        }
    }
}