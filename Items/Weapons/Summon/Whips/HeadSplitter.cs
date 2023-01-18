using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Summon.Whips
{
    public class HeadSplitter : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<HeadSplitterProjectile>(), 50, 2, 8, 30);

            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override bool MeleePrefix() => true;
    }

    public class HeadSplitterProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.HeadSplitter}");

            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();

            Projectile.WhipSettings.Segments = 40;
            Projectile.WhipSettings.RangeMultiplier = 0.9f;
        }

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override bool PreAI()
        {
            return true;
        }

        public override void ModifyDamageScaling(ref float damageScale)
        {
            //damage enemies less after every 3 hits
            damageScale *= (float)Math.Pow(0.6f, (int)Projectile.ai[1] / 3);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<HeadSplitterDebuff>(), 240);
            target.AddBuff(BuffID.Venom, 240);
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            Projectile.ai[1]++;
        }

        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float ai0 = Timer;

            for (int index = 0; index < Math.Min(3, Timer); index++)
            {
                Timer = ai0 - index;

                List<Vector2> list = new List<Vector2>();
                Projectile.FillWhipControlPoints(Projectile, list);
                for (int n = 0; n < list.Count; n++)
                {
                    Point point = list[n].ToPoint();
                    Rectangle myRect = new Rectangle(point.X - projHitbox.Width / 2, point.Y - projHitbox.Height / 2, projHitbox.Width, projHitbox.Height);
                    if (myRect.Intersects(targetHitbox))
                    {
                        Timer = ai0;
                        return true;
                    }
                }
            }

            Timer = ai0;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float ai0 = Timer;

            for (int index = 0; index < Math.Min(3, Timer); index++)
            {
                Timer = ai0 - index;

                List<Vector2> list = new List<Vector2>();
                Projectile.FillWhipControlPoints(Projectile, list);

                DrawLine(list);

                SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                Main.instance.LoadProjectile(Type);
                Texture2D texture = TextureAssets.Projectile[Type].Value;

                Vector2 pos = list[0];

                for (int i = 0; i < list.Count - 1; i++)
                {
                    Rectangle frame = new Rectangle(0, 0, 22, 24);
                    Vector2 origin = new Vector2(11, 5);
                    float scale = 1;

                    if (i == list.Count - 2)
                    {
                        frame.Y = 62;
                        frame.Height = 24;

                        Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                        float t = Timer / timeToFlyOut;
                        scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                    }
                    else if (i == 0)
                    {
                        frame.Y = 26;
                        frame.Height = 10;
                    }
                    else if (i % 2 == 0)
                    {
                        frame.Y = 38;
                        frame.Height = 10;
                    }
                    else
                    {
                        frame.Y = 50;
                        frame.Height = 10;
                    }

                    Vector2 element = list[i];
                    Vector2 diff = list[i + 1] - element;

                    float rotation = diff.ToRotation() - MathHelper.PiOver2;
                    Color color = Lighting.GetColor(element.ToTileCoordinates());

                    Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                    pos += diff;
                }
            }

            Timer = ai0;

            return false;
        }
    }

    public class HeadSplitterDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.VortexDebuff;

        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<PolaritiesNPC>().whipTagDamage += 8;
        }
    }
}
