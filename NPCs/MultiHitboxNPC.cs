using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;
using Terraria.GameInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Polarities.Items;
using Polarities.NPCs;
using MonoMod.Cil;
using Terraria.ModLoader.IO;
using Terraria.Enums;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using static Terraria.GameContent.ItemDropRules.Chains;
using System.Reflection;
using Mono.Cecil.Cil;

namespace Polarities.NPCs
{
    //Javelins break with MultiHitboxNPCs
    //I don't really see how to fix this one without a full rewrite of Javelin code
    //This doesn't have any impact on their functionality so it's not a major issue
    //TODO: Produce hit sound from real center
    //TODO: Mouse hovering
    //TODO: It could be cool to use or at least allow for some sort of nested structure to minimize collision checks (I probably don't need this right now but if/when I start doing things with more hitboxes than the NPC cap it'll be useful
	public class MultiHitboxNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool useMultipleHitboxes;
        public Rectangle[] hitboxes;
        public int lastHitSegment;

        public int widthForInteractions;
        public int heightForInteractions;

        public override void Load()
        {
            On.Terraria.Collision.CheckAABBvLineCollision_Vector2_Vector2_Vector2_Vector2_float_refSingle += Collision_CheckAABBvLineCollision_Vector2_Vector2_Vector2_Vector2_float_refSingle;

            On.Terraria.Player.ItemCheck_MeleeHitNPCs += Player_ItemCheck_MeleeHitNPCs;
            On.Terraria.NPC.Collision_LavaCollision += NPC_Collision_LavaCollision;
            On.Terraria.NPC.UpdateNPC_BuffSetFlags += NPC_UpdateNPC_BuffSetFlags;
            On.Terraria.NPC.UpdateNPC_BuffApplyVFX += NPC_UpdateNPC_BuffApplyVFX;
            On.Terraria.NPC.UpdateCollision += NPC_UpdateCollision;

            IL.Terraria.Player.CollideWithNPCs += Player_CollideWithNPCs;
            IL.Terraria.Player.DashMovement += Player_DashMovement;
            IL.Terraria.Player.JumpMovement += Player_JumpMovement;
            IL.Terraria.Player.Update += Player_Update;
            IL.Terraria.NPC.StrikeNPC += NPC_StrikeNPC;
            IL.Terraria.GameContent.Shaders.WaterShaderData.DrawWaves += WaterShaderData_DrawWaves;
        }

        //A patch that fixes what I think is a bug in vanilla collision for things like zenith in which they don't collide when fully enclosed
        private bool Collision_CheckAABBvLineCollision_Vector2_Vector2_Vector2_Vector2_float_refSingle(On.Terraria.Collision.orig_CheckAABBvLineCollision_Vector2_Vector2_Vector2_Vector2_float_refSingle orig, Vector2 objectPosition, Vector2 objectDimensions, Vector2 lineStart, Vector2 lineEnd, float lineWidth, ref float collisionPoint)
        {
            if (orig(objectPosition, objectDimensions, lineStart, lineEnd, lineWidth, ref collisionPoint)) return true;
            else if (CustomCollision.CheckAABBvPoint(objectPosition, objectDimensions, lineStart)) { collisionPoint = 0f; return true; }
            else return false;
        }

        private void NPC_UpdateCollision(On.Terraria.NPC.orig_UpdateCollision orig, NPC self)
        {
            MultiHitboxNPC multiHitbox;
            if (self.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
            {
                if (multiHitbox.useMultipleHitboxes)
                {
                    multiHitbox.doDataReset = true;
                    Vector2 oldCenter = self.Center;
                    self.width = multiHitbox.widthForInteractions;
                    self.height = multiHitbox.heightForInteractions;
                    self.Center = oldCenter;
                }

                orig(self);

                if (multiHitbox.useMultipleHitboxes && multiHitbox.doDataReset)
                {
                    multiHitbox.doDataReset = false;
                    Vector2 oldCenter = self.Center;
                    self.width = multiHitbox.preModifyDataWidth;
                    self.height = multiHitbox.preModifyDataHeight;
                    self.Center = oldCenter;
                }
            }
            else
            {
                orig(self);
            }
        }

        private void NPC_UpdateNPC_BuffApplyVFX(On.Terraria.NPC.orig_UpdateNPC_BuffApplyVFX orig, NPC self)
        {
            orig(self);

            MultiHitboxNPC multiHitbox;
            if (self.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                if (multiHitbox.useMultipleHitboxes && multiHitbox.doDataReset)
                {
                    multiHitbox.doDataReset = false;
                    Vector2 oldCenter = self.Center;
                    self.width = multiHitbox.preModifyDataWidth;
                    self.height = multiHitbox.preModifyDataHeight;
                    self.Center = oldCenter;
                }
        }

        private void NPC_UpdateNPC_BuffSetFlags(On.Terraria.NPC.orig_UpdateNPC_BuffSetFlags orig, NPC self, bool lowerBuffTime)
        {
            MultiHitboxNPC multiHitbox;
            if (self.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                if (multiHitbox.useMultipleHitboxes)
                {
                    multiHitbox.doDataReset = true;
                    Vector2 oldCenter = self.Center;
                    self.width = multiHitbox.widthForInteractions;
                    self.height = multiHitbox.heightForInteractions;
                    self.Center = oldCenter;
                }

            orig(self, lowerBuffTime);
        }

        int preModifyDataWidth;
        int preModifyDataHeight;
        bool doDataReset;
        private void WaterShaderData_DrawWaves(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld(typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public)),
                i => i.MatchLdloc(7),
                i => i.MatchLdelemRef(),
                i => i.MatchStloc(8)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloc, 8);
            c.EmitDelegate<Action<NPC>>((npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    if (multiHitbox.useMultipleHitboxes)
                    {
                        multiHitbox.doDataReset = true;
                        Vector2 oldCenter = npc.Center;
                        npc.width = multiHitbox.widthForInteractions;
                        npc.height = multiHitbox.heightForInteractions;
                        npc.Center = oldCenter;
                    }
            });

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(7),
                i => i.MatchLdcI4(1),
                i => i.MatchAdd(),
                i => i.MatchStloc(7),
                i => i.MatchLdloc(7),
                i => i.MatchLdcI4(200),
                i => i.MatchBlt(out _)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.EmitDelegate<Action>(() =>
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc == null || !npc.active) continue;
                    MultiHitboxNPC multiHitbox;
                    if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                        if (multiHitbox.useMultipleHitboxes && multiHitbox.doDataReset)
                        {
                            multiHitbox.doDataReset = false;
                            Vector2 oldCenter = npc.Center;
                            npc.width = multiHitbox.preModifyDataWidth;
                            npc.height = multiHitbox.preModifyDataHeight;
                            npc.Center = oldCenter;
                        }
                }
            });
        }

        private void NPC_StrikeNPC(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdflda(typeof(Entity).GetField("position", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdfld(typeof(Vector2).GetField("X", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchConvI4(),
                i => i.MatchLdarg(0),
                i => i.MatchLdflda(typeof(Entity).GetField("position", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdfld(typeof(Vector2).GetField("Y", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchConvI4(),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Entity).GetField("width", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Entity).GetField("height", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchNewobj(typeof(Rectangle).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })),
                i => i.MatchLdloc(3),
                i => i.MatchLdloc(1),
                i => i.MatchConvI4(),
                i => i.MatchLdarg(4),
                i => i.MatchLdcI4(0),
                i => i.MatchCall(typeof(CombatText).GetMethod("NewText", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(Rectangle), typeof(Color), typeof(int), typeof(bool), typeof(bool) })),
                i => i.MatchPop()
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Index += 13;
            ILLabel label = c.DefineLabel();
            label.Target = c.Next;
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<NPC, bool>>((npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return multiHitbox.useMultipleHitboxes;
                return false;
            });
            c.Emit(OpCodes.Brfalse, label);
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<NPC, Rectangle>>((npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return multiHitbox.MostRecentHitbox();
                return npc.Hitbox;
            });

            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdflda(typeof(Entity).GetField("position", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdfld(typeof(Vector2).GetField("X", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchConvI4(),
                i => i.MatchLdarg(0),
                i => i.MatchLdflda(typeof(Entity).GetField("position", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdfld(typeof(Vector2).GetField("Y", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchConvI4(),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Entity).GetField("width", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Entity).GetField("height", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchNewobj(typeof(Rectangle).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })),
                i => i.MatchLdloc(4),
                i => i.MatchLdloc(1),
                i => i.MatchConvI4(),
                i => i.MatchLdarg(4),
                i => i.MatchLdcI4(0),
                i => i.MatchCall(typeof(CombatText).GetMethod("NewText", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(Rectangle), typeof(Color), typeof(int), typeof(bool), typeof(bool) })),
                i => i.MatchPop()
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Index += 13;
            ILLabel label2 = c.DefineLabel();
            label2.Target = c.Next;
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<NPC, bool>>((npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return multiHitbox.useMultipleHitboxes;
                return false;
            });
            c.Emit(OpCodes.Brfalse, label2);
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<NPC, Rectangle>>((npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return multiHitbox.MostRecentHitbox();
                return npc.Hitbox;
            });
        }

        //ramming mounts + lawnmower
        private void Player_CollideWithNPCs(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(2),
                i => i.MatchCallvirt(typeof(NPC).GetMethod("getRect", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchStloc(3),
                i => i.MatchLdarga(1),
                i => i.MatchLdloc(3),
                i => i.MatchCall(typeof(Rectangle).GetMethod("Intersects", BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Rectangle) })),
                i => i.MatchBrfalse(out label)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldarg, 1);
            c.Emit(OpCodes.Ldloc, 2);
            c.EmitDelegate<Func<Rectangle, NPC, bool>>((playerHitbox, npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return !multiHitbox.useMultipleHitboxes || multiHitbox.CollideRectangle(playerHitbox);
                return true;
            });
            c.Emit(OpCodes.Brfalse, label);
        }

        //minecart collision
        private void Player_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchConvI4(),
                i => i.MatchLdsfld(typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public)),
                i => i.MatchLdloc(145),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(Entity).GetField("width", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchLdsfld(typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public)),
                i => i.MatchLdloc(145),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(Entity).GetField("height", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchNewobj(typeof(Rectangle).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })),
                i => i.MatchCall(typeof(Rectangle).GetMethod("Intersects", BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Rectangle) })),
                i => i.MatchBrfalse(out label)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloc, 144);
            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
            c.Emit(OpCodes.Ldloc, 145);
            c.Emit(OpCodes.Ldelem_Ref);
            c.EmitDelegate<Func<Rectangle, NPC, bool>>((cartHitbox, npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return !multiHitbox.useMultipleHitboxes || multiHitbox.CollideRectangle(cartHitbox);
                return true;
            });
            c.Emit(OpCodes.Brfalse, label);
        }

        //lava collision with custom hitboxes
        private bool NPC_Collision_LavaCollision(On.Terraria.NPC.orig_Collision_LavaCollision orig, NPC self)
        {
            MultiHitboxNPC multiHitbox;
            if (self.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                if (multiHitbox.useMultipleHitboxes)
                {
                    for (int i = 0; i < multiHitbox.hitboxes.Length; i++)
                    {
                        Rectangle hitbox = multiHitbox.hitboxes[i];
                        if (Collision.LavaCollision(hitbox.TopLeft(), hitbox.Width, hitbox.Height))
                        {
                            multiHitbox.UpdateToSegment(i);
                            return orig(self);
                        }
                    }
                    return false;
                }
            return orig(self);
        }

        //slime/qs mount and golf cart apparently
        private void Player_JumpMovement(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(2),
                i => i.MatchCallvirt(typeof(NPC).GetMethod("getRect", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchStloc(3),
                i => i.MatchLdloca(0),
                i => i.MatchLdloc(3),
                i => i.MatchCall(typeof(Rectangle).GetMethod("Intersects", BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Rectangle) })),
                i => i.MatchBrfalse(out label)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloc, 0);
            c.Emit(OpCodes.Ldloc, 2);
            c.EmitDelegate<Func<Rectangle, NPC, bool>>((jumpHitbox, npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return !multiHitbox.useMultipleHitboxes || multiHitbox.CollideRectangle(jumpHitbox);
                return true;
            });
            c.Emit(OpCodes.Brfalse, label);

            ILLabel label2 = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(10),
                i => i.MatchCallvirt(typeof(NPC).GetMethod("getRect", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchStloc(11),
                i => i.MatchLdloca(8),
                i => i.MatchLdloc(11),
                i => i.MatchCall(typeof(Rectangle).GetMethod("Intersects", BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Rectangle) })),
                i => i.MatchBrfalse(out label2)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloc, 8);
            c.Emit(OpCodes.Ldloc, 10);
            c.EmitDelegate<Func<Rectangle, NPC, bool>>((jumpHitbox, npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return !multiHitbox.useMultipleHitboxes || multiHitbox.CollideRectangle(jumpHitbox);
                return true;
            });
            c.Emit(OpCodes.Brfalse, label2);
        }

        //make bonks respect custom hitboxes
        private void Player_DashMovement(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(2),
                i => i.MatchCallvirt(typeof(NPC).GetMethod("getRect", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchStloc(3),
                i => i.MatchLdloca(0),
                i => i.MatchLdloc(3),
                i => i.MatchCall(typeof(Rectangle).GetMethod("Intersects", BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Rectangle) })),
                i => i.MatchBrfalse(out label)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloc, 0);
            c.Emit(OpCodes.Ldloc, 2);
            c.EmitDelegate<Func<Rectangle, NPC, bool>>((dashHitbox, npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return !multiHitbox.useMultipleHitboxes || multiHitbox.CollideRectangle(dashHitbox);
                return true;
            });
            c.Emit(OpCodes.Brfalse, label);

            ILLabel label2 = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(11),
                i => i.MatchCallvirt(typeof(NPC).GetMethod("getRect", BindingFlags.Instance | BindingFlags.Public)),
                i => i.MatchStloc(12),
                i => i.MatchLdloca(9),
                i => i.MatchLdloc(12),
                i => i.MatchCall(typeof(Rectangle).GetMethod("Intersects", BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Rectangle) })),
                i => i.MatchBrfalse(out label2)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloc, 9);
            c.Emit(OpCodes.Ldloc, 11);
            c.EmitDelegate<Func<Rectangle, NPC, bool>>((dashHitbox, npc) =>
            {
                MultiHitboxNPC multiHitbox;
                if (npc.TryGetGlobalNPC<MultiHitboxNPC>(out multiHitbox))
                    return !multiHitbox.useMultipleHitboxes || multiHitbox.CollideRectangle(dashHitbox);
                return true;
            });
            c.Emit(OpCodes.Brfalse, label2);
        }

        static Rectangle rememberItemRectangle;
        private void Player_ItemCheck_MeleeHitNPCs(On.Terraria.Player.orig_ItemCheck_MeleeHitNPCs orig, Player self, Item sItem, Rectangle itemRectangle, int originalDamage, float knockBack)
        {
            rememberItemRectangle = itemRectangle;
            orig(self, sItem, itemRectangle, originalDamage, knockBack);
        }

        public Rectangle MostRecentHitbox()
        {
            return hitboxes[lastHitSegment];
        }

        public Action<int> SegmentUpdate;
        public void UpdateToSegment(int i)
        {
            lastHitSegment = i;
            if (SegmentUpdate != null)
            {
                SegmentUpdate.Invoke(i);
            }
        }

        public bool CollideRectangle(Rectangle collider)
        {
            for (int i = 0; i < hitboxes.Length; i++)
            {
                Rectangle hitbox = hitboxes[i];
                if (collider.Intersects(hitbox))
                {
                    UpdateToSegment(i);
                    return true;
                }
            }
            return false;
        }

        public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
        {
            if (useMultipleHitboxes && !CollideRectangle(rememberItemRectangle)) return false;
            return null;
        }

        public bool CollideProjectile(Projectile projectile, Rectangle projectileHitbox)
        {
            for (int i = 0; i < hitboxes.Length; i++)
            {
                Rectangle hitbox = hitboxes[i];
                if (projectile.Colliding(projectileHitbox, hitbox))
                {
                    UpdateToSegment(i);
                    return true;
                }
            }
            return false;
        }

        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (useMultipleHitboxes && !CollideProjectile(projectile, projectile.Hitbox)) return false;
            return null;
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (useMultipleHitboxes && !CollideRectangle(target.Hitbox)) return false;
            return true;
        }

        public override bool? CanHitNPC(NPC npc, NPC target)
        {
            MultiHitboxNPC targetMultiHitbox;
            if (!target.TryGetGlobalNPC<MultiHitboxNPC>(out targetMultiHitbox)) return null;
            if (useMultipleHitboxes)
            {
                if (targetMultiHitbox.useMultipleHitboxes)
                {
                    for (int i = 0; i < targetMultiHitbox.hitboxes.Length; i++)
                    {
                        Rectangle hitbox = targetMultiHitbox.hitboxes[i];
                        if (CollideRectangle(hitbox))
                        {
                            targetMultiHitbox.UpdateToSegment(i);
                            return null;
                        }
                    }
                    return false;
                }
                if (!CollideRectangle(target.Hitbox)) return false;
                return null;
            }
            if (targetMultiHitbox.useMultipleHitboxes && !targetMultiHitbox.CollideRectangle(npc.Hitbox)) return false;
            return null;
        }

        public override void SetDefaults(NPC npc)
        {
            widthForInteractions = npc.width;
            heightForInteractions = npc.height;
        }

        public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (useMultipleHitboxes)
            {
                position = new Vector2(npc.Center.X, npc.Center.Y - heightForInteractions / 2 + npc.gfxOffY);
                if (Main.HealthBarDrawSettings == 1)
                {
                    position.Y += (float)heightForInteractions + 10f + Main.NPCAddHeight(npc);
                }
                else if (Main.HealthBarDrawSettings == 2)
                {
                    position.Y -= 24f + Main.NPCAddHeight(npc) / 2f;
                }
                return true;
            }
            return null;
        }

        public override bool PreAI(NPC npc)
        {
            if (useMultipleHitboxes)
            {
                Vector2 oldCenter = npc.Center;
                npc.width = widthForInteractions;
                npc.height = heightForInteractions;
                npc.Center = oldCenter;
            }

            return true;
        }

        public override void PostAI(NPC npc)
        {
            if (useMultipleHitboxes)
            {
                //update width and height for special interactions before we reset the hitbox
                widthForInteractions = npc.width;
                heightForInteractions = npc.height;

                //update hitbox to cover all small ones while keeping the same center
                lastHitSegment = -1;

                float left = npc.Center.X;
                float right = npc.Center.X;
                float top = npc.Center.Y;
                float bottom = npc.Center.Y;
                foreach (Rectangle hitbox in hitboxes)
                {
                    if (hitbox.Left < left) left = hitbox.Left;
                    if (hitbox.Right > right) right = hitbox.Right;
                    if (hitbox.Top < top) top = hitbox.Top;
                    if (hitbox.Bottom > bottom) bottom = hitbox.Bottom;
                }

                int inflateX = (int)Math.Ceiling(Math.Max(npc.Center.X - left, right - npc.Center.X));
                int inflateY = (int)Math.Ceiling(Math.Max(npc.Center.Y - top, bottom - npc.Center.Y));

                npc.position = npc.Center - new Vector2(inflateX, inflateY);
                npc.width = 2 * inflateX;
                npc.height = 2 * inflateY;

                //store hitbox width and height data to be restored after special interactions
                preModifyDataWidth = npc.width;
                preModifyDataHeight = npc.height;
            }
        }

        //hitbox-drawing method for debugging
        /*public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (useMultipleHitboxes)
            {
                foreach (Rectangle hitbox in hitboxes)
                {
                    spriteBatch.Draw(Textures.PixelTexture.Value, new Rectangle(hitbox.X - (int)screenPos.X, hitbox.Y - (int)screenPos.Y, hitbox.Width, hitbox.Height), Color.Red * 0.5f);
                }
            }
        }*/
    }
}

