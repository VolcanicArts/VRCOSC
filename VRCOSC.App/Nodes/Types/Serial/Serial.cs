// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Serial;

[Node("Serial Write", "Serial")]
public sealed class SerialWriteNode : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFail = new();

    public ValueInput<string?> PortName = new();
    public ValueInput<int> BaudRate = new();
    public ValueInput<Parity> Parity = new();
    public ValueInput<int> DataBits = new();
    public ValueInput<StopBits> StopBits = new();
    public ValueInput<string?> Command = new();

    protected override Task Process(PulseContext c)
    {
        var portName = PortName.Read(c);
        if (string.IsNullOrWhiteSpace(portName)) return Task.CompletedTask;

        SerialPort? serial;

        try
        {
            serial = new SerialPort(portName, BaudRate.Read(c), Parity.Read(c), DataBits.Read(c), StopBits.Read(c));
        }
        catch (Exception)
        {
            return OnFail.Execute(c);
        }

        var command = Command.Read(c);
        if (command is null) return OnFail.Execute(c);

        try
        {
            serial.Open();
            serial.WriteLine(command);
            serial.Close();
            return OnSuccess.Execute(c);
        }
        catch (Exception)
        {
            serial.Close();
            return OnFail.Execute(c);
        }
    }
}