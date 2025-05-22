// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO.Ports;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Serial Write", "Actions/Serial")]
public sealed class SerialWriteNode : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> PortName = new();
    public ValueInput<int> BaudRate = new();
    public ValueInput<Parity> Parity = new();
    public ValueInput<int> DataBits = new();
    public ValueInput<StopBits> StopBits = new();
    public ValueInput<string> Command = new();

    protected override void Process(PulseContext c)
    {
        var portName = PortName.Read(c);
        if (string.IsNullOrEmpty(portName)) return;

        using SerialPort serial = new SerialPort(portName, BaudRate.Read(c), Parity.Read(c), DataBits.Read(c), StopBits.Read(c));
        var command = Command.Read(c);

        if (string.IsNullOrEmpty(command))
        {
            OnFail.Execute(c);
            return;
        }

        try
        {
            serial.Open();
            serial.WriteLine(command);
        }
        catch (Exception)
        {
            OnFail.Execute(c);
            return;
        }
        finally
        {
            serial.Close();
        }

        OnSuccess.Execute(c);
    }
}