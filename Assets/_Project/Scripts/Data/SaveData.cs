using System;

namespace Project.Data
{
    [Serializable]
    public sealed class SaveData
    {
        public int stage = 1;
        public int gold = 0;
        public int attackLevel = 0;
        public int attackSpeedLevel = 0;
        public int critChanceLevel = 0;
        public string lastQuitUtc = "";
    }
}
