using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Polarities.Items
{
    public interface IDrawHeldItem
    {
        void DrawHeldItem(ref PlayerDrawSet drawInfo);

        virtual bool DoVanillaDraw() => true;
    }

    public class HeldItemDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.ArmOverItem, PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.itemAnimation != 0 && !drawInfo.drawPlayer.HeldItem.noUseGraphic && drawInfo.drawPlayer.HeldItem.ModItem != null && drawInfo.drawPlayer.HeldItem.ModItem is IDrawHeldItem;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            (drawInfo.drawPlayer.HeldItem.ModItem as IDrawHeldItem)?.DrawHeldItem(ref drawInfo);
        }
    }
}

