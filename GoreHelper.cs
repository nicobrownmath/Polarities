using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Polarities
{
    public static class GoreHelper
    {
        public static int GoreType(string name)
        {
            return ModContent.GetInstance<Polarities>().Find<ModGore>(name).Type;
        }
        public static Gore DeathGore(this NPC npc, Vector2 pos, string name, Vector2? velocity = null)
        {
            var g = Gore.NewGoreDirect(npc.GetSource_Death(), pos, velocity ?? npc.velocity, GoreType(name));
            g.position.X -= g.Width / 2f;
            g.position.Y -= g.Height / 2f;
            return g;
        }
        public static Gore DeathGore(this NPC npc, string name, Vector2 offset = default(Vector2), Vector2? velocity = null)
        {
            return DeathGore(npc, npc.Center + offset, name, velocity ?? npc.velocity);
        }
    }
}