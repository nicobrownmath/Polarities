using Terraria;
using Terraria.ModLoader;

namespace Polarities.Prefixes
{
    public abstract class AutomaticOrManualPrefix : ModPrefix
    {
        public abstract bool IsAutomatic { get; }

        public override float RollChance(Item item) => 1f;
        public override bool CanRoll(Item item)
        {
            if (item.channel)
            {
                return false;
            }
            if (item.autoReuse ^ !IsAutomatic)
            {
                return false;
            }
            return true;
        }

        public override PrefixCategory Category => PrefixCategory.AnyWeapon;

        public override void Apply(Item item)
        {
            item.autoReuse = IsAutomatic;
        }

        public override void ModifyValue(ref float valueMult)
        {
            float multiplier = IsAutomatic ? 1.5f : 0.66f;
            valueMult *= multiplier;
        }
    }

    public class AutomaticPrefix : AutomaticOrManualPrefix { public override bool IsAutomatic => true; }
    public class ManualPrefix : AutomaticOrManualPrefix { public override bool IsAutomatic => false; }
}