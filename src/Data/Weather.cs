using System;
using System.Drawing;

namespace RogueSurvivor.Data
{
    [Serializable]
    enum Weather
    {
        _FIRST,

        CLEAR = _FIRST,
        CLOUDY,
        RAIN,
        HEAVY_RAIN,

        _COUNT
    }

    static class WeatherMethods
    {
        public static string AsString(this Weather weather)
        {
            switch (weather)
            {
                case Weather.CLOUDY: return "Cloudy";
                case Weather.HEAVY_RAIN: return "Heavy rain";
                case Weather.RAIN: return "Rain";
                case Weather.CLEAR: return "Clear";

                default:
                    throw new ArgumentOutOfRangeException("unhandled weather");
            }
        }

        public static Color ToColor(this Weather weather)
        {
            switch (weather)
            {
                case Weather.CLOUDY: return Color.Gray;
                case Weather.HEAVY_RAIN: return Color.Blue;
                case Weather.RAIN: return Color.LightBlue;
                case Weather.CLEAR: return Color.Yellow;

                default:
                    throw new ArgumentOutOfRangeException("unhandled weather");
            }
        }
    }
}
