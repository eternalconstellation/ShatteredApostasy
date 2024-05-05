using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ShatteredApostasy.Systems
{
    public class KeybindSystem : ModSystem
    {
        public ModKeybind DebugResetVelocity { get; private set; }
        public ModKeybind DebugGoUP { get; private set; }
        public ModKeybind DebugGoDOWN { get; private set; }
        public ModKeybind DebugStayStill { get; private set; }
        public override void Load()
        {
            DebugResetVelocity = KeybindLoader.RegisterKeybind(Mod, "DebugResetVelocity", "Q");
            DebugGoUP = KeybindLoader.RegisterKeybind(Mod, "DebugGoUp", "E");
            DebugGoDOWN = KeybindLoader.RegisterKeybind(Mod, "DebugGoDown", "Z");
            DebugStayStill = KeybindLoader.RegisterKeybind(Mod, "DebugStayStill", "Y");
        }

        public override void Unload()
        {
            DebugGoUP = null;
            DebugResetVelocity = null;
            DebugGoDOWN = null;
            DebugStayStill = null;
        }
    }
}
