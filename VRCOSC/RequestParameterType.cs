// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC;

/// <summary>
/// Determines the type of a key-value parameter supplied to a <see cref="WebRequest"/>.
/// </summary>
public enum RequestParameterType
{
    /// <summary>
    /// This parameter should be contained in the query string of the request URL.
    /// </summary>
    Query,

    /// <summary>
    /// This parameter should be placed in the body of the request, using the <c>multipart/form-data</c> MIME type.
    /// </summary>
    Form
}
