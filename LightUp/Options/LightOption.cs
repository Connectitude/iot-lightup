using System;

namespace Connectitude.LightUp.Options
{
    public class LightOption
    {
        public LightOption()
        {
            Brightness = 100;
        }

        public string Color { get; set; }

        public byte Brightness { get; set; }
    }
}
