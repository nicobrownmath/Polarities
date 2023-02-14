using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Armor;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Accessories.Wings
{
    [AutoloadEquip(EquipType.Wings)]
    public class BubbyWings : ModItem, IDrawArmor
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            //registers a wings glowmask
            ArmorMasks.wingIndexToArmorDraw.TryAdd(EquipLoader.GetEquipSlot(Mod, Name, EquipType.Wings), this);

            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(flyTime: 150, flySpeedOverride: 9f);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Cyan;
            Item.accessory = true;
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 0.5f;
            ascentWhenRising = 0.075f;
            maxCanAscendMultiplier = 0.5f;
            maxAscentMultiplier = 1f;
            constantAscend = 0.075f;
        }

        public static Asset<Texture2D> Mask1Texture;
        public static Asset<Texture2D> Mask2Texture;

        public override void Load()
        {
            Mask1Texture = Request<Texture2D>(Texture + "_Wings_Mask1");
            Mask2Texture = Request<Texture2D>(Texture + "_Wings_Mask2");
        }

        public override void Unload()
        {
            Mask1Texture = null;
            Mask2Texture = null;
        }

        public bool DoVanillaDraw() { return false; }

        public void DrawArmor(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            if (!drawPlayer.invis)
            {
                Texture2D texture = TextureAssets.Wings[drawPlayer.wings].Value;

                float drawX = (int)drawInfo.Position.X + drawPlayer.width / 2 - 5f - drawPlayer.direction * 13f;
                float drawY = (int)drawInfo.Position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height / 2 + 3f - 11f * drawPlayer.gravDir;
                Vector2 origin = new Vector2(20, 29);
                Vector2 position = new Vector2(drawX, drawY) + drawPlayer.bodyPosition - Main.screenPosition;

                Color color = drawInfo.colorArmorBody;
                Rectangle frame = new Rectangle(0, drawPlayer.GetModPlayer<PolaritiesPlayer>().bubbyWingFrame * 58, 50, 58);
                float rotation = drawPlayer.bodyRotation;
                SpriteEffects spriteEffects = drawInfo.playerEffect;

                DrawData drawData = new DrawData(texture, position, frame, color, rotation, origin, 1f, spriteEffects, 0)
                {
                    shader = drawInfo.cWings
                };
                drawInfo.DrawDataCache.Add(drawData);

                texture = Mask1Texture.Value;
                color = Color.White * drawPlayer.stealth;
                drawData = new DrawData(texture, position, frame, color, rotation, origin, 1f, spriteEffects, 0)
                {
                    shader = drawInfo.cWings
                };
                drawInfo.DrawDataCache.Add(drawData);

                texture = Mask2Texture.Value;
                color = Color.White * drawPlayer.stealth * 0.5f;
                drawData = new DrawData(texture, position, frame, color, rotation, origin, 1f, spriteEffects, 0)
                {
                    shader = drawInfo.cWings
                };
                drawInfo.DrawDataCache.Add(drawData);
            }
        }
    }
}

