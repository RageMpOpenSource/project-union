
using System.Drawing;

namespace PlayerGroups.Groups.Data
{
    public class PlayerGroupData
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public string[] Commands { get; set; }
    }
}
