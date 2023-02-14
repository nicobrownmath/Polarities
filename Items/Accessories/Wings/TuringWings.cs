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
    public class TuringWings : ModItem, IDrawArmor
    {
        public static Asset<Texture2D> PulseTexture;

        public override void Load()
        {
            PulseTexture = Request<Texture2D>(Texture + "_Pulse");
        }

        public override void Unload()
        {
            PulseTexture = null;
        }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            //registers a wings glowmask
            ArmorMasks.wingIndexToArmorDraw.TryAdd(EquipLoader.GetEquipSlot(Mod, Name, EquipType.Wings), this);

            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(flyTime: 150, flySpeedOverride: 7f);
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Cyan;
            Item.accessory = true;
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 0.5f;
            ascentWhenRising = 0.1f;
            maxCanAscendMultiplier = 0.5f;
            maxAscentMultiplier = 1.5f;
            constantAscend = 0.1f;
        }

        public bool DoVanillaDraw()
        {
            return false;
        }

        public void DrawArmor(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            // We don't want the glowmask to draw if the player is cloaked or dead
            if (drawInfo.shadow != 0f || drawPlayer.dead)
            {
                return;
            }

            // The texture we want to display on our player
            Texture2D texture = PulseTexture.Value;

            float alpha = (255 - drawPlayer.immuneAlpha) / 255f;
            Color color = Color.White * drawPlayer.stealth;
            Rectangle frame = new Rectangle(0, 0, 28, 28);
            float rotation = drawPlayer.bodyRotation;
            SpriteEffects spriteEffects = SpriteEffects.None;

            float drawX = (int)drawInfo.Position.X + drawPlayer.width / 2;
            float drawY = (int)drawInfo.Position.Y + drawPlayer.height / 2;
            Vector2 origin = new Vector2(14, -14);

            if (!drawPlayer.mount.Active && drawPlayer.grapCount <= 0)
            {
                PolaritiesPlayer modPlayer = drawPlayer.GetModPlayer<PolaritiesPlayer>();
                for (int i = 0; i < 10; i++)
                {
                    if (modPlayer.oldVelocities[i].Y != 0)
                    {
                        Vector2 pulsePosition = modPlayer.oldCenters[i] - new Vector2(modPlayer.oldDirections[i] * 10, drawPlayer.gravDir * 6) + modPlayer.oldVelocities[i] * 0.75f * i;
                        for (int j = 0; j < 4; j++)
                        {
                            float pulseScale = i * 0.125f +
                                ((drawPlayer.gravDir == 1) ?
                                (j == 0 || j == 3) ? 0f : 0.25f
                                :
                                (j == 0 || j == 3) ? 0.25f : 0f);

                            float pulseRotation = 0;
                            switch (j)
                            {
                                case 0:
                                    pulseRotation = MathHelper.Pi / 2 - MathHelper.Pi / 6;
                                    break;
                                case 1:
                                    pulseRotation = MathHelper.Pi / 2 + MathHelper.Pi / 6;
                                    break;
                                case 2:
                                    pulseRotation = -MathHelper.Pi / 2 - MathHelper.Pi / 6;
                                    break;
                                case 3:
                                    pulseRotation = -MathHelper.Pi / 2 + MathHelper.Pi / 6;
                                    break;
                            }

                            DrawData drawData = new DrawData(texture, drawPlayer.position - drawPlayer.oldPosition + pulsePosition - Main.screenPosition, frame, color * alpha * ((4.5f - pulseScale) / 4.5f), rotation + pulseRotation, origin, pulseScale, spriteEffects, 0)
                            {
                                shader = drawInfo.cWings
                            };
                            drawInfo.DrawDataCache.Add(drawData);
                        }
                    }
                }
            }

            int num3 = 0;
            int num4 = 0;
            int num5 = 4;
            Color color6 = drawInfo.colorArmorBody;
            Vector2 vector = drawInfo.Position - Main.screenPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + new Vector2(0, 7);
            Vector2 vector5 = vector + new Vector2(num4 - 9, num3 + 2) * drawPlayer.Directions;
            DrawData drawData2 = new DrawData(TextureAssets.Wings[drawPlayer.wings].Value, vector5.Floor(), (Rectangle?)new Rectangle(0, TextureAssets.Wings[drawPlayer.wings].Height() / num5 * drawPlayer.wingFrame, TextureAssets.Wings[drawPlayer.wings].Width(), TextureAssets.Wings[drawPlayer.wings].Height() / num5), color6, drawPlayer.bodyRotation, new Vector2(TextureAssets.Wings[drawPlayer.wings].Width() / 2, TextureAssets.Wings[drawPlayer.wings].Height() / num5 / 2), 1f, drawInfo.playerEffect, 0)
            {
                shader = drawInfo.cWings
            };
            drawInfo.DrawDataCache.Add(drawData2);
        }
    }
}

