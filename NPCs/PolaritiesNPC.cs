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
using Polarities.Items.Books;
using MonoMod.RuntimeDetour.HookGen;
using Polarities.Biomes;
using Polarities.Items.Accessories;
using Polarities.Items.Weapons.Ranged;
using Polarities.Items.Weapons.Summon.Orbs;
using Polarities.Projectiles;
using Terraria.Audio;
using Polarities.Items.Weapons.Magic;
using Polarities.Buffs;
using Polarities.Items.Weapons.Melee;
using Polarities.Items.Weapons.Ranged.Atlatls;
using Polarities.Items.Armor.MechaMayhemArmor;
using Polarities.Items.Materials;

namespace Polarities.NPCs
{
    public enum NPCCapSlotID
    {
        HallowInvasion,
        WorldEvilInvasion,
        WorldEvilInvasionWorm,
    }

    public class PolaritiesNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public Dictionary<int, int> hammerTimes;
        public bool flawless = true;

        public bool usesProjectileHitCooldowns = false;
        public int projectileHitCooldownTime = 0;
        public int ignoredDefenseFromCritAmount;

        public float defenseMultiplier;

        public int tentacleClubs;
        public int chlorophyteDarts;
        public int contagunPhages;
        public int boneShards;

        public int desiccation;
        public int incineration;
        public bool coneVenom;

        public bool spiritBite;
        public int spiritBiteLevel;

        //vanilla NPC.takenDamageMultiplier doesn't have any effect when less than 1, so we do this instead
        public float neutralTakenDamageMultiplier = 1f;

        public static Dictionary<int, bool> bestiaryCritter = new Dictionary<int, bool>();

        public static Dictionary<int, int> npcTypeCap = new Dictionary<int, int>();

        public static Dictionary<int, NPCCapSlotID> customNPCCapSlot = new Dictionary<int, NPCCapSlotID>();
        public static Dictionary<NPCCapSlotID, float> customNPCCapSlotCaps = new Dictionary<NPCCapSlotID, float>
        {
            [NPCCapSlotID.HallowInvasion] = 6f,
            [NPCCapSlotID.WorldEvilInvasion] = 2f,
            [NPCCapSlotID.WorldEvilInvasionWorm] = 2f,
        };

        public static HashSet<int> customSlimes = new HashSet<int>();
        public static HashSet<int> forceCountForRadar = new HashSet<int>();
        public static HashSet<int> canSpawnInLava = new HashSet<int>();

        public override void Load()
        {
            On.Terraria.NPC.GetNPCColorTintedByBuffs += NPC_GetNPCColorTintedByBuffs;

            IL.Terraria.NPC.StrikeNPC += NPC_StrikeNPC;

            //counts weird critters
            IL.Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.AddEmptyEntries_CrittersAndEnemies_Automated += BestiaryDatabaseNPCsPopulator_AddEmptyEntries_CrittersAndEnemies_Automated;
            IL.Terraria.GameContent.Bestiary.NPCWasNearPlayerTracker.ScanWorldForFinds += NPCWasNearPlayerTracker_ScanWorldForFinds;
            On.Terraria.NPC.HittableForOnHitRewards += NPC_HittableForOnHitRewards;

            //avoid bad spawns
            IL_ChooseSpawn += PolaritiesNPC_IL_ChooseSpawn;

            //flawless continuity for EoW
            On.Terraria.NPC.Transform += NPC_Transform;

            //force counts things for the radar
            IL.Terraria.Main.DrawInfoAccs += Main_DrawInfoAccs;

            //allows npcs to spawn in lava
            IL.Terraria.NPC.SpawnNPC += NPC_SpawnNPC;
        }

        public override void Unload()
        {
            bestiaryCritter = null;
            customNPCCapSlot = null;
            customNPCCapSlotCaps = null;
            customSlimes = null;
            forceCountForRadar = null;
            canSpawnInLava = null;

            IL_ChooseSpawn -= PolaritiesNPC_IL_ChooseSpawn;
        }

        public override void SetDefaults(NPC npc)
        {
            hammerTimes = new Dictionary<int, int>();

            switch (npc.type)
            {
                case NPCID.DungeonGuardian:
                    npc.buffImmune[BuffType<Incinerating>()] = true;
                    break;
            }
        }

        private void Main_DrawInfoAccs(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld(typeof(Main).GetField("npc", BindingFlags.Public | BindingFlags.Static)),
                i => i.MatchLdloc(38),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(Entity).GetField("active", BindingFlags.Public | BindingFlags.Instance)),
                i => i.MatchBrfalse(out _),
                i => i.MatchLdsfld(typeof(Main).GetField("npc", BindingFlags.Public | BindingFlags.Static)),
                i => i.MatchLdloc(38),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(NPC).GetField("friendly", BindingFlags.Public | BindingFlags.Instance)),
                i => i.MatchBrtrue(out _),
                i => i.MatchLdsfld(typeof(Main).GetField("npc", BindingFlags.Public | BindingFlags.Static)),
                i => i.MatchLdloc(38),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(NPC).GetField("damage", BindingFlags.Public | BindingFlags.Instance)),
                i => i.MatchLdcI4(0),
                i => i.MatchBle(out _),
                i => i.MatchLdsfld(typeof(Main).GetField("npc", BindingFlags.Public | BindingFlags.Static)),
                i => i.MatchLdloc(38),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(NPC).GetField("lifeMax", BindingFlags.Public | BindingFlags.Instance)),
                i => i.MatchLdcI4(5),
                i => i.MatchBle(out _)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            ILLabel label = c.DefineLabel();
            label.Target = c.Next;

            c.Index -= 17;

            c.Emit(OpCodes.Ldloc, 38);
            c.EmitDelegate<Func<int, bool>>((index) =>
            {
                //return true to force counting
                return forceCountForRadar.Contains(Main.npc[index].type);
            });
            c.Emit(OpCodes.Brtrue, label);
        }

        //modifies enemy spawn pool after other mods for extra compatibility
        private static event ILContext.Manipulator IL_ChooseSpawn
        {
            add => HookEndpointManager.Modify(typeof(NPCLoader).GetMethod("ChooseSpawn", BindingFlags.Public | BindingFlags.Static), value);
            remove => HookEndpointManager.Unmodify(typeof(NPCLoader).GetMethod("ChooseSpawn", BindingFlags.Public | BindingFlags.Static), value);
        }

        public static bool lavaSpawnFlag;

        private void NPC_SpawnNPC(ILContext il)
        {
            //allows npcs to spawn in lava
            var c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(0),
                i => i.MatchStloc(55),
                i => i.MatchBr(out _)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location 1");
                return;
            }

            c.Index += 10;

            c.EmitDelegate<Action>(() => { lavaSpawnFlag = false; });

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchStloc(2),
                i => i.MatchBr(out _),
                //we shouldn't need any of the stuff before here but tile method matching is awful
                i => i.MatchLdsflda(typeof(Main).GetField("tile", BindingFlags.Public | BindingFlags.Static)),
                i => i.MatchLdloc(63),
                i => i.MatchLdloc(64),
                i => i.MatchCall(typeof(Tilemap).GetProperty("Item", new Type[] { typeof(int), typeof(int) }).GetGetMethod()),
                i => i.MatchStloc(41),
                i => i.MatchLdloca(41),
                i => i.MatchCall(out _), //this SHOULD be able to just be typeof(Tile).GetMethod("lava", new Type[] { }), but NO
                i => i.MatchBrfalse(out label)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location 2");
                return;
            }

            //we're in lava
            c.EmitDelegate<Action>(() => { lavaSpawnFlag = true; });
            //go to label
            c.Emit(OpCodes.Br, label);
        }

        private void PolaritiesNPC_IL_ChooseSpawn(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(0),
                i => i.MatchLdcI4(0),
                i => i.MatchLdcR4(1),
                i => i.MatchCallvirt(typeof(IDictionary<int, float>).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance).GetSetMethod())
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location 1");
                return;
            }

            //remove vanilla spawns if conditions are met
            c.Emit(OpCodes.Ldloc, 0);
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Action<IDictionary<int, float>, NPCSpawnInfo>>((pool, spawnInfo) =>
            {
                //don't remove vanilla spawns from pillars
                if (spawnInfo.Player.ZoneTowerSolar || spawnInfo.Player.ZoneTowerStardust || spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerVortex) return;

                //remove vanilla spawns if in pestilence/rapture
                if (spawnInfo.Player.InModBiome(GetInstance<HallowInvasion>()) || spawnInfo.Player.InModBiome(GetInstance<HallowInvasion>()))
                {
                    pool.Remove(0);
                }
            });

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchBleUn(out label),
                i => i.MatchLdloc(0),
                i => i.MatchLdloc(4),
                i => i.MatchCallvirt(typeof(ModNPC).GetProperty("NPC", BindingFlags.Public | BindingFlags.Instance).GetGetMethod()),
                i => i.MatchLdfld(typeof(NPC).GetField("type", BindingFlags.Public | BindingFlags.Instance)),
                i => i.MatchLdloc(5),
                i => i.MatchCallvirt(typeof(IDictionary<int, float>).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance).GetSetMethod())
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location 2");
                return;
            }

            c.Index++;

            c.Emit(OpCodes.Ldloc, 0);
            c.Emit(OpCodes.Ldarg, 0);
            c.Emit(OpCodes.Ldloc, 4);
            c.EmitDelegate<Func<IDictionary<int, float>, NPCSpawnInfo, ModNPC, bool>>((pool, spawnInfo, modNPC) =>
            {
                //return true to use normal spawn pool finding code, false to use custom code
                if (spawnInfo.Player.ZoneTowerSolar || spawnInfo.Player.ZoneTowerStardust || spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerVortex)
                {
                    if (modNPC.Mod == Mod)
                    {
                        return false;
                    }
                }
                else
                {
                    if (spawnInfo.Player.InModBiome(GetInstance<HallowInvasion>()))
                    {
                        //purge invalid rapture spawns
                        if (!HallowInvasion.ValidNPC(modNPC.Type))
                        {
                            return false;
                        }
                    }
                    else if (spawnInfo.Player.InModBiome(GetInstance<WorldEvilInvasion>()))
                    {
                        //purge invalid world evil enemy spawns
                        if (!WorldEvilInvasion.ValidNPC(modNPC.Type))
                        {
                            return false;
                        }
                    }

                    if (lavaSpawnFlag)
                    {
                        //purge invalid lava spawns
                        if (!canSpawnInLava.Contains(modNPC.Type))
                        {
                            return false;
                        }
                    }
                }
                return true;
            });
            c.Emit(OpCodes.Brfalse, label);

            //replace vanilla spawns with null if in lava
            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdloc(12),
                i => i.MatchRet()
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location 3");
                return;
            }

            c.Index++;

            c.EmitDelegate<Func<int?, int?>>((type) =>
            {
                return type != 0 ? type :
                    lavaSpawnFlag ? null : 0;
            });
        }

        public override void SpawnNPC(int npc, int tileX, int tileY)
        {
            //rapture enemies cannot spawn naturally if past their cap
            NPC realNPC = Main.npc[npc];
            if (npcTypeCap.ContainsKey(realNPC.type))
            {
                int npcsOfType = 0;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].type == realNPC.type)
                    {
                        npcsOfType++;
                        if (npcsOfType > npcTypeCap[realNPC.type])
                        {
                            realNPC.active = false;
                            return;
                        }
                    }
                }
            }
            if (customNPCCapSlot.ContainsKey(realNPC.type))
            {
                //count enemies
                float customNPCCapSlotCount = 0;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && customNPCCapSlot.ContainsKey(Main.npc[i].type) && customNPCCapSlot[Main.npc[i].type] == customNPCCapSlot[realNPC.type] && !Main.npc[i].dontCountMe)
                    {
                        customNPCCapSlotCount += Main.npc[i].npcSlots;
                    }
                }
                if (customNPCCapSlotCount > customNPCCapSlotCaps[customNPCCapSlot[realNPC.type]])
                {
                    realNPC.active = false;
                    return;
                }
            }
        }

        //true forces a bestiary critter, false forces not a critter, null uses vanilla logic
        static bool? IsBestiaryCritter(int npcType)
        {
            return bestiaryCritter.ContainsKey(npcType) ? bestiaryCritter[npcType] : null;
        }

        private void BestiaryDatabaseNPCsPopulator_AddEmptyEntries_CrittersAndEnemies_Automated(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloca(3),
                i => i.MatchCall(typeof(KeyValuePair<int, NPC>).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetGetMethod()),
                i => i.MatchCallvirt(typeof(NPC).GetProperty("CountsAsACritter", BindingFlags.Public | BindingFlags.Instance).GetGetMethod())
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloca, 3);
            c.Emit(OpCodes.Call, typeof(KeyValuePair<int, NPC>).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetGetMethod());
            c.EmitDelegate<Func<bool, NPC, bool>>((defaultCritterValue, npc) =>
            {
                return IsBestiaryCritter(npc.type) ?? defaultCritterValue;
            });
        }

        private void NPCWasNearPlayerTracker_ScanWorldForFinds(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(3),
                i => i.MatchCallvirt(typeof(NPC).GetProperty("CountsAsACritter", BindingFlags.Public | BindingFlags.Instance).GetGetMethod())
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloc, 3);
            c.EmitDelegate<Func<bool, NPC, bool>>((defaultCritterValue, npc) =>
            {
                return IsBestiaryCritter(npc.type) ?? defaultCritterValue;
            });
        }

        private bool NPC_HittableForOnHitRewards(On.Terraria.NPC.orig_HittableForOnHitRewards orig, NPC self)
        {
            if (IsBestiaryCritter(self.type) == true) return false;
            if (IsBestiaryCritter(self.type) == false && !self.immortal) return true;
            return orig(self);
        }

        private Color NPC_GetNPCColorTintedByBuffs(On.Terraria.NPC.orig_GetNPCColorTintedByBuffs orig, NPC self, Color npcColor)
        {
            npcColor = orig(self, npcColor);
            if (self.GetGlobalNPC<PolaritiesNPC>().hammerTimes.Count > 0)
            {
                npcColor = NPC.buffColor(npcColor, 0.6f, 0.6f, 0.6f, 1f);
            }
            return npcColor;
        }

        private void NPC_StrikeNPC(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //modify defense values
            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(NPC).GetField("defense", BindingFlags.Public | BindingFlags.Instance)),
                i => i.MatchStloc(2),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(NPC).GetField("ichor", BindingFlags.Public | BindingFlags.Instance)),
                i => i.MatchBrfalse(out _)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Index += 3;

            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<NPC, int>>((NPC npc) => {
                int defense = npc.defense;
                npc.GetGlobalNPC<PolaritiesNPC>().ModifyDefense(npc, ref defense);
                return defense;
            });
            c.Emit(OpCodes.Stloc, 2);

            //modify combat text color
            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(4),
                i => i.MatchBrtrue(out _),
                i => i.MatchLdsfld(typeof(CombatText).GetField("DamagedHostile", BindingFlags.Public | BindingFlags.Static)),
                i => i.MatchBr(out _),
                i => i.MatchLdsfld(typeof(CombatText).GetField("DamagedHostileCrit", BindingFlags.Public | BindingFlags.Static)),
                i => i.MatchStloc(4)
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldloc, 4);
            c.Emit(OpCodes.Ldarg, 4);
            c.EmitDelegate<Func<Color, bool, Color>>((defaultColor, crit) =>
            {
                if (Main.LocalPlayer.HasBuff(BuffType<EmpressOfLightBookBuff>()))
                {
                    if (crit)
                    {
                        return Main.DiscoColor;
                    }
                    //desaturated for non-crits
                    return Main.hslToRgb(Main.rgbToHsl(Main.DiscoColor) * new Vector3(1f, 0.25f, 1.5f));
                }
                return defaultColor;
            });
            c.Emit(OpCodes.Stloc, 4);
        }

        private void NPC_Transform(On.Terraria.NPC.orig_Transform orig, NPC self, int newType)
        {
            bool flawless = self.GetGlobalNPC<PolaritiesNPC>().flawless;
            Dictionary<int, int> hammerTimes = self.GetGlobalNPC<PolaritiesNPC>().hammerTimes;

            orig(self, newType);

            self.GetGlobalNPC<PolaritiesNPC>().flawless = flawless;
            self.GetGlobalNPC<PolaritiesNPC>().hammerTimes = hammerTimes;
        }

        public override void ResetEffects(NPC npc)
        {
            defenseMultiplier = 1f;

            List<int> removeKeys = new List<int>();
            foreach (int i in hammerTimes.Keys)
            {
                hammerTimes[i]--;
                if (hammerTimes[i] <= 0)
                {
                    removeKeys.Add(i);
                }
            }
            foreach (int i in removeKeys)
            {
                hammerTimes.Remove(i);
            }

            contagunPhages = 0;
            tentacleClubs = 0;
            chlorophyteDarts = 0;
            boneShards = 0;
            desiccation = 0;
            incineration = 0;
            coneVenom = false;

            if (!spiritBite)
            {
                spiritBiteLevel = 0;
            }
            spiritBite = false;
        }

        public override bool PreAI(NPC npc)
        {
            if (npc.HasBuff(BuffID.Slow) && npc.knockBackResist != 0f)
            {
                float slowingAmount = 2 * Math.Max(0, Math.Min(0.45f, npc.knockBackResist));
                npc.position -= npc.velocity * slowingAmount;
            }

            return true;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (projectile.IsTypeSummon())
            {
                damage += spiritBiteLevel;
            }
        }

        public void ModifyDefense(NPC npc, ref int defense)
        {
            defense = (int)(defense * defenseMultiplier);

            int hammerDefenseLoss = 0;
            foreach (int i in hammerTimes.Keys)
            {
                if (i > hammerDefenseLoss && hammerTimes[i] > 0) hammerDefenseLoss = i;
            }

            defense -= hammerDefenseLoss;

            defense -= ignoredDefenseFromCritAmount;

            defense -= boneShards;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!npc.buffImmune[BuffID.BoneJavelin])
            {
                if (tentacleClubs > 0)
                {
                    if (npc.lifeRegen > 0)
                    {
                        npc.lifeRegen = 0;
                    }
                    int amountLoss = tentacleClubs * 10;
                    npc.lifeRegen -= amountLoss * 2;
                    if (damage < amountLoss)
                    {
                        damage = amountLoss;
                    }
                }
                if (chlorophyteDarts > 0)
                {
                    if (npc.lifeRegen > 0)
                    {
                        npc.lifeRegen = 0;
                    }
                    int amountLoss = chlorophyteDarts * 24;
                    npc.lifeRegen -= amountLoss * 2;
                    if (damage < amountLoss)
                    {
                        damage = amountLoss;
                    }
                }
                if (contagunPhages > 0)
                {
                    if (npc.lifeRegen > 0)
                    {
                        npc.lifeRegen = 0;
                    }
                    int amountLoss = contagunPhages * 10;
                    npc.lifeRegen -= amountLoss * 2;
                    if (damage < amountLoss)
                    {
                        damage = amountLoss;
                    }
                }
                if (boneShards > 0)
                {
                    if (npc.lifeRegen > 0)
                    {
                        npc.lifeRegen = 0;
                    }
                    int amountLoss = boneShards * 2;
                    npc.lifeRegen -= amountLoss * 2;
                    if (damage < amountLoss)
                    {
                        damage = amountLoss;
                    }
                }
            }
            if (desiccation > 60 * 10)
            {
                npc.lifeRegen -= 60;
                if (damage < 5)
                {
                    damage = 5;
                }
            }
            if (coneVenom)
            {
                npc.lifeRegen -= 140;
                if (damage < 12)
                {
                    damage = 12;
                }
            }
            if (incineration > 0)
            {
                npc.lifeRegen -= incineration * 2;
                if (damage < incineration / 6)
                {
                    damage = incineration / 6;
                }
            }

            if (contagunPhages > 10)
            {
                SoundEngine.PlaySound(SoundID.Item17, npc.Center);
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ProjectileType<ContagunVirusProjectile>() && Main.projectile[i].ai[0] == npc.whoAmI + 1)
                    {
                        Main.projectile[i].ai[0] = 0;
                        Main.projectile[i].velocity = new Vector2(10, 0).RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 2f);

                        Projectile.NewProjectile(Main.projectile[i].GetSource_FromAI(), Main.projectile[i].Center, new Vector2(Main.rand.NextFloat(5f)).RotatedByRandom(MathHelper.TwoPi), ProjectileType<ContagunProjectile>(), Main.projectile[i].damage, Main.projectile[i].knockBack, Main.projectile[i].owner, ai0: Main.rand.NextFloat(MathHelper.TwoPi), ai1: 240f);
                    }
                }
            }

            UpdateCustomSoulDrain(npc);
        }

        private void UpdateCustomSoulDrain(NPC npc)
        {
            if (!npc.soulDrain)
            {
                return;
            }
            int num3 = 1100;
            for (int i = 0; i < 255; i++)
            {
                if (!Main.player[i].active || Main.player[i].dead)
                {
                    continue;
                }
                Vector2 val = npc.Center - Main.player[i].position;
                if (val.Length() < (float)num3 && Main.player[i].ownedProjectileCounts[ProjectileType<LifeGrasperAura>()] > 0)
                {
                    if (i == Main.myPlayer)
                    {
                        Main.player[i].soulDrain++;
                    }
                    if (!Main.rand.NextBool(3))
                    {
                        Vector2 center = npc.Center;
                        center.X += (float)Main.rand.Next(-100, 100) * 0.05f;
                        center.Y += (float)Main.rand.Next(-100, 100) * 0.05f;
                        center += npc.velocity;
                        int num2 = Dust.NewDust(center, 1, 1, DustID.LifeDrain);
                        Dust obj = Main.dust[num2];
                        obj.velocity *= 0f;
                        Main.dust[num2].scale = (float)Main.rand.Next(70, 85) * 0.01f;
                        Main.dust[num2].fadeIn = i + 1;
                    }
                }
            }
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.InModBiome(GetInstance<HallowInvasion>()))
            {
                spawnRate = (int)(spawnRate / 2);
            }

            spawnRate = (int)(spawnRate * player.GetModPlayer<PolaritiesPlayer>().spawnRate);

            //no enemy spawning while a boss is alive unless during pillars
            if (!player.ZoneTowerNebula && !player.ZoneTowerSolar && !player.ZoneTowerStardust && !player.ZoneTowerVortex)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].boss && Main.npc[i].active && Main.npc[i].type != NPCID.MartianSaucerCore)
                    {
                        maxSpawns = 0;
                        return;
                    }
                }
            }

            if (PolaritiesSystem.esophageSpawnTimer > 0 || PolaritiesSystem.sunPixieSpawnTimer > 0)
            {
                maxSpawns = 0;
                return;
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Polarities.customNPCGlowMasks.ContainsKey(npc.type))
            {
                float num246 = Main.NPCAddHeight(npc);
                SpriteEffects spriteEffects = (SpriteEffects)0;
                if (npc.spriteDirection == 1)
                {
                    spriteEffects = (SpriteEffects)1;
                }
                Vector2 halfSize = new Vector2((float)(Polarities.customNPCGlowMasks[npc.type].Width() / 2), (float)(Polarities.customNPCGlowMasks[npc.type].Height() / Main.npcFrameCount[npc.type] / 2));

                Color color = npc.GetAlpha(npc.GetNPCColorTintedByBuffs(Color.White));

                spriteBatch.Draw(Polarities.customNPCGlowMasks[npc.type].Value, npc.Bottom - screenPos + new Vector2((float)(-Polarities.customNPCGlowMasks[npc.type].Width()) * npc.scale / 2f + halfSize.X * npc.scale, (float)(-Polarities.customNPCGlowMasks[npc.type].Height()) * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + halfSize.Y * npc.scale + num246 + npc.gfxOffY), (Rectangle?)npc.frame, color, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
            }
        }

        public override bool? CanHitNPC(NPC npc, NPC target)
        {
            if (target.type == NPCType<NPCs.TownNPCs.Ghostwriter>() && !(npc.type == NPCID.Wraith || npc.type == NPCID.Ghost || npc.type == NPCID.Reaper || npc.type == NPCID.Poltergeist || npc.type == NPCID.DungeonSpirit))
            {
                return false;
            }
            return null;
        }

        public override void BuffTownNPC(ref float damageMult, ref int defense)
        {
            if (PolaritiesSystem.downedStormCloudfish)
            {
                damageMult += 0.1f;
                defense += 3;
            }
            if (PolaritiesSystem.downedStarConstruct)
            {
                damageMult += 0.1f;
                defense += 3;
            }
            if (PolaritiesSystem.downedGigabat)
            {
                damageMult += 0.1f;
                defense += 3;
            }
            if (PolaritiesSystem.downedRiftDenizen)
            {
                damageMult += 0.1f;
                defense += 3;
            }

            if (PolaritiesSystem.downedSunPixie)
            {
                damageMult += 0.15f;
                defense += 6;
            }
            if (PolaritiesSystem.downedEsophage)
            {
                damageMult += 0.15f;
                defense += 6;
            }
            if (PolaritiesSystem.downedSelfsimilarSentinel)
            {
                damageMult += 0.15f;
                defense += 6;
            }
            if (PolaritiesSystem.downedEclipxie)
            {
                damageMult += 0.15f;
                defense += 6;
            }
            if (PolaritiesSystem.downedHemorrphage)
            {
                damageMult += 0.15f;
                defense += 6;
            }

            if (PolaritiesSystem.downedPolarities)
            {
                damageMult += 0.15f;
                defense += 10;
            }
            if (NPC.downedMoonlord)
            {
                damageMult += 0.15f;
                defense += 10;
            }
        }

        public override void OnKill(NPC npc)
        {
            switch(npc.type)
            {
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsTail:
                    if (npc.boss)
                    {
                        if (!PolaritiesSystem.downedEaterOfWorlds)
                        {
                            PolaritiesSystem.downedEaterOfWorlds = true;
                        }
                    }
                    break;
                case NPCID.BrainofCthulhu:
                    if (!PolaritiesSystem.downedBrainOfCthulhu)
                    {
                        PolaritiesSystem.downedBrainOfCthulhu = true;
                    }
                    break;
            }
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (HallowInvasion.ValidNPC(npc.type))
            {
                npcLoot.Add(ItemDropRule.ByCondition(new SunPixieSummonItemDropCondition(), ItemType<SunPixieSummonItem>()));
            }
            if (WorldEvilInvasion.ValidNPC(npc.type))
            {
                npcLoot.Add(ItemDropRule.ByCondition(new EsophageSummonItemDropCondition(), ItemType<EsophageSummonItem>()));
            }

            if (customSlimes.Contains(npc.type))
            {
                npcLoot.Add(ItemDropRule.NormalvsExpert(ItemID.SlimeStaff, 10000, 7000));
            }

            switch(npc.type)
            {
                case NPCID.GraniteFlyer:
                case NPCID.GraniteGolem:
                    npcLoot.Add(ItemDropRule.Common(ItemType<BlueQuartz>(), 2, 1, 2));
                    break;

                //bosses (mostly flawless stuff)
                case NPCID.KingSlime:
                    npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Gelthrower>()));
                    break;
                case NPCID.EyeofCthulhu:
                    npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Eyeruption>()));
                    break;
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsTail:
                    //TODO: npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Items.Weapons.Magic.ConsumptionCannon>())); (this can't actually be quite this simple)
                    break;
                case NPCID.BrainofCthulhu:
                    //TODO: npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Items.Weapons.Melee.NeuralBasher>()));
                    break;
                case NPCID.QueenBee:
                    npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<RoyalOrb>()));
                    break;
                case NPCID.SkeletronHead:
                    npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Items.Weapons.Melee.Warhammers.BonyBackhand>()));
                    break;
                case NPCID.WallofFlesh:
                    npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<MawOfFlesh>()));
                    break;
                case NPCID.TheDestroyer:
                    npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<FlawlessMechTail>()));
                    break;
                case NPCID.Retinazer:
                case NPCID.Spazmatism:
                    {
                        LeadingConditionRule leadingConditionRule = new LeadingConditionRule(new Conditions.MissingTwin());
                        leadingConditionRule.OnSuccess(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<FlawlessMechMask>()));
                        npcLoot.Add(leadingConditionRule);
                    }
                    break;
                case NPCID.SkeletronPrime:
                    npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<FlawlessMechChestplate>()));
                    break;
                case NPCID.Plantera:
                    {
                        npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<UnfoldingBlossom>()));

                        LeadingConditionRule leadingConditionRule = new LeadingConditionRule(new Conditions.NotExpert());
                        leadingConditionRule.OnSuccess(ItemDropRule.Common(ItemType<JunglesRage>(), 4));
                    }
                    break;
                case NPCID.Everscream:
                    {
                        //adds the candy cane atlatl
                        List<IItemDropRule> everscreamLoot = npcLoot.Get(includeGlobalDrops: false);
                        for (int index = 0; index < everscreamLoot.Count; index++)
                        {
                            IItemDropRule rule = everscreamLoot[index];
                            if (rule is LeadingConditionRule leadingConditionRule && leadingConditionRule.condition is Conditions.FrostMoonDropGatingChance)
                            {
                                foreach (IItemDropRuleChainAttempt tryAttempt in rule.ChainedRules)
                                {
                                    if (tryAttempt is TryIfSucceeded chain && chain.RuleToChain is CommonDrop rule2)
                                    {
                                        foreach (IItemDropRuleChainAttempt tryAttempt2 in rule2.ChainedRules)
                                        {
                                            if (tryAttempt2 is TryIfFailedRandomRoll chain2 && chain2.RuleToChain is OneFromOptionsDropRule rule3)
                                            {
                                                Array.Resize(ref rule3.dropIds, rule3.dropIds.Length + 1);
                                                rule3.dropIds[rule3.dropIds.Length - 1] = ItemType<CandyCaneAtlatl>();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case NPCID.DD2Betsy:
                    npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<WyvernsNest>()));
                    break;
            }

            //replace trophies and master pets
            List<IItemDropRule> originalLoot = npcLoot.Get(includeGlobalDrops: false);
            for(int index = 0; index < originalLoot.Count; index++)
            {
                IItemDropRule rule = originalLoot[index];

                //why can we not just insert things
                void RuleReplace(IItemDropRule newRule)
                {
                    npcLoot.Add(newRule);
                    npcLoot.Remove(rule);

                    List<IItemDropRule> currentLoot = npcLoot.Get(includeGlobalDrops: false);

                    for (int i = index; i < currentLoot.Count - 1; i++)
                    {
                        npcLoot.Remove(currentLoot[i]);
                        npcLoot.Add(currentLoot[i]);
                    }
                }

                void ChainedRuleReplace(IItemDropRule baseRule, IItemDropRuleChainAttempt oldTryAttempt, IItemDropRule newRule)
                {
                    baseRule.ChainedRules.Remove(oldTryAttempt);
                    baseRule.OnSuccess(newRule);

                    IItemDropRuleChainAttempt[] currentLoot = new IItemDropRuleChainAttempt[baseRule.ChainedRules.Count];
                    baseRule.ChainedRules.CopyTo(currentLoot);

                    for (int i = index; i < currentLoot.Length - 1; i++)
                    {
                        baseRule.ChainedRules.Remove(currentLoot[i]);
                        baseRule.ChainedRules.Add(currentLoot[i]);
                    }
                }

                //in vanilla, Conditions.LegacyHack_IsABoss is used only for trophies and for the eater of worlds
                if (rule is ItemDropWithConditionRule trophyDrop && trophyDrop.condition is Conditions.LegacyHack_IsABoss && trophyDrop.chanceDenominator == 10 && trophyDrop.chanceNumerator == 1 && trophyDrop.amountDroppedMinimum == 1 && trophyDrop.amountDroppedMaximum == 1)
                {
                    //replace with a better trophy rule
                    RuleReplace(new FlawlessOrRandomDropRule(trophyDrop.itemId, 10, 1, 1, 1, new Conditions.LegacyHack_IsABoss()));
                }
                //in vanilla, DropBasedOnMasterMode is only used for master mode pets
                //This does not work for eater of worlds, the twins, and the pumpkin/frost moons, as their master mode pet drops have conditions, so we handle those separately
                else if (rule is DropBasedOnMasterMode masterPetDrop && masterPetDrop.ruleForDefault is DropNothing && masterPetDrop.ruleForMasterMode is DropPerPlayerOnThePlayer perPlayerDrop && perPlayerDrop.chanceDenominator == 4 && perPlayerDrop.chanceNumerator == 1 && perPlayerDrop.amountDroppedMaximum == 1 && perPlayerDrop.amountDroppedMinimum == 1)
                {
                    //replace with a better master pet rule
                    RuleReplace(ModUtils.MasterModeDropOnAllPlayersOrFlawless(perPlayerDrop.itemId, 4, 1, 1, 1));
                }
                //EoW master mode pet
                else if (npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsTail)
                {
                    if (rule is LeadingConditionRule leadingConditionRule && leadingConditionRule.condition is Conditions.LegacyHack_IsABoss)
                    {
                        List<IItemDropRuleChainAttempt> replaceRules = new List<IItemDropRuleChainAttempt>();
                        List<IItemDropRule> newRules = new List<IItemDropRule>();
                        foreach (IItemDropRuleChainAttempt tryAttempt in rule.ChainedRules)
                        {
                            if (tryAttempt is TryIfSucceeded rule2 && rule2.RuleToChain is DropBasedOnMasterMode masterPetDrop2 && masterPetDrop2.ruleForDefault is DropNothing && masterPetDrop2.ruleForMasterMode is DropPerPlayerOnThePlayer perPlayerDrop2 && perPlayerDrop2.chanceDenominator == 4 && perPlayerDrop2.chanceNumerator == 1 && perPlayerDrop2.amountDroppedMaximum == 1 && perPlayerDrop2.amountDroppedMinimum == 1)
                            {
                                //replace with a better master pet rule
                                newRules.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(perPlayerDrop2.itemId, 4, 1, 1, 1));
                                replaceRules.Add(tryAttempt);
                            }
                        }
                        for (int i = 0; i < replaceRules.Count; i++)
                        {
                            ChainedRuleReplace(rule, replaceRules[i], newRules[i]);
                        }
                    }
                }
                //Twins master mode pet
                else if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)
                {
                    if (rule is LeadingConditionRule leadingConditionRule && leadingConditionRule.condition is Conditions.MissingTwin)
                    {
                        List<IItemDropRuleChainAttempt> replaceRules = new List<IItemDropRuleChainAttempt>();
                        List<IItemDropRule> newRules = new List<IItemDropRule>();
                        foreach (IItemDropRuleChainAttempt tryAttempt in rule.ChainedRules)
                        {
                            if (tryAttempt is TryIfSucceeded rule2 && rule2.RuleToChain is DropBasedOnMasterMode masterPetDrop2 && masterPetDrop2.ruleForDefault is DropNothing && masterPetDrop2.ruleForMasterMode is DropPerPlayerOnThePlayer perPlayerDrop2 && perPlayerDrop2.chanceDenominator == 4 && perPlayerDrop2.chanceNumerator == 1 && perPlayerDrop2.amountDroppedMaximum == 1 && perPlayerDrop2.amountDroppedMinimum == 1)
                            {
                                //replace with a better master pet rule
                                newRules.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(perPlayerDrop2.itemId, 4, 1, 1, 1));
                                replaceRules.Add(tryAttempt);
                            }
                        }
                        for (int i = 0; i < replaceRules.Count; i++)
                        {
                            ChainedRuleReplace(rule, replaceRules[i], newRules[i]);
                        }
                    }
                }
                //Pumpkin moon master mode pet
                else if (npc.type == NPCID.MourningWood || npc.type == NPCID.Pumpking)
                {
                    if (rule is LeadingConditionRule leadingConditionRule && leadingConditionRule.condition is Conditions.PumpkinMoonDropGatingChance)
                    {
                        List<IItemDropRuleChainAttempt> replaceRules = new List<IItemDropRuleChainAttempt>();
                        List<IItemDropRule> newRules = new List<IItemDropRule>();
                        foreach (IItemDropRuleChainAttempt tryAttempt in rule.ChainedRules)
                        {
                            if (tryAttempt is TryIfSucceeded rule2 && rule2.RuleToChain is DropBasedOnMasterMode masterPetDrop2 && masterPetDrop2.ruleForDefault is DropNothing && masterPetDrop2.ruleForMasterMode is DropPerPlayerOnThePlayer perPlayerDrop2 && perPlayerDrop2.chanceDenominator == 4 && perPlayerDrop2.chanceNumerator == 1 && perPlayerDrop2.amountDroppedMaximum == 1 && perPlayerDrop2.amountDroppedMinimum == 1)
                            {
                                //replace with a better master pet rule
                                newRules.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(perPlayerDrop2.itemId, 4, 1, 1, 1));
                                replaceRules.Add(tryAttempt);
                            }
                        }
                        for (int i = 0; i < replaceRules.Count; i++)
                        {
                            ChainedRuleReplace(rule, replaceRules[i], newRules[i]);
                        }
                    }
                }
                //Frost moon master mode pet
                else if (npc.type == NPCID.Everscream || npc.type == NPCID.SantaNK1 || npc.type == NPCID.IceQueen)
                {
                    if (rule is LeadingConditionRule leadingConditionRule && leadingConditionRule.condition is Conditions.FrostMoonDropGatingChance)
                    {
                        List<IItemDropRuleChainAttempt> replaceRules = new List<IItemDropRuleChainAttempt>();
                        List<IItemDropRule> newRules = new List<IItemDropRule>();
                        foreach (IItemDropRuleChainAttempt tryAttempt in rule.ChainedRules)
                        {
                            if (tryAttempt is TryIfSucceeded rule2 && rule2.RuleToChain is DropBasedOnMasterMode masterPetDrop2 && masterPetDrop2.ruleForDefault is DropNothing && masterPetDrop2.ruleForMasterMode is DropPerPlayerOnThePlayer perPlayerDrop2 && perPlayerDrop2.chanceDenominator == 4 && perPlayerDrop2.chanceNumerator == 1 && perPlayerDrop2.amountDroppedMaximum == 1 && perPlayerDrop2.amountDroppedMinimum == 1)
                            {
                                //replace with a better master pet rule
                                newRules.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(perPlayerDrop2.itemId, 4, 1, 1, 1));
                                replaceRules.Add(tryAttempt);
                            }
                        }
                        for (int i = 0; i < replaceRules.Count; i++)
                        {
                            ChainedRuleReplace(rule, replaceRules[i], newRules[i]);
                        }
                    }
                }
            }
        }

        public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            damage *= neutralTakenDamageMultiplier;
            return true;
        }
    }
}

