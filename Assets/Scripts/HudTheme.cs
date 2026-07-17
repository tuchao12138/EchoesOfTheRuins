using UnityEngine;

namespace EchoesOfTheRuins
{
    public readonly struct HudAlert
    {
        public readonly string Message;
        public readonly Color Color;

        public HudAlert(string message, Color color)
        {
            Message = message;
            Color = color;
        }
    }

    public static class HudTheme
    {
        public static HudAlert GetAlert(GuardianState state) => state switch
        {
            GuardianState.Chase or GuardianState.Capture => new HudAlert("DETECTED - BREAK LINE OF SIGHT", new Color(1f, .22f, .18f)),
            GuardianState.Investigate or GuardianState.Search => new HudAlert("SEARCHING - STAY IN COVER", new Color(1f, .76f, .18f)),
            _ => new HudAlert("SAFE - WATCH THE BLUE VISION CONE", new Color(.38f, .86f, 1f))
        };
    }
}
