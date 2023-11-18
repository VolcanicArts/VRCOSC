// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.SDK.Modules.Heartrate;

/// <summary>
/// The base class for anything looking to gather heartrate data from a source
/// </summary>
public abstract class HeartrateProvider
{
    private DateTimeOffset lastHeartrateDateTime;

    /// <summary>
    /// Used to decide the connection state of this <see cref="HeartrateProvider"/>
    /// </summary>
    public virtual bool IsConnected => false;

    /// <summary>
    /// Used decide if this <see cref="HeartrateProvider"/> is receiving heartrate values.
    /// For example, <see cref="IsConnected"/> could be true but values may not be being received
    /// </summary>
    public virtual bool IsReceiving => IsConnected && lastHeartrateDateTime + IsReceivingTimeout >= DateTimeOffset.Now;

    /// <summary>
    /// Called when this <see cref="HeartrateProvider"/> is broadcasting information.
    /// By default, <see cref="HeartrateModule{T}"/> captures this and automatically logs the message
    /// </summary>
    public Action<string>? OnLog;

    /// <summary>
    /// Called when this <see cref="HeartrateProvider"/> is connected and ready to send data to <see cref="OnHeartrateUpdate"/>
    /// </summary>
    public Action? OnConnected;

    /// <summary>
    /// Called when this <see cref="HeartrateProvider"/> is disconnected and will no longer send data to <see cref="OnHeartrateUpdate"/>
    /// </summary>
    public Action? OnDisconnected;

    /// <summary>
    /// Called when this <see cref="HeartrateProvider"/> receives a new heartrate value.
    /// This is listened on internally so DO NOT set directly. Use the +=/-= syntax instead if you want to bind/unbind
    /// </summary>
    public Action<int>? OnHeartrateUpdate;

    /// <summary>
    /// How long should we wait after the last <see cref="OnHeartrateUpdate"/> call until <see cref="IsReceiving"/> resolves to false
    /// </summary>
    protected virtual TimeSpan IsReceivingTimeout => TimeSpan.FromSeconds(30);

    /// <summary>
    /// A stub method that forwards <paramref name="message"/> to <see cref="OnLog"/>
    /// </summary>
    /// <param name="message">The message to forward to <see cref="OnLog"/></param>
    protected void Log(string message) => OnLog?.Invoke(message);

    protected HeartrateProvider()
    {
        OnHeartrateUpdate += _ => lastHeartrateDateTime = DateTimeOffset.Now;
    }

    /// <summary>
    /// Initialises this <see cref="HeartrateProvider"/>
    /// </summary>
    public virtual Task Initialise()
    {
        lastHeartrateDateTime = DateTimeOffset.MinValue;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Tears down this <see cref="HeartrateProvider"/>
    /// </summary>
    public abstract Task Teardown();
}
