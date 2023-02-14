using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Polarities
{
    public abstract class Dash : ILoadable
    {
        private static Dictionary<int, Dash> dashes = new Dictionary<int, Dash>(); //takes index to dashes

        public abstract int Index { get; } //must not equal zero

        public abstract float Speed { get; }
        public abstract int Cooldown { get; }
        public abstract int Duration { get; }

        public void Load(Mod mod)
        {
            dashes.Add(Index, this);
        }

        public void Unload()
        {
            dashes = null;
        }

        public static bool HasDash(int index)
        {
            return dashes.ContainsKey(index);
        }

        public static Dash GetDash(int index)
        {
            return dashes[index];
        }

        public virtual bool TryUse(Player player)
        {
            return true;
        }

        public virtual void OnDash(Player player) { }

        public virtual void Update(Player player, int timeLeft)
        {
            player.eocDash = player.GetModPlayer<PolaritiesPlayer>().dashTimer;
            player.armorEffectDrawShadowEOCShield = true;
        }
    }
}

