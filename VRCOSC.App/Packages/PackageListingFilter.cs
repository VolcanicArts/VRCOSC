// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Packages;

[Flags]
public enum PackageListingFilter
{
    None = 0,
    Type_Official = 1 << 0,
    Type_Curated = 1 << 1,
    Type_Community = 1 << 2,
    Release_Unavailable = 1 << 3,
    Release_Incompatible = 1 << 4
}
