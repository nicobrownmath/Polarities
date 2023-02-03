using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Polarities.Biomes;
using Polarities.Items;
using Polarities.Items.Armor.Vanity;
using Polarities.Items.Pets;
using Polarities.Items.Placeable.Relics;
using Polarities.Items.Placeable.Trophies;
using Polarities.Items.Weapons.Magic;
using Polarities.Items.Weapons.Melee;
using Polarities.Items.Weapons.Ranged;
using Polarities.Items.Weapons.Summon.Minions;
using Polarities.NPCs;
using Polarities.NPCs.Critters;
using Polarities.NPCs.ConvectiveWanderer;
using Polarities.NPCs.Esophage;
using Polarities.NPCs.Gigabat;
using Polarities.NPCs.StarConstruct;
using Polarities.NPCs.StormCloudfish;
using Polarities.NPCs.SunPixie;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.NPCs.Enemies.HallowInvasion;
using Polarities.NPCs.Enemies.WorldEvilInvasion;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Polarities.NPCs.Eclipxie;
using Terraria.Audio;
using Polarities.Items.Placeable.MusicBoxes;

namespace Polarities
{
    public class Polarities : Mod
	{
        public static bool AprilFools => (DateTime.Now.Day == 1) && (DateTime.Now.Month == 4);
        public static bool SnakeDay => (DateTime.Now.Day == 16) && (DateTime.Now.Month == 7);

        public static Dictionary<int, int> customNPCBestiaryStars = new Dictionary<int, int>();
        public static Dictionary<int, Asset<Texture2D>> customTileGlowMasks = new Dictionary<int, Asset<Texture2D>>();
        public static Dictionary<int, Asset<Texture2D>> customNPCGlowMasks = new Dictionary<int, Asset<Texture2D>>();

        //pre-generated random data
        //the size is odd because we only ever move 4 steps along the data stream so this way we can loop 4 times without actually repeating
        public static PreGeneratedRandom preGeneratedRand = new PreGeneratedRandom(358297, 4095);

        public static ModKeybind ArmorSetBonusHotkey;
        public static ModKeybind ConvectiveDashHotkey;

        public const string CallShootProjectile = "Polarities/Projectiles/CallShootProjectile";

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
            ConvectiveDashHotkey = KeybindLoader.RegisterKeybind(this, "Convective Dash", Keys.I);

            string texture = GetModNPC(ModContent.NPCType<NPCs.StormCloudfish.StormCloudfish>()).BossHeadTexture + "_2";
            AddBossHeadTexture(texture, -1);
            NPCs.StormCloudfish.StormCloudfish.secondStageHeadSlot = ModContent.GetModBossHeadSlot(texture);

            //shaders
            Ref<Effect> miscEffectsRef = new Ref<Effect>(Assets.Request<Effect>("Effects/MiscEffects", AssetRequestMode.ImmediateLoad).Value);
            Ref<Effect> filtersRef = new Ref<Effect>(Assets.Request<Effect>("Effects/Filters", AssetRequestMode.ImmediateLoad).Value);

            Filters.Scene["Polarities:ScreenWarp"] = new Filter(new ScreenShaderData(filtersRef, "ScreenWarpPass"), EffectPriority.VeryHigh);
            Filters.Scene["Polarities:ScreenWarp"].Load();

            GameShaders.Misc["Polarities:TriangleFade"] = new MiscShaderData(miscEffectsRef, "TriangleFadePass"); //currently unused
            GameShaders.Misc["Polarities:WarpZoomRipple"] = new MiscShaderData(miscEffectsRef, "WarpZoomRipplePass");
            GameShaders.Misc["Polarities:EclipxieSun"] = new MiscShaderData(miscEffectsRef, "EclipxieSunPass");
            GameShaders.Misc["Polarities:RadialOverlay"] = new MiscShaderData(miscEffectsRef, "RadialOverlayPass");
            GameShaders.Misc["Polarities:DrawAsSphere"] = new MiscShaderData(miscEffectsRef, "DrawAsSpherePass");
            GameShaders.Misc["Polarities:DrawWavy"] = new MiscShaderData(miscEffectsRef, "DrawWavyPass");
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
            ConvectiveDashHotkey = null;
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

            //boss checklist support
            if (ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
            {
                bossChecklist.Call("AddEvent", this, "$Mods.Polarities.BiomeName.HallowInvasion",
                    new List<int> { NPCType<Pegasus>(), NPCType<IlluminantScourer>(), NPCType<SunServitor>(), NPCType<Aequorean>(), NPCType<SunKnight>(), NPCType<Trailblazer>(), NPCType<Painbow>() }, //enemies
                    11.4f, () => PolaritiesSystem.downedHallowInvasion, () => true,
                    new List<int> { }, //collection
                    ItemType<HallowInvasionSummonItem>(), //spawning
                    "Use a [i:" + ItemType<HallowInvasionSummonItem>() + "], or wait after defeating any mechanical boss."
                );
                bossChecklist.Call("AddEvent", this, "$Mods.Polarities.BiomeName.WorldEvilInvasion",
                    new List<int> { NPCType<RavenousCursed>(), NPCType<LivingSpine>(), NPCType<LightEater>(), NPCType<Crimago>(), NPCType<TendrilAmalgam>(), NPCType<Uraraneid>() }, //enemies
                    11.6f, () => PolaritiesSystem.downedWorldEvilInvasion, () => true,
                    new List<int> { ItemType<PestilenceMusicBox>() }, //collection
                    ItemType<WorldEvilInvasionSummonItem>(), //spawning
                    "Use a [i:" + ItemType<WorldEvilInvasionSummonItem>() + "], or wait after defeating every mechanical boss.",
                    (SpriteBatch sb, Rectangle rect, Color color) => {
                        Texture2D texture = Request<Texture2D>("Polarities/Textures/BossChecklist/WorldEvilInvasion").Value;
                        Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
                        sb.Draw(texture, centered, color);
                    }
                );

                bossChecklist.Call("AddBoss", this, "$Mods.Polarities.NPCName.StormCloudfish", NPCType<StormCloudfish>(), 1.9f, () => PolaritiesSystem.downedStormCloudfish, () => true,
                    new List<int> { ItemType<StormCloudfishTrophy>(), ItemType<StormCloudfishMask>(), ItemType<StormCloudfishRelic>(), ItemType<GoldfishExplorerPetItem>(), ItemType<StormCloudfishMusicBox>(), ItemType<StormCloudfishPetItem>(), ItemType<EyeOfTheStormfish>() }, //collection
                    ItemType<StormCloudfishSummonItem>(), //spawning
                    "Fly a [i:" + ItemType<StormCloudfishSummonItem>() + " as high as you can at the surface.",
                    null,
                    (SpriteBatch sb, Rectangle rect, Color color) => {
                        Texture2D texture = Request<Texture2D>("Polarities/Textures/BossChecklist/StormCloudfish").Value;
                        Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
                        sb.Draw(texture, centered, color);
                    }
                );
                bossChecklist.Call("AddBoss", this, "$Mods.Polarities.NPCName.StarConstruct", NPCType<StarConstruct>(), 2.9f, () => PolaritiesSystem.downedStarConstruct, () => true,
                    new List<int> { ItemType<StarConstructTrophy>(), ItemType<StarConstructMask>(), ItemType<StarConstructRelic>(), ItemType<StarConstructPetItem>(), ItemType<StarConstructMusicBox>(), ItemType<Stardance>() }, //collection
                    ItemType<StarConstructSummonItem>(), //spawning
                    "Wait for a dormant construct to spawn at night while the player has at least 300 maximum life, or use a [i:" + ItemType<StarConstructSummonItem>() + " at the surface.",
                    null,
                    (SpriteBatch sb, Rectangle rect, Color color) => {
                        Texture2D texture = Request<Texture2D>("Polarities/Textures/BossChecklist/StarConstruct").Value;
                        Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
                        sb.Draw(texture, centered, color);
                    }
                );
                bossChecklist.Call("AddBoss", this, "$Mods.Polarities.NPCName.Gigabat", NPCType<Gigabat>(), 3.9f, () => PolaritiesSystem.downedGigabat, () => true,
                    new List<int> { ItemType<GigabatTrophy>(), ItemType<GigabatMask>(), ItemType<GigabatRelic>(), ItemType<GigabatPetItem>(), ItemType<GigabatMusicBox>(), ItemType<Batastrophe>() }, //collection
                    new List<int> { ItemType<AmethystGemflyItem>(), ItemType<TopazGemflyItem>(), ItemType<SapphireGemflyItem>(), ItemType<EmeraldGemflyItem>(), ItemType<RubyGemflyItem>(), ItemType<DiamondGemflyItem>(), ItemType<AmberGemflyItem>(), ItemType<GigabatSummonItem>() }, //spawning
                    "Release gemflies and wait, or use a [i:" + ItemType<GigabatSummonItem>() + "], while underground."
                );
                bossChecklist.Call("AddBoss", this, "$Mods.Polarities.NPCName.SunPixie", NPCType<SunPixie>(), 11.41f, () => PolaritiesSystem.downedSunPixie, () => true,
                    new List<int> { ItemType<SunPixieTrophy>(), ItemType<SunPixieMask>(), ItemType<SunPixieRelic>(), ItemType<SunPixiePetItem>(), ItemType<SunPixieMusicBox>(), ItemType<RayOfSunshine>() }, //collection
                    ItemType<SunPixieSummonItem>(), //spawning
                    "Reach the end of the Rapture, or use a [i:" + ItemType<SunPixieSummonItem>() + "] anywhere.",
                    null,
                    (SpriteBatch sb, Rectangle rect, Color color) => {
                        Texture2D texture = Request<Texture2D>("Polarities/Textures/BossChecklist/SunPixie").Value;
                        Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
                        sb.Draw(texture, centered, color);
                    }
                );
                bossChecklist.Call("AddBoss", this, "$Mods.Polarities.NPCName.Esophage", NPCType<Esophage>(), 11.61f, () => PolaritiesSystem.downedEsophage, () => true,
                    new List<int> { ItemType<EsophageTrophy>(), ItemType<EsophageMask>(), ItemType<EsophageRelic>(), ItemType<EsophageMusicBox>(), ItemType<Contagun>() }, //collection
                    ItemType<EsophageSummonItem>(), //spawning
                    "Reach the end of the Pestilence, or use a [i:" + ItemType<EsophageSummonItem>() + "] anywhere.",
                    null,
                    (SpriteBatch sb, Rectangle rect, Color color) => {
                        Texture2D texture = Request<Texture2D>("Polarities/Textures/BossChecklist/Esophage").Value;
                        Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
                        sb.Draw(texture, centered, color);
                    }
                );
                bossChecklist.Call("AddBoss", this, "$Mods.Polarities.NPCName.ConvectiveWanderer", NPCType<ConvectiveWanderer>(), 12.99f, () => PolaritiesSystem.downedConvectiveWanderer, () => true,
                    new List<int> { ItemType<ConvectiveWandererTrophy>(), ItemType<ConvectiveWandererMask>(), ItemType<ConvectiveWandererRelic>() }, //collection
                    new List<int> { ItemType<BabyWandererItem>(), ItemType<ConvectiveWandererSummonItem>() }, //spawning
                    "Kill a baby wanderer, or use a [i:" + ItemType<ConvectiveWandererSummonItem>() + "], at the lava ocean.",
                    null,
                    (SpriteBatch sb, Rectangle rect, Color color) => {
                        Texture2D texture = Request<Texture2D>("Polarities/Textures/BossChecklist/ConvectiveWanderer").Value;
                        Vector2 centered = new Vector2(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
                        sb.Draw(texture, centered, texture.Frame(), color, 0f, texture.Size() / 2, 0.2f, SpriteEffects.None, 0f);
                    }
                );
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

        public override void AddRecipeGroups()/* tModPorter Note: Removed. Use ModSystem.AddRecipeGroups */
        {
            RecipeGroup.RegisterGroup(
                "WoodenAtlatl",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemType<Items.Weapons.Ranged.Atlatls.WoodenAtlatl>())}",
                    ItemType<Items.Weapons.Ranged.Atlatls.WoodenAtlatl>(),
                    ItemType<Items.Weapons.Ranged.Atlatls.BorealWoodAtlatl>(),
                    ItemType<Items.Weapons.Ranged.Atlatls.PalmWoodAtlatl>(),
                    ItemType<Items.Weapons.Ranged.Atlatls.MahoganyAtlatl>(),
                    ItemType<Items.Weapons.Ranged.Atlatls.EbonwoodAtlatl>(),
                    ItemType<Items.Weapons.Ranged.Atlatls.ShadewoodAtlatl>(),
                    ItemType<Items.Weapons.Ranged.Atlatls.PearlwoodAtlatl>()
                ));

            RecipeGroup.RegisterGroup(
                "SilverBar",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}",
                    ItemID.SilverBar,
                    ItemID.TungstenBar
                ));

            RecipeGroup.RegisterGroup(
                "AdamantiteBar",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.AdamantiteBar)}",
                    ItemID.AdamantiteBar,
                    ItemID.TitaniumBar
                ));

            RecipeGroup.RegisterGroup(
                "RottenChunk",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.RottenChunk)}",
                    ItemID.RottenChunk,
                    ItemID.Vertebrae
                ));

            RecipeGroup.RegisterGroup(
                "ShadowScale",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.ShadowScale)}",
                    ItemID.ShadowScale,
                    ItemID.TissueSample
                ));

            RecipeGroup.RegisterGroup(
                "Duck",
                new RecipeGroup(
                    () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.Duck)}",
                    ItemID.Duck,
                    ItemID.MallardDuck
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

        internal static SoundStyle GetSounds(string name, int num, float volume = 1f, float pitch = 0f, float variance = 0f)
        {
            return new SoundStyle("Polarities/Sounds" + name, 0, num) { Volume = volume, Pitch = pitch, PitchVariance = variance, };
        }
        internal static SoundStyle GetSound(string name, float volume = 1f, float pitch = 0f, float variance = 0f)
        {
            return new SoundStyle("Polarities/Sounds/" + name) { Volume = volume, Pitch = pitch, PitchVariance = variance, };
        }
    }
}