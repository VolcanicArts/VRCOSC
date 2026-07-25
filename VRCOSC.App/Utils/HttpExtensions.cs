// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net.Http;
using System.Threading.Tasks;

namespace VRCOSC.App.Utils;

public static class HttpExtensions
{
    extension(HttpResponseMessage response)
    {
        public async Task<string> ToVerboseMessage()
        {
            var content = await response.Content.ReadAsStringAsync();
            return $"HTTP {(int)response.StatusCode} ({response.StatusCode}){(string.IsNullOrWhiteSpace(content) ? "." : $" ({content})")}";
        }
    }
}