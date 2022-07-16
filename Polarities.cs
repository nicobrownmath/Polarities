using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Polarities.Biomes;
using Polarities.NPCs;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities
{
	public class Polarities : Mod
	{
        public static Dictionary<int, int> customNPCBestiaryStars = new Dictionary<int, int>();
        public static Dictionary<int, Asset<Texture2D>> customTileGlowMasks = new Dictionary<int, Asset<Texture2D>>();
        public static Dictionary<int, Asset<Texture2D>> customNPCGlowMasks = new Dictionary<int, Asset<Texture2D>>();

        //pre-generated random data
        //the size is odd because we only ever move 4 steps along the data stream so this way we can loop 4 times without actually repeating
        public static PreGeneratedRandom preGeneratedRand = new PreGeneratedRandom(358297, 4095);

        public static ModKeybind ArmorSetBonusHotkey;

        public override void Load()
        {
            ModUtils.Load();

            //for handling custom NPC rarities
            On.Terraria.ID.ContentSamples.FillNpcRarities += ContentSamples_FillNpcRarities;

            //music edit
            On.Terraria.Main.UpdateAudio_DecideOnNewMusic += Main_UpdateAudio_DecideOnNewMusic;

            IL_ResizeArrays += Polarities_IL_ResizeArrays;

            //register hotkeys
            ArmorSetBonusHotkey = KeybindLoader.RegisterKeybind(this, "Convective Set Bonus", Keys.K);

            string texture = GetModNPC(ModContent.NPCType<NPCs.StormCloudfish.StormCloudfish>()).BossHeadTexture + "_2";
            AddBossHeadTexture(texture, -1);
            NPCs.StormCloudfish.StormCloudfish.secondStageHeadSlot = ModContent.GetModBossHeadSlot(texture);
        }

        public override void Unload()
        {
            ModUtils.Unload();

            customNPCBestiaryStars = null;
            customTileGlowMasks = null;
            customNPCGlowMasks = null;
            preGeneratedRand = null;

            //reset to vanilla size
            Array.Resize<Asset<Texture2D>>(ref TextureAssets.GlowMask, Main.maxGlowMasks);

            IL_ResizeArrays -= Polarities_IL_ResizeArrays;

            //unload hotkeys
            ArmorSetBonusHotkey = null;
        }

        private void Polarities_IL_ResizeArrays(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchCall(typeof(LoaderManager).GetMethod("ResizeArrays", BindingFlags.Static | BindingFlags.NonPublic))
                ))
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Action<bool>>((unloading) =>
            {
                //Currently unused, anything put in here will run after ResizeArrays
                //Potentially useful for loading, to change the size of vanilla arrays or arrays from the mod
            });
        }

        //I should not need to do this in order to have something run after ResizeArrays
        private static event ILContext.Manipulator IL_ResizeArrays
        {
            add => HookEndpointManager.Modify(typeof(ModContent).GetMethod("ResizeArrays", BindingFlags.NonPublic | BindingFlags.Static), value);
            remove => HookEndpointManager.Unmodify(typeof(ModContent).GetMethod("ResizeArrays", BindingFlags.NonPublic | BindingFlags.Static), value);
        }

        public override void PostSetupContent()
        {
            short maskIndex = (short)TextureAssets.GlowMask.Length;

            Array.Resize<Asset<Texture2D>>(ref TextureAssets.GlowMask, TextureAssets.GlowMask.Length + customTileGlowMasks.Count);

            foreach (int type in customTileGlowMasks.Keys)
            {
                Main.tileGlowMask[type] = maskIndex;
                TextureAssets.GlowMask[maskIndex] = customTileGlowMasks[type];
                maskIndex++;
            }
        }

        private void ContentSamples_FillNpcRarities(On.Terraria.ID.ContentSamples.orig_FillNpcRarities orig)
        {
            orig();
            foreach (int type in customNPCBestiaryStars.Keys)
            {
                ContentSamples.NpcBestiaryRarityStars[type] = customNPCBestiaryStars[type];
            }
        }

        private void Main_UpdateAudio_DecideOnNewMusic(On.Terraria.Main.orig_UpdateAudio_DecideOnNewMusic orig, Main self)
        {
            orig(self);

            if (PolaritiesSystem.sunPixieSpawnTimer > 300)
            {
                Main.newMusic = 0;
                Main.musicFade[Main.curMusic] = 0f;
            }
            else if (PolaritiesSystem.sunPixieSpawnTimer > 0)
            {
                Main.newMusic = GetInstance<HallowInvasion>().Music;
                Main.musicFade[Main.curMusic] -= 1 / 120f;
            }
            else if (PolaritiesSystem.sunPixieSpawnTimer < 0)
            {
                Main.musicFade[Main.curMusic] += 1 / 10f;
            }

            if (PolaritiesSystem.esophageSpawnTimer > 300)
            {
                Main.newMusic = 0;
                Main.musicFade[Main.curMusic] = 0f;
            }
            else if (PolaritiesSystem.esophageSpawnTimer > 0)
            {
                Main.newMusic = GetInstance<WorldEvilInvasion>().Music;
                Main.musicFade[Main.curMusic] -= 1 / 120f;
            }
            else if (PolaritiesSystem.esophageSpawnTimer < 0)
            {
                Main.musicFade[Main.curMusic] += 1 / 10f;
            }
        }

        public override void AddRecipeGroups()
        {
            RecipeGroup.RegisterGroup(
                "Polarities:WoodenAtlatl",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ModContent.ItemType<Items.Weapons.Ranged.Atlatls.WoodenAtlatl>())}",
                    ModContent.ItemType<Items.Weapons.Ranged.Atlatls.WoodenAtlatl>(),
                    ModContent.ItemType<Items.Weapons.Ranged.Atlatls.BorealWoodAtlatl>(),
                    ModContent.ItemType<Items.Weapons.Ranged.Atlatls.PalmWoodAtlatl>(),
                    ModContent.ItemType<Items.Weapons.Ranged.Atlatls.MahoganyAtlatl>(),
                    ModContent.ItemType<Items.Weapons.Ranged.Atlatls.EbonwoodAtlatl>(),
                    ModContent.ItemType<Items.Weapons.Ranged.Atlatls.ShadewoodAtlatl>(),
                    ModContent.ItemType<Items.Weapons.Ranged.Atlatls.PearlwoodAtlatl>()
                ));

            RecipeGroup.RegisterGroup(
                "Polarities:SilverBar",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}",
                    ItemID.SilverBar,
                    ItemID.TungstenBar
                ));

            RecipeGroup.RegisterGroup(
                "Polarities:AdamantiteBar",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.AdamantiteBar)}",
                    ItemID.AdamantiteBar,
                    ItemID.TitaniumBar
                ));

            RecipeGroup.RegisterGroup(
                "Polarities:RottenChunk",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.RottenChunk)}",
                    ItemID.RottenChunk,
                    ItemID.Vertebrae
                ));

            RecipeGroup.RegisterGroup(
                "Polarities:ShadowScale",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.ShadowScale)}",
                    ItemID.ShadowScale,
                    ItemID.TissueSample
                ));
        }

        //TODO: Do more compatiblity options
        //TODO: Boss checklist entries
        //TODO: Fargo's compatibility (this basically just needs the event summons)
        public override object Call(params object[] args)
        {
            switch (args[0])
            {
                //TODO: Check that this is up to date
                case "AbominationnClearEvents":
                    //fargo's mod call for event clearing
                    bool eventOccurring = PolaritiesSystem.worldEvilInvasion || PolaritiesSystem.hallowInvasion;
                    bool canClearEvents = Convert.ToBoolean(args[1]);
                    if (eventOccurring && canClearEvents)
                    {
                        PolaritiesSystem.worldEvilInvasion = false;
                        PolaritiesSystem.hallowInvasion = false;
                    }
                    return eventOccurring;
                case "InFractalDimension":
                    return false;//TODO: Subworld.IsActive<FractalSubworld>();
                //TODO: Check if it's the rapture/pestilence, activate/deactivate them
            }
            return null;
        }
    }
}