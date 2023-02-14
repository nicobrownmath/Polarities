using Polarities.Items.Weapons.Ranged.Atlatls;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Prefixes
{
    public abstract class AtlatlPrefix : ModPrefix
    {
        public virtual float ScaleBoost => 1f;
        public virtual float SpeedBoost => 1f;
        public virtual float VelocityBoost => 1f;
        public virtual float DamageBoost => 1f;
        public virtual int CritBoost => 0;
        public virtual float KBBoost => 1f;

        public override float RollChance(Item item) => 1f;

        public override bool CanRoll(Item item)
        {
            return item.ModItem != null && item.ModItem is AtlatlBase;
        }

        public override PrefixCategory Category => PrefixCategory.Ranged;

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult *= DamageBoost;
            knockbackMult *= KBBoost;
            useTimeMult *= SpeedBoost;
            scaleMult *= ScaleBoost;
            shootSpeedMult *= VelocityBoost;
            critBonus += CritBoost;
        }
    }

    public class LargeAtlatlPrefix : AtlatlPrefix { public override float ScaleBoost => 1.12f; }
    public class MassiveAtlatlPrefix : AtlatlPrefix { public override float ScaleBoost => 1.18f; }
    public class DangerousAtlatlPrefix : AtlatlPrefix { public override float ScaleBoost => 1.05f; public override float DamageBoost => 1.05f; public override int CritBoost => 2; }
    public class SavageAtlatlPrefix : AtlatlPrefix { public override float ScaleBoost => 1.10f; public override float DamageBoost => 1.10f; public override float KBBoost => 1.10f; }
    public class TinyAtlatlPrefix : AtlatlPrefix { public override float ScaleBoost => 0.82f; }
    public class TerribleAtlatlPrefix : AtlatlPrefix { public override float ScaleBoost => 0.87f; public override float DamageBoost => 0.85f; public override float KBBoost => 0.85f; }
    public class SmallAtlatlPrefix : AtlatlPrefix { public override float ScaleBoost => 0.9f; }
    public class UnhappyAtlatlPrefix : AtlatlPrefix { public override float SpeedBoost => 1.10f; public override float ScaleBoost => 0.9f; public override float KBBoost => 0.9f; }
    public class BulkyAtlatlPrefix : AtlatlPrefix { public override float DamageBoost => 1.05f; public override float SpeedBoost => 1.15f; public override float ScaleBoost => 1.10f; public override float KBBoost => 1.10f; }
    public class ShamefulAtlatlPrefix : AtlatlPrefix { public override float DamageBoost => 0.9f; public override float ScaleBoost => 1.10f; public override float KBBoost => 0.80f; }
    public class HeavyAtlatlPrefix : AtlatlPrefix { public override float SpeedBoost => 1.10f; public override float KBBoost => 1.15f; }
    public class LightAtlatlPrefix : AtlatlPrefix { public override float SpeedBoost => 0.85f; public override float KBBoost => 0.90f; }
    public class LegendaryAtlatlPrefix : AtlatlPrefix { public override float DamageBoost => 1.15f; public override float SpeedBoost => 0.90f; public override int CritBoost => 5; public override float ScaleBoost => 1.10f; public override float KBBoost => 1.15f; }
}