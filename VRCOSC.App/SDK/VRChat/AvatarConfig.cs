﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.SDK.VRChat;

public class AvatarConfig
{
    [JsonProperty("id")]
    public string Id = null!;

    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("parameters")]
    public List<AvatarConfigParameter> Parameters = null!;
}

public class AvatarConfigParameter
{
    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("input")]
    public AddressTypePair? Input;

    [JsonProperty("output")]
    public AddressTypePair? Output;
}

public class AddressTypePair
{
    [JsonProperty("address")]
    public string? Address;

    [JsonProperty("type")]
    private string? type;

    public ParameterType Type => type switch
    {
        "Bool" => ParameterType.Bool,
        "Float" => ParameterType.Float,
        "Int" => ParameterType.Int,
        _ => throw new InvalidOperationException($"Cannot parse type {type}")
    };
}