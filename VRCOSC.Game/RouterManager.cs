// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Platform;
using VRCOSC.Game.OSC;

namespace VRCOSC.Game;

public class RouterManager
{
    public List<RouterData> Store = new();

    private const string file_name = "router.json";
    private readonly Storage storage;

    public RouterManager(Storage storage)
    {
        this.storage = storage;
    }

    public RouterData Create()
    {
        var routerData = new RouterData
        {
            Label = string.Empty,
            Endpoints = new OSCRouterEndpoints()
        };

        Store.Add(routerData);

        return routerData;
    }

    public void LoadData()
    {
        using (var stream = storage.GetStream(file_name))
        {
            if (stream is not null)
            {
                using var reader = new StreamReader(stream);

                var loadedData = JsonConvert.DeserializeObject<List<RouterData>>(reader.ReadToEnd())!;
                if (loadedData is not null) Store = loadedData;
            }
        }

        executeAfterLoad();
    }

    private void executeAfterLoad()
    {
        SaveData();
    }

    public void SaveData()
    {
        if (!Store.Any())
        {
            storage.Delete(file_name);
            return;
        }

        using var stream = storage.CreateFileSafely(file_name);

        if (stream is null) return;

        using var writer = new StreamWriter(stream);

        writer.Write(JsonConvert.SerializeObject(Store));
    }
}

public class RouterData
{
    public string Label = null!;
    public OSCRouterEndpoints Endpoints = null!;
}
