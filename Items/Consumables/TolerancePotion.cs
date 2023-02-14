using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Consumables
{
    public class TolerancePotion : ModItem, IInventoryDrawItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (20);

            ItemID.Sets.DrinkParticleColors[Type] = new Color[] { Color.White, Color.LightPink };
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 34;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 2);
            Item.buffType = BuffType<TolerancePotionCooldownBuff>();
            Item.buffTime = 3600;
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffType<TolerancePotionCooldownBuff>()) && player.HasBuff(BuffID.PotionSickness);
        }

        public override bool ConsumeItem(Player player)
        {
            player.buffTime[player.FindBuffIndex(BuffID.PotionSickness)] /= 2;
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater)
                .AddIngredient(ItemType<Placeable.SaltCrystals>())
                .AddIngredient(ItemType<Materials.BrineShrimp>())
                .AddTile(TileID.Bottles)
                .Register();
        }

        void IInventoryDrawItem.PostInventoryDraw(SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
        {
            if (context == 13) //I don't think we can use the tml hook if we want to replicate vanilla behavior
            {
                Player player = Main.player[Main.myPlayer];

                if (!player.HasBuff(BuffType<TolerancePotionCooldownBuff>())) return;

                Item item = inv[slot];
                float inventoryScale = Main.inventoryScale;

                Color color = Color.White;
                if (lightColor != Color.Transparent)
                {
                    color = lightColor;
                }

                Texture2D value = TextureAssets.InventoryBack.Value;

                Texture2D value6 = TextureAssets.Item[item.type].Value;
                Rectangle rectangle2 = ((Main.itemAnimations[item.type] == null) ? value6.Frame() : Main.itemAnimations[item.type].GetFrame(value6));

                float num10 = 1f;
                if (rectangle2.Width > 32 || rectangle2.Height > 32)
                {
                    num10 = ((rectangle2.Width <= rectangle2.Height) ? (32f / rectangle2.Height) : (32f / rectangle2.Width));
                }
                num10 *= inventoryScale;

                Vector2 position3 = position + value.Size() * inventoryScale / 2f - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
                Color color3 = item.GetAlpha(color) * (player.buffTime[player.FindBuffIndex(BuffType<TolerancePotionCooldownBuff>())] / (float)player.GetModPlayer<PolaritiesPlayer>().tolerancePotionDelayTime);
                spriteBatch.Draw(TextureAssets.Cd.Value, position3, null, color3, 0f, default(Vector2), num10, 0, 0f);
            }
        }
    }

    public class TolerancePotionCooldownBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
        }

        public override bool ReApply(Player player, int time, int buffIndex)
        {
            player.GetModPlayer<PolaritiesPlayer>().tolerancePotionDelayTime = time;
            return true;
        }
    }
}

