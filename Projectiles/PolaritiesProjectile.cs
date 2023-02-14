using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Polarities.Items.Books;
using Polarities.NPCs;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Projectiles
{
    public class PolaritiesProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override void Load()
        {
            //completely overwrite UpdateMaxTurrets to allow for sentry slots
            //this probably isn't great for crossmod but in fairness the method is tiny and IL edits would be likely to conflict too
            On.Terraria.Player.UpdateMaxTurrets += Player_UpdateMaxTurrets;

            //projectiles with forced drawing
            IL.Terraria.Main.DrawProj += Main_DrawProj;

            //prevents projectiles with doNotStrikeNPC true from damaging enemies
            IL.Terraria.Projectile.Damage += Projectile_Damage;

            //custom projectile shaders
            On.Terraria.Main.GetProjectileDesiredShader += Main_GetProjectileDesiredShader;

            //prevent certain projectiles from despawning outside of the world
            IL.Terraria.Projectile.Update += Projectile_Update;
        }

        public bool canLeaveWorld;

        private void Projectile_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdflda(typeof(Entity).GetField("position", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdfld(typeof(Vector2).GetField("Y", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdsfld(typeof(Main).GetField("topWorld", BindingFlags.Static | BindingFlags.Public)),
                i => i.MatchBle(out _),
                i => i.MatchLdarg(0),
                i => i.MatchLdflda(typeof(Entity).GetField("position", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdfld(typeof(Vector2).GetField("Y", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Entity).GetField("height", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchConvR4(),
                i => i.MatchAdd(),
                i => i.MatchLdsfld(typeof(Main).GetField("bottomWorld", BindingFlags.Static | BindingFlags.Public)),
                i => i.MatchBltUn(out label),
                i => i.MatchLdarg(0)
                ))
            {
                Mod.Logger.Debug("Failed to find patch location");
                return;
            }

            c.EmitDelegate<Func<Projectile, bool>>((projectile) =>
            {
                return projectile.GetGlobalProjectile<PolaritiesProjectile>().canLeaveWorld;
            });
            c.Emit(OpCodes.Brtrue, label);
            c.Emit(OpCodes.Ldarg, 0);
        }

        public int customShader = 0;

        private int Main_GetProjectileDesiredShader(On.Terraria.Main.orig_GetProjectileDesiredShader orig, int i)
        {
            Projectile projectile = Main.projectile[i];

            if (projectile.GetGlobalProjectile<PolaritiesProjectile>().customShader != 0)
            {
                return projectile.GetGlobalProjectile<PolaritiesProjectile>().customShader;
            }

            return orig(i);
        }

        public bool doNotStrikeNPC = false;

        private void Projectile_Damage(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdloc(36),
                i => i.MatchBrfalse(out _),
                i => i.MatchLdloc(33),
                i => i.MatchLdloc(39),
                i => i.MatchLdloc(34),
                i => i.MatchLdloc(40),
                i => i.MatchLdloc(35),
                i => i.MatchLdcI4(0),
                i => i.MatchLdcI4(0),
                i => i.MatchCallvirt(typeof(NPC).GetMethod("StrikeNPC", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchConvI4(),
                i => i.MatchBr(out label)
                ))
            {
                Mod.Logger.Debug("Failed to find patch location");
                return;
            }

            ILLabel label2 = c.DefineLabel();
            label2.Target = c.Next;

            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<Projectile, bool>>((projectile) =>
            {
                return projectile.GetGlobalProjectile<PolaritiesProjectile>().doNotStrikeNPC;
            });
            c.Emit(OpCodes.Brfalse, label2);
            c.Emit(OpCodes.Ldc_R8, 0.0);
            c.Emit(OpCodes.Br, label);
        }

        private void Main_DrawProj(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloca(2),
                i => i.MatchLdsfld(typeof(Main).GetField("projectile", BindingFlags.Static | BindingFlags.Public)),
                i => i.MatchLdarg(1),
                i => i.MatchLdelemRef(),
                i => i.MatchCallvirt(typeof(Entity).GetProperty("Hitbox", BindingFlags.Instance | BindingFlags.Public).GetGetMethod()),
                i => i.MatchCall(typeof(Rectangle).GetMethod("Intersects", BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Rectangle) })),
                i => i.MatchBrtrue(out label)
                ))
            {
                Mod.Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldarg, 1);
            c.EmitDelegate<Func<int, bool>>((index) =>
            {
                return Main.projectile[index].GetGlobalProjectile<PolaritiesProjectile>().ForceDraw;
            });
            c.Emit(OpCodes.Brtrue, label);
        }

        public bool ForceDraw = false;

        private bool hookSpeedModified = false;
        private int baseHookExtraUpdates = 0;

        public float turretSlots = 1f;

        public int projectileHitCooldown;
        public int generalHitCooldown;

        public bool usesGeneralHitCooldowns;
        public int generalHitCooldownTime;

        public bool planteraBookHooks;

        public bool candyCaneAtlatl;

        public override bool PreAI(Projectile projectile)
        {
            if (generalHitCooldown > 0) generalHitCooldown--;
            if (projectileHitCooldown > 0) projectileHitCooldown--;

            if (projectile.aiStyle == 7)
            {
                //hook speed modification
                if (!hookSpeedModified)
                {
                    hookSpeedModified = true;
                    projectile.velocity *= Main.player[projectile.owner].GetModPlayer<PolaritiesPlayer>().hookSpeedMult;

                    if (Main.player[projectile.owner].GetModPlayer<PolaritiesPlayer>().hookSpeedMult > 1)
                    {
                        //dusts for cosmic cable
                        int num537 = 4;
                        int num538 = 3;
                        for (int num539 = 0; num539 < num537; num539++)
                        {
                            Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default(Color), 1.2f);
                        }
                        for (int num540 = 0; num540 < num538; num540++)
                        {
                            int num541 = Main.rand.Next(16, 18);
                            Gore.NewGore(projectile.GetSource_FromAI(), projectile.position + new Vector2(Main.rand.Next(projectile.width - 16), Main.rand.Next(projectile.height - 16)), new Vector2(projectile.velocity.X * 0.05f, projectile.velocity.Y * 0.05f), num541);
                        }
                        for (int num542 = 0; num542 < 4; num542++)
                        {
                            Dust.NewDust(projectile.position + new Vector2(Main.rand.Next(projectile.width - 16), Main.rand.Next(projectile.height - 16)), projectile.width, projectile.height, 57, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default(Color), 1.2f);
                        }
                        for (int num543 = 0; num543 < 3; num543++)
                        {
                            Gore.NewGore(projectile.GetSource_FromAI(), projectile.position + new Vector2(Main.rand.Next(projectile.width - 16), Main.rand.Next(projectile.height - 16)), new Vector2(projectile.velocity.X * 0.05f, projectile.velocity.Y * 0.05f), Main.rand.Next(16, 18));
                        }
                    }

                    if (projectile.velocity.Length() > 16)
                    {
                        baseHookExtraUpdates = projectile.extraUpdates;

                        int speedMultiplier = 1 + (int)(projectile.velocity.Length() / 16);
                        projectile.velocity /= speedMultiplier;
                        projectile.extraUpdates = projectile.extraUpdates * speedMultiplier + (speedMultiplier - 1);
                    }
                }

                if (projectile.ai[0] != 0)
                {
                    projectile.extraUpdates = baseHookExtraUpdates;
                }

                //plantera book tentacles
                if (Main.player[projectile.owner].HasBuff(BuffType<PlanteraBookBuff>()))
                {
                    if (!planteraBookHooks)
                    {
                        planteraBookHooks = true;
                        for (int i = 0; i < 3; i++)
                        {
                            Projectile.NewProjectile(projectile.GetSource_FromAI(), Main.player[projectile.owner].Center, Main.player[projectile.owner].velocity, ProjectileType<PlanteraBookHookProjectile>(), 30, 2, projectile.owner, Main.rand.NextFloat(MathHelper.Pi * 2), projectile.whoAmI);
                        }
                    }
                }
                else
                {
                    planteraBookHooks = false;
                }
            }

            return true;
        }

        private void Player_UpdateMaxTurrets(On.Terraria.Player.orig_UpdateMaxTurrets orig, Player self)
        {
            List<Projectile> list = new List<Projectile>();
            float occupiedTurretSlots = 0f;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].WipableTurret)
                {
                    list.Add(Main.projectile[i]);
                    occupiedTurretSlots += Main.projectile[i].GetGlobalProjectile<PolaritiesProjectile>().turretSlots;
                }
            }
            int num = 0;
            while (occupiedTurretSlots > self.maxTurrets && ++num < 1000)
            {
                Projectile projectile = list[0];
                for (int j = 1; j < list.Count; j++)
                {
                    if (list[j].timeLeft < projectile.timeLeft)
                    {
                        projectile = list[j];
                    }
                }
                occupiedTurretSlots -= projectile.GetGlobalProjectile<PolaritiesProjectile>().turretSlots;
                projectile.Kill();
                list.Remove(projectile);
            }
        }

        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            if (target.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns && projectileHitCooldown != 0)
            {
                return false;
            }
            if (generalHitCooldown != 0)
            {
                return false;
            }

            return base.CanHitNPC(projectile, target);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if (target.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns)
            {
                projectileHitCooldown = target.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime;
            }
            if (usesGeneralHitCooldowns)
            {
                target.immune[projectile.owner] = 0;
                generalHitCooldown = generalHitCooldownTime;
            }

            Player player = Main.player[projectile.owner];

            if (candyCaneAtlatl)
            {
                candyCaneAtlatl = false;

                player.GetModPlayer<PolaritiesPlayer>().candyCaneAtlatlBoost += 60;
                if (player.GetModPlayer<PolaritiesPlayer>().candyCaneAtlatlBoost >= 60 * 15)
                {
                    player.GetModPlayer<PolaritiesPlayer>().candyCaneAtlatlBoost = 60 * 15;
                }
            }
        }
    }
}

