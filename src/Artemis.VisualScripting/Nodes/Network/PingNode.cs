using System.Net.NetworkInformation;
using Artemis.Core;
using Timer = System.Timers.Timer;

namespace Artemis.VisualScripting.Nodes.Network;

[Node("Ping", "Outputs the round-trip ping time for a given host", "Network", OutputType = typeof(string))]
public class PingNode : Node
{
    #region Constructors

    public PingNode()
    {
        Name = "Ping";
        Host = CreateInputPin<string>("Host");
        Output = CreateOutputPin<Numeric>("Ping (ms)");

        //update ping every 5 seconds
        _timer = new Timer(TimeSpan.FromSeconds(5));
        _timer.Elapsed += async (_, _) => await UpdatePing();
        _timer.Start();
    }

    #endregion

    #region Properties & Fields

    private readonly Timer _timer;
    private DateTime _lastEvaluateRequest;

    public InputPin<string> Host { get; }
    public OutputPin<Numeric> Output { get; }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        //keep track of the last time the node requested an evaluation
        //so we can decide whether or not to update the ping
        _lastEvaluateRequest = DateTime.UtcNow;
    }

    #endregion

    /// <summary>
    ///    Updates the ping value
    /// </summary>
    private async Task UpdatePing()
    {
        // If the last time the node requested an evaluation was more than 5 seconds ago, don't update the ping
        if (DateTime.UtcNow - _lastEvaluateRequest > TimeSpan.FromSeconds(5))
            return;
        
        if (string.IsNullOrEmpty(Host.Value))
        {
            Output.Value = -1;
            return;
        }

        try
        {
            Ping ping = new();
            PingReply reply = await ping.SendPingAsync(Host.Value);
            if (reply.Status == IPStatus.Success)
            {
                Output.Value = reply.RoundtripTime;
                return;
            }
        }
        catch
        {
            // ignored, let pings fail silently
        }

        Output.Value = -1;
    }
}