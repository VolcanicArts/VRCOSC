// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.Json;

namespace VRCOSC.App.Utils;

public static class JsonSerializerSafe
{
    public static Result<T> TryDeserialize<T>(string content)
    {
        try
        {
            var data = JsonSerializer.Deserialize<T>(content);
            return data is not null ? data : new JsonException("Null data has been deserialized");
        }
        catch (Exception e)
        {
            return e;
        }
    }
}