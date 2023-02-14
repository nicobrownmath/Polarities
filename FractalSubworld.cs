using Microsoft.Xna.Framework;
using Polarities.Biomes.Fractal;
using Polarities.Buffs;
using Polarities.Items.Placeable.Bars;
using Polarities.Items.Placeable.Blocks.Fractal;
using Polarities.Items.Placeable.Furniture;
using Polarities.Items.Placeable.Furniture.Fractal;
using Polarities.Tiles;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Polarities
{
    public readonly struct BigRational : IComparable<BigRational>, IEquatable<BigRational>
    {
        public readonly BigInteger num;
        public readonly BigInteger den;

        public int CompareTo(BigRational bigRational)
        {
            return (num * bigRational.den).CompareTo(den * bigRational.num);
        }

        public bool Equals(BigRational bigRational)
        {
            return (num * bigRational.den).Equals(den * bigRational.num);
        }

        public static BigRational operator +(BigRational a) => a;
        public static BigRational operator -(BigRational a) => new BigRational(-a.num, a.den);

        public static BigRational operator +(BigRational a, BigRational b)
            => new BigRational(a.num * b.den + b.num * a.den, a.den * b.den);

        public static BigRational operator -(BigRational a, BigRational b)
        => a + (-b);

        public static BigRational operator *(BigRational a, BigRational b)
            => new BigRational(a.num * b.num, a.den * b.den);

        public static BigRational operator /(BigRational a, BigRational b)
        {
            if (b.num == 0)
            {
                throw new DivideByZeroException();
            }
            return new BigRational(a.num * b.den, a.den * b.num);
        }

        public static BigRational operator /(BigRational a, int b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException();
            }
            return new BigRational(a.num, a.den * b);
        }

        public double ToDouble()
        {
            BigInteger quotient = BigInteger.DivRem(num, den, out BigInteger remainder);

            return (double)quotient + (double)remainder / (double)den;
        }

        public BigRational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0)
            {
                throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            }

            num = numerator / BigInteger.GreatestCommonDivisor(numerator, denominator);
            den = denominator / BigInteger.GreatestCommonDivisor(numerator, denominator);

            if (den < 0)
            {
                num = -num;
                den = -den;
            }
        }

        public override string ToString() => $"{num} / {den}";
    }

    public class FractalSubworld : Subworld
    {
        //for data syncing
        public static TagCompound subworldSyncedData;

        internal delegate ref bool DownedFlag();

        internal static Dictionary<string, DownedFlag> DownedFunctions { get; private set; }

        public override void Load()
        {
            DownedFunctions = new Dictionary<string, DownedFlag>()
            {
                ["slimeKing"] = () => ref NPC.downedSlimeKing,
                ["boss1"] = () => ref NPC.downedBoss1,
                ["boss2"] = () => ref NPC.downedBoss2,
                ["boss3"] = () => ref NPC.downedBoss3,
                ["queenBee"] = () => ref NPC.downedQueenBee,
                ["hardMode"] = () => ref Main.hardMode,
                ["mechBossAny"] = () => ref NPC.downedMechBossAny,
                ["mechBoss1"] = () => ref NPC.downedMechBoss1,
                ["mechBoss2"] = () => ref NPC.downedMechBoss2,
                ["mechBoss3"] = () => ref NPC.downedMechBoss3,
                ["plantBoss"] = () => ref NPC.downedPlantBoss,
                ["golemBoss"] = () => ref NPC.downedGolemBoss,
                ["fishron"] = () => ref NPC.downedFishron,
                ["ancientCultist"] = () => ref NPC.downedAncientCultist,
                ["moonlord"] = () => ref NPC.downedMoonlord,
                ["qs"] = () => ref NPC.downedQueenSlime, // queenSlime
                ["eol"] = () => ref NPC.downedEmpressOfLight, // empressOfLight

                ["goblins"] = () => ref NPC.downedGoblins,
                ["oldOnesArmyT1"] = () => ref DD2Event.DownedInvasionT1,
                ["oldOnesArmyT2"] = () => ref DD2Event.DownedInvasionT2,
                ["oldOnesArmyT3"] = () => ref DD2Event.DownedInvasionT3,
                ["frost"] = () => ref NPC.downedFrost,
                ["pirates"] = () => ref NPC.downedPirates,
                ["halloweenTree"] = () => ref NPC.downedHalloweenTree,
                ["halloweenKing"] = () => ref NPC.downedHalloweenKing,
                ["christmasTree"] = () => ref NPC.downedChristmasTree,
                ["christmasSantank"] = () => ref NPC.downedChristmasSantank,
                ["christmasIceQueen"] = () => ref NPC.downedChristmasIceQueen,
                ["martians"] = () => ref NPC.downedMartians,
                ["towerNebula"] = () => ref NPC.downedTowerNebula,
                ["towerSolar"] = () => ref NPC.downedTowerSolar,
                ["towerStardust"] = () => ref NPC.downedTowerStardust,
                ["towerVortex"] = () => ref NPC.downedTowerVortex,

                ["scfish"] = () => ref PolaritiesSystem.downedStormCloudfish, // stormCloudfish
                ["starConstruct"] = () => ref PolaritiesSystem.downedStarConstruct,
                ["gb"] = () => ref PolaritiesSystem.downedGigabat, // gigabat
                ["riftDenizen"] = () => ref PolaritiesSystem.downedRiftDenizen,
                ["sunPixie"] = () => ref PolaritiesSystem.downedSunPixie,
                ["esophage"] = () => ref PolaritiesSystem.downedEsophage,
                ["sentinel"] = () => ref PolaritiesSystem.downedSelfsimilarSentinel,
                ["ecp"] = () => ref PolaritiesSystem.downedEclipxie, // eclipxie
                ["hemorrphage"] = () => ref PolaritiesSystem.downedHemorrphage,
                ["polarities"] = () => ref PolaritiesSystem.downedPolarities,

                ["hallowInvasion"] = () => ref PolaritiesSystem.downedHallowInvasion,
                ["worldEvilInvasion"] = () => ref PolaritiesSystem.downedWorldEvilInvasion,

                ["boc"] = () => ref PolaritiesSystem.downedBrainOfCthulhu, // brainOfCthulhu
                ["eow"] = () => ref PolaritiesSystem.downedEaterOfWorlds, // eaterOfWorlds
            };
        }

        public override void Unload()
        {
            DownedFunctions?.Clear();
            DownedFunctions = null;
        }

        public static void SaveUniversalData()
        {
            var downed = new List<string>();
            foreach (var pair in DownedFunctions)
            {
                if (pair.Value?.Invoke() == true)
                {
                    downed.Add(pair.Key);
                }
            }

            subworldSyncedData = new TagCompound()
            {
                ["downed"] = downed,
                ["killCount"] = NPC.killCount.ToList(),
                ["anglerQuest"] = Main.anglerQuest,
                ["anglerQuestFinished"] = Main.anglerQuestFinished,
            };
        }
        public static void LoadUniversalData()
        {
            if (subworldSyncedData.ContainsKey("downed"))
            {
                var downed = subworldSyncedData.GetList<string>("downed");

                foreach (var pair in DownedFunctions)
                {
                    if (pair.Value != null && downed.Contains(pair.Key))
                    {
                        pair.Value() = true;
                    }
                }
            }
            if (subworldSyncedData.ContainsKey("killCount"))
            {
                NPC.killCount = subworldSyncedData.GetList<int>("killCount").ToArray();
            }
            if (subworldSyncedData.ContainsKey("anglerQuest"))
            {
                Main.anglerQuest = subworldSyncedData.GetInt("anglerQuest");
            }
            if (subworldSyncedData.ContainsKey("anglerQuestFinished"))
            {
                Main.anglerQuestFinished = subworldSyncedData.GetBool("anglerQuestFinished");
            }
        }

        public static bool Active => SubworldSystem.IsActive<FractalSubworld>();

        public static bool entering;
        public static bool exiting;
        private static int animSeed;
        public static Stopwatch animStopwatch = new Stopwatch();

        public override void DrawMenu(GameTime gameTime)
        {
            base.DrawMenu(gameTime);
            Mod.Logger.Debug("This is the menu.");
        }

        public override void DrawSetup(GameTime gameTime)
        {
            base.DrawSetup(gameTime);
            Mod.Logger.Debug("This is the draw setup thing...?");
        }

        public static void DrawEntryAnimation(GameTime gameTime)
        {
            //if (!animStopwatch.IsRunning)
            //{
            //    //initialize animation
            //    animStopwatch.Restart();
            //    animSeed = Main.rand.Next();
            //}

            //float totalTime = (float)(animStopwatch.Elapsed.TotalSeconds);

            //float progress = Math.Min(1, totalTime / 3f);

            ////Main.graphics.GraphicsDevice.Clear(new Color((int)(1 * progress), (int)(2 * progress), (int)(24 * progress)));
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, (Effect)null, Main.Transform);
            //Main.spriteBatch.Draw(ModContent.GetTexture("Polarities/Projectiles/CallShootProjectile"), Vector2.Zero, new Rectangle(0, 0, 1, 1), new Color((int)(1 * progress), (int)(2 * progress), (int)(24 * progress)), 0f, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight), (SpriteEffects)0, 0f);
            //Main.spriteBatch.End();

            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, (Effect)null, Main.Transform);

            //UnifiedRandom animRand = new UnifiedRandom(animSeed);
            //for (int i = 1; i <= 256; i++)
            //{
            //    const float borderWidth = 100f;
            //    const float borderHeight = 100f;

            //    float parallaxMultiplier = (float)Math.Pow(i / 256f, 2);
            //    float scaleMultiplier = animRand.NextFloat(0.8f, 1.25f);
            //    Vector2 riftPos = new Vector2(animRand.NextFloat(0, (Main.screenWidth + borderWidth * 2) / parallaxMultiplier), animRand.NextFloat(0, (Main.screenHeight + borderHeight * 2) / parallaxMultiplier));

            //    float riftBlinking = (float)Math.Max(0, Math.Min(1, 10 * Math.Sin(animRand.NextFloat(MathHelper.TwoPi) + totalTime / 2f)));
            //    Vector2 riftOffset = new Vector2(animRand.NextFloat(0.5f, 1f), 0).RotatedBy(animRand.NextFloat(-0.5f, 0.5f)) * totalTime * 80;

            //    if (riftBlinking > 0)
            //    {
            //        Texture2D riftTexture = GetTexture("Polarities/Clouds/rift_1");
            //        Color drawColor = new Color((int)((1 + (255 - 1) * parallaxMultiplier) * progress), (int)((2 + (255 - 2) * parallaxMultiplier) * progress), (int)((24 + (255 - 24) * parallaxMultiplier) * progress));

            //        Vector2 drawPos = riftPos + riftOffset * parallaxMultiplier;
            //        drawPos = new Vector2(drawPos.X % (Main.screenWidth + borderWidth * 2), drawPos.Y % (Main.screenHeight + borderHeight * 2));
            //        drawPos += new Vector2(Main.screenWidth + borderWidth * 2, Main.screenHeight + borderHeight * 2);
            //        drawPos = new Vector2(drawPos.X % (Main.screenWidth + borderWidth * 2), drawPos.Y % (Main.screenHeight + borderHeight * 2));

            //        float textureRadius = riftTexture.Size().Length() / 2;
            //        if (drawPos.X > -textureRadius && drawPos.X < Main.screenWidth + textureRadius && drawPos.Y > -textureRadius && drawPos.Y < Main.screenHeight + textureRadius)
            //            Main.spriteBatch.Draw(riftTexture, drawPos, riftTexture.Frame(), drawColor, 0f, riftTexture.Size() / 2, new Vector2(riftBlinking, 1) * 1.33f * parallaxMultiplier * scaleMultiplier, (SpriteEffects)0, 0f);
            //    }
            //}

            ///*if (totalTime > 10f)
            //{
            //    String text = "Loading";
            //    String extraText = "";
            //    for (int i = 0; i < (int)(totalTime - 10) % 4; i++)
            //    {
            //        extraText += ".";
            //    }
            //    float alpha = Math.Min(1, (totalTime - 10f) / 4f);
            //    ReLogic.Graphics.DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, Main.fontDeathText, text + extraText, new Vector2(Main.screenWidth, Main.screenHeight) / 2, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor) * alpha, 0f, Main.fontDeathText.MeasureString(text) / 2, 1f, (SpriteEffects)0, 0f);
            //}*/

            //Texture2D edgeTexture = ModContent.GetTexture("Polarities/MiscTextures/FractalTransitionBorder");
            //Main.spriteBatch.Draw(edgeTexture, Vector2.Zero, edgeTexture.Frame(), Color.Black, 0f, Vector2.Zero, new Vector2(Main.screenWidth / (float)edgeTexture.Width, Main.screenHeight / (float)edgeTexture.Height), (SpriteEffects)0, 0f);

            //Main.spriteBatch.End();
        }

        public static void DoEnter()
        {
            entering = true;
            SaveUniversalData();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Main.projectile[i].active = false;
            }

            SubworldSystem.Enter<FractalSubworld>();
        }
        public static void DoExit()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Main.projectile[i].active = false;
            }

            SubworldSystem.Exit();
        }

        public override void OnLoad()
        {
            Main.dayTime = true;
            Main.time = Main.dayLength / 2;
            SubworldSystem.noReturn = false;

            ResetDimension();

            base.OnLoad();
        }
        public override void OnUnload()
        {
            base.OnUnload();
        }

        public override void OnEnter()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Main.LocalPlayer.ClearBuff(ModContent.BuffType<Fractalizing>());
                Main.LocalPlayer.Polarities().fractalization = 0;
            }
        }

        public override void OnExit()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Main.LocalPlayer.ClearBuff(ModContent.BuffType<Fractalizing>());
                Main.LocalPlayer.Polarities().fractalization = 0;
            }
        }


        public override int Width => Main.maxTilesX; //lower these values if testing worldgen because tbh it takes a while
        public override int Height => Main.maxTilesY;

        public override bool ShouldSave => true;
        public override bool NormalUpdates => false;

        private static int COEFFICIENTCOUNT = 192; //lower these values if testing worldgen because tbh it takes a while
        private static int MAXITERATIONS = 256;

        //world size multipliers for ease of access
        //Product is 1 for small worlds, 16/7 = 2.285714... for medium worlds, and 4 for large worlds
        private static float worldSizeMultiplierX => Main.maxTilesX / 4200f;
        private static float worldSizeMultiplierY => Main.maxTilesY / 1200f;

        //these values need to scale with size
        private static int NUMSTRANDS => (int)(540 * worldSizeMultiplierX * worldSizeMultiplierY);
        private static int NUMCAVES => (int)(128 * worldSizeMultiplierX * worldSizeMultiplierY);
        private static int NUM_LIGHTSLATE_VEINS => (int)(768 * worldSizeMultiplierX * worldSizeMultiplierY);
        private static int NUM_FRACTAL_ORE_VEINS => (int)(768 * worldSizeMultiplierX * worldSizeMultiplierY);
        private static int NUM_DENDRITIC_ORE_VEINS => (int)(128 * worldSizeMultiplierX * worldSizeMultiplierY);

        private static int NUM_SENTINEL_CAVES => (int)(3 * worldSizeMultiplierX * worldSizeMultiplierY);

        //this value needs to scale with Height
        public static int skyHeight => Main.maxTilesY / 4;

        private static double FEIGENBAUMCONSTANT = 4.669201609;

        private double[] randOffsets = new double[8];

        //temporal gating system:
        //hardmode danger time is 5 minutes, health is at least halved here
        //post-golem time is 10 minutes, health is reduced to zero here

        public static int HARDMODE_DANGER_TIME
        {
            get => Main.expertMode ? 18000 : 27000;
        }
        public static int POST_SENTINEL_TIME
        {
            get => Main.expertMode ? 36000 : 54000;
        }



        private BigRational[,] uCoefficients = new BigRational[COEFFICIENTCOUNT * 2, COEFFICIENTCOUNT * 2];
        private bool[,] knownUCoefficients = new bool[COEFFICIENTCOUNT * 2, COEFFICIENTCOUNT * 2];
        private BigRational[] aCoefficients = new BigRational[COEFFICIENTCOUNT * 2];
        private BigRational[,] wCoefficients = new BigRational[COEFFICIENTCOUNT * 2, COEFFICIENTCOUNT * 2];
        //private double[] bCoefficients = new double[COEFFICIENTCOUNT];

        //number spam!
        //this is for generating points on the boundary of the mandelbrot set
        private double[] bCoefficients = new double[] {
            -0.5,
            0.125,
            -0.25,
            0.1171875,
            0,
            -0.0458984375,
            -0.0625,
            0.030120849609375,
            0,
            -0.014011383056640625,
            0.03125,
            -0.014550447463989258,
            0,
            -0.020547360181808472,
            -0.041015625,
            0.027590879704803228,
            0,
            -0.0095875554834492505,
            0.01904296875,
            -0.0087413567198382225,
            -0.015625,
            -0.0022058940562601492,
            0.01416015625,
            -0.00025794322824879146,
            0,
            0.0062778928052065197,
            -0.00792694091796875,
            -0.0024475944800471661,
            -0.00390625,
            -0.010415658002275605,
            -0.0087337493896484375,
            0.0058706156531350417,
            0,
            -0.0021057886362261507,
            0.0027189254760742188,
            -0.0051262377166050406,
            -0.0009765625,
            0.00081575411762917856,
            0.0054866671562194824,
            -0.0013344333522704675,
            0,
            0.00068247218066929316,
            -0.0034244805574417114,
            0.00093380229204111142,
            -0.010009765625,
            -0.0007297655417124279,
            0.0084098130464553833,
            -0.0018054806434445821,
            0,
            0.0022869196081968237,
            -0.0034234257182106376,
            -0.0025678994205525158,
            0.0040283203125,
            0.0036886948038130202,
            -0.00087479929788969457,
            -0.0010763903483377534,
            0,
            -0.0016709186030983549,
            -0.00040554576844442636,
            -0.0045221782526756538,
            0.000244140625,
            -0.00213422292745167,
            -0.0107750440683958,
            0.0063718617820966699,
            0,
            -0.0038698104215115236,
            0.0049842382561564591,
            -0.00057800362191535808,
            -0.0008392333984375,
            -0.002396134314233227,
            0.0023232390110479173,
            -0.00013760910910918113,
            0,
            0.0017556768424705216,
            -0.0051433419902018329,
            -0.00043319558547705826,
            -0.0031838417053222656,
            -0.00083413641083289784,
            0.0072304024672940059,
            -0.0024589323339278199,
            0,
            0.0021884698637767933,
            -0.0031591760710338868,
            0.00026354202423913632,
            0.0012444257736206055,
            0.0022973074557057654,
            -0.0010226157613696663,
            -2.8916820319341262E-05,
            0,
            -0.0041723310959538764,
            -7.1389269436152036E-05,
            -0.00066055975954124762,
            -0.0031091272830963135,
            0.00033472994654042311,
            0.0023641127364584583,
            6.9061639422312186E-05,
            0,
            0.00054024932416875198,
            -0.0003530578212284264,
            -2.5951338605666478E-05,
            0.00051956623792648315,
            0.0010887922004316638,
            -0.00048524114505197951,
            2.5507281287138199E-05,
            -0.00201416015625,
            -0.00088941785704427353,
            0.00012292656649944445,
            -0.0031961982437320592,
            0.003545023500919342,
            0.0029336000289393004,
            -0.0015415242237938713,
            0.00043707417730027404,
            0,
            -0.0011133900712646211,
            0.00027976464943632319,
            0.0018444675058909908,
            0.00059529487043619156,
            -0.00075914150985705747,
            -0.0011043554070704863,
            -0.001252118655675688,
            -0.0005035400390625,
            0.00013590946970896083,
            -0.0013823520165868234,
            1.6638674007869759E-05,
            -0.00016894773580133915,
            -0.003625465313028182,
            -0.0020910110916632766,
            0.0017981524511511905,
            0,
            -9.4093208554502019E-05,
            0.0017067402892367086,
            -0.0007139906584758953,
            -0.0021463851371663623,
            -0.00048034608514886944,
            0.0011341461965100469,
            -0.00015293280266961821,
            4.9591064453125E-05,
            -0.00089829356265279652,
            -0.00075660077818067741,
            0.00060403166759494004,
            -0.0010349910644436022,
            0.00057901396089195842,
            0.0017419145052653955,
            -0.00066985442845764491,
            0,
            0.00051697980507948874,
            -0.00074653780538838578,
            -0.00059767698808830248,
            0.00083416366624078364,
            0.00038036898398854398,
            -0.00053849108119395704,
            -0.00023746806070431057,
            -0.0012063980102539063,
            -0.00030006253917988946,
            -0.00088029960442445739,
            -0.0010612136523732061,
            -0.00035173148842204682,
            0.00028525087108801988,
            0.0016110772590845605,
            -0.00029825203703013481,
            0,
            0.00025005066029651504,
            -0.00079654064571301115,
            0.00031723406479812098,
            0.0003279021386219938,
            0.00063605358706161907,
            -0.00067657986827850494,
            -0.0010427144576563765,
            0.00012087821960449219,
            -0.00054609952466175792,
            0.000200020617925269,
            0.00019554901133849397,
            0.0012888983132199883,
            0.0009833033148889123,
            -0.0011333454937864161,
            0.00014000212606972377,
            0,
            -0.00046134148241303111,
            0.00066029766848002894,
            0.00054643657648634062,
            1.2145259020712729E-05,
            -0.0012361917410412938,
            -0.00071518042072494289,
            0.00027944209436338882,
            -2.7239322662353516E-05,
            -0.001124065861833989,
            -0.00062699405752346881,
            0.00061604689060051291,
            -0.0025979647182794441,
            -0.00039036786922158379,
            0.0024644351176327946,
            -0.00036048322412911445
        };
        private static int wastesDirection;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            //for generating magic mandelbrot numbers
            /*new PassLegacy("UAndACoefficient", (progress, config) =>
            {
                UAndACoefficientGenpass(progress);
            })
            ,
            new PassLegacy("WCoefficient", (progress, config) =>
            {
                WCoefficientGenpass(progress);
            })
            ,
            new PassLegacy("BCoefficient", (progress, config) =>
            {
                BCoefficientGenpass(progress);
            }),*/
            new PassLegacy("Init", (progress, config) =>
            {
                InitGenpass(progress);
            }),
            new PassLegacy("FractalTerrain", (progress, config) =>
            {
                FractalTerrainGenpass(progress);
            }),
            new PassLegacy("FractalCaves", (progress, config) =>
            {
                FractalCavesGenpass(progress);
            }),
            new PassLegacy("SentinelCaves", (progress, config) =>
            {
                SentinelCavesGenpass(progress);
            }),
            new PassLegacy("FractalCoasts", (progress, config) =>
            {
                FractalCoastsGenpass(progress);
            }),
            new PassLegacy("FractalWastes", (progress, config) =>
            {
                FractalWastesGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                FractalStrandsGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                FractalOresGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                FractalChestsGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                FractalHousesGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                SentinelVeinsGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                FractalTrapsGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                FractalWatersGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                FractalPlantsGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                HyphaeGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                StabilizeGenpass(progress);
            }),
            new PassLegacy("Init", (progress, config) =>
            {
                EndGenpass(progress);
            })
        };

        private void InitGenpass(GenerationProgress progress)
        {
            //set protected tiles
            protectedTiles = new int[]{
                TileType<FractalBrickTile>(),
                TileType<SelfsimilarOreTile>(),
            };

            WorldGen.noTileActions = true;
        }

        private void EndGenpass(GenerationProgress progress)
        {
            WorldGen.noTileActions = false;
        }

        /*private void UAndACoefficientGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating U and A coefficients";
            for (int m = 0; m < COEFFICIENTCOUNT + 1; m++)
            {
                progress.Set(m / (float)(COEFFICIENTCOUNT + 1));
                aCoefficients[m] = UCoefficient(0, m);
            }
        }

        private void WCoefficientGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating W coefficients";

            for (int m = 0; m < COEFFICIENTCOUNT + 1; m++)
            {
                for (int n = 0; n < COEFFICIENTCOUNT; n++)
                {
                    progress.Message = "Generating W Coefficients";
                    progress.Set(((m * COEFFICIENTCOUNT + n) / (float)(COEFFICIENTCOUNT * COEFFICIENTCOUNT + COEFFICIENTCOUNT)) * ((m * COEFFICIENTCOUNT + n) / (float)(COEFFICIENTCOUNT * COEFFICIENTCOUNT + COEFFICIENTCOUNT)));

                    if (n == 0)
                    {
                        wCoefficients[n, m] = new BigRational(0, 1);
                    }
                    else
                    {
                        BigRational val = aCoefficients[m] + wCoefficients[n - 1, m];
                        for (int j = 0; j <= m - 2; j++)
                        {
                            val += aCoefficients[j + 1] * wCoefficients[n - 1, m - j - 1];
                        }
                        wCoefficients[n, m] = val;
                    }
                }
            }
        }

        private void BCoefficientGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating B coefficients";

            for (int n = 0; n < COEFFICIENTCOUNT; n++)
            {
                progress.Set(n / (float)COEFFICIENTCOUNT);

                if (n == 0)
                {
                    bCoefficients[n] = -0.5;
                } else
                {
                    bCoefficients[n] = (-wCoefficients[n, n + 1] / n).ToDouble();
                }
            }
            string bCoefficientString = "new double[] {";
            for (int n = 0; n < COEFFICIENTCOUNT; n++)
            {
                //round-trip double conversion
                bCoefficientString += bCoefficients[n].ToString("G17");
                if (n < COEFFICIENTCOUNT - 1)
                {
                    bCoefficientString += ",\n";
                }
                else
                {
                    bCoefficientString += "\n";
                }
            }
            bCoefficientString += "}";
            modWorld.mod.Logger.Info(bCoefficientString);
        }*/

        private void FractalTerrainGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating fractal terrain"; //Sets the text above the worldgen progress bar
            Main.worldSurface = Main.maxTilesY - 42; //Hides the underground layer just out of bounds
            Main.rockLayer = Main.maxTilesY; //Hides the cavern layer way out of bounds
            SubworldSystem.hideUnderworld = true;
            SubworldSystem.noReturn = true;
            for (int i = 0; i < 8; i++)
            {
                randOffsets[i] = WorldGen.genRand.NextDouble() * 2 * Math.PI;
            }

            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY)); //Controls the progress bar, should only be set between 0f and 1f


                    double x = (2 * i / (double)Width - 1) / 128 * Width / Height;
                    double y = (2 * j / (double)Height - 2) / 128;

                    Vector2 vars = new Vector2((float)expInterpolate(-1.77985, -1.779, j / (double)Height, Math.Pow(FEIGENBAUMCONSTANT, 0.25)), 0) + FindRandomVariance(i, j) / (int)Math.Pow(2, 15) * (j / (float)Height);

                    float varX = vars.X;
                    float varY = vars.Y;

                    for (int iterations = 0; iterations < MAXITERATIONS; iterations++)
                    {
                        double newX = x * x - y * y + varX;
                        double newY = 2 * x * y + varY;
                        x = newX;
                        y = newY;

                        if (x * x + y * y > 4)
                        {
                            break;
                        }
                    }
                    var tile = Main.tile[i, j];
                    if (x * x + y * y < 4)
                    {
                        tile.HasTile = true;
                        Main.tile[i, j].TileType = (ushort)TileType<FractalMatterTile>();
                    }
                    else
                    {
                        tile.HasTile = false;
                    }
                }
            }

            for (int i = 1; i < Main.maxTilesX - 1; i++)
            {
                for (int j = 1; j < Main.maxTilesY - 1; j++)
                {
                    if (Main.tile[i - 1, j - 1].HasTile && Main.tile[i - 1, j].HasTile && Main.tile[i - 1, j + 1].HasTile &&
                    Main.tile[i, j - 1].HasTile && Main.tile[i, j].HasTile && Main.tile[i, j + 1].HasTile &&
                    Main.tile[i + 1, j - 1].HasTile && Main.tile[i + 1, j].HasTile && Main.tile[i + 1, j + 1].HasTile)
                    {
                        Main.tile[i, j].WallType = (ushort)WallType<FractalMatterWallNatural>();
                    }
                }
            }

            Main.spawnTileY = Main.maxTilesY - 2;
            while (Main.tile[Main.spawnTileX, Main.spawnTileY].HasTile || Main.tile[Main.spawnTileX - 2, Main.spawnTileY].HasTile || Main.tile[Main.spawnTileX - 1, Main.spawnTileY].HasTile ||
            Main.tile[Main.spawnTileX, Main.spawnTileY - 2].HasTile || Main.tile[Main.spawnTileX - 2, Main.spawnTileY - 2].HasTile || Main.tile[Main.spawnTileX - 1, Main.spawnTileY - 2].HasTile ||
            Main.tile[Main.spawnTileX, Main.spawnTileY - 1].HasTile || Main.tile[Main.spawnTileX - 2, Main.spawnTileY - 1].HasTile || Main.tile[Main.spawnTileX - 1, Main.spawnTileY - 1].HasTile)
            {
                if (Main.spawnTileY == 1)
                {
                    Main.spawnTileY = Main.maxTilesY - 1;
                    Main.spawnTileX++;
                }

                Main.spawnTileY--;
            }
        }

        private void FractalCavesGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating caves";

            for (int a = 0; a < NUMCAVES; a++)
            {
                progress.Set(a / (float)NUMCAVES);

                int centerX = 0;
                int centerY = 0;
                for (int tries = 0; tries < 256; tries++)
                {
                    centerX = WorldGen.genRand.Next(Main.maxTilesX);
                    centerY = WorldGen.genRand.Next(Main.maxTilesY);

                    if (Math.Pow((centerX - Main.maxTilesX / 2) / (float)Main.maxTilesX, 2) < 2 * (centerY - Main.maxTilesY / 2) / (float)Main.maxTilesY)
                    {
                        break;
                    }
                }
                int scale = WorldGen.genRand.Next(200, 300);
                double rot = WorldGen.genRand.NextDouble() * Math.PI * 2;

                Vector2 vars = MandelbrotBoundary(WorldGen.genRand.NextDouble() * Math.PI * 2);

                float varX = vars.X;
                float varY = vars.Y;

                for (int i = Math.Max(0, centerX - scale * 2); i <= Math.Min(Main.maxTilesX - 1, centerX + scale * 2); i++)
                {
                    for (int j = Math.Max(0, centerY - scale * 2); j <= Math.Min(Main.maxTilesY - 1, centerY + scale * 2); j++)
                    {
                        double x = (centerX - i) / (double)scale * Math.Cos(rot) - (centerY - j) / (double)scale * Math.Sin(rot);
                        double y = (centerY - j) / (double)scale * Math.Cos(rot) + (centerX - i) / (double)scale * Math.Sin(rot);

                        for (int iterations = 0; iterations < 20; iterations++)
                        {
                            double newX = x * x - y * y + varX;
                            double newY = 2 * x * y + varY;
                            x = newX;
                            y = newY;

                            if (x * x + y * y > 4)
                            {
                                break;
                            }
                        }
                        var tile = Main.tile[i, j];
                        if (x * x + y * y < 4)
                        {
                            tile.HasTile = false;
                        }
                    }
                }
            }
        }

        private void FractalWatersGenpass(GenerationProgress progress)
        {
            progress.Message = "Flood-filling";

            List<int[]> waterPoints = new List<int[]>();

            for (int y = 3 * Main.maxTilesY / 4; y < Main.maxTilesY; y++)
            {
                Tile tile;

                //fill this whole rectangle
                for (int x = 0; x < Main.maxTilesX / 6 - 1; x++)
                {
                    tile = Framing.GetTileSafely(x, y);

                    //fill with water

                    if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                    {
                        tile.LiquidType = 0;
                        tile.LiquidAmount = 255;
                    }
                }
                //fill this whole other rectangle
                for (int x = 5 * Main.maxTilesX / 6 + 1; x < Main.maxTilesX; x++)
                {
                    tile = Framing.GetTileSafely(x, y);

                    //fill with water

                    if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                    {
                        tile.LiquidType = 0;
                        tile.LiquidAmount = 255;
                    }
                }

                tile = Framing.GetTileSafely(Main.maxTilesX / 6 - 1, y);

                //add to water points
                if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                {
                    waterPoints.Add(new int[2] { Main.maxTilesX / 6 - 1, y });

                    tile.LiquidType = 0;
                    tile.LiquidAmount = 255;
                }

                tile = Framing.GetTileSafely(5 * Main.maxTilesX / 6, y);

                //add to water points
                if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                {
                    waterPoints.Add(new int[2] { 5 * Main.maxTilesX / 6, y });

                    tile.LiquidType = 0;
                    tile.LiquidAmount = 255;
                }
            }

            while (waterPoints.Count > 0)
            {
                int x;
                int y;


                x = waterPoints[0][0] + 1;
                y = waterPoints[0][1];

                if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    if ((!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) && tile.LiquidAmount == 0)
                    {
                        waterPoints.Add(new int[2] { x, y });

                        tile.LiquidType = 0;
                        tile.LiquidAmount = 255;
                    }
                }
                x = waterPoints[0][0] - 1;
                y = waterPoints[0][1];

                if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    if ((!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) && tile.LiquidAmount == 0)
                    {
                        waterPoints.Add(new int[2] { x, y });

                        tile.LiquidType = 0;
                        tile.LiquidAmount = 255;
                    }
                }
                x = waterPoints[0][0];
                y = waterPoints[0][1] + 1;

                if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    if ((!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) && tile.LiquidAmount == 0)
                    {
                        waterPoints.Add(new int[2] { x, y });

                        tile.LiquidType = 0;
                        tile.LiquidAmount = 255;
                    }
                }


                waterPoints.RemoveAt(0);
            }
        }

        private void FractalCoastsGenpass(GenerationProgress progress)
        {
            progress.Message = "Coasting";

            for (int x = 10; x < Main.maxTilesX - 10; x++)
            {
                for (int y = 10; y < Main.maxTilesY - 10; y++)
                {
                    float xFrac = (x + WorldGen.genRand.NextFloat(-20, 20)) / Main.maxTilesX;
                    float yFrac = (y + WorldGen.genRand.NextFloat(-20, 20)) / Main.maxTilesY;

                    if (yFrac > 2 - 3 * (2 * xFrac - 1) * (2 * xFrac - 1))
                    {
                        Tile tile = Framing.GetTileSafely(x, y);

                        if (tile.HasTile && !protectedTiles.Contains(tile.TileType))
                        {
                            tile.TileType = (ushort)TileType<FractalDustTile>();

                            if (WorldGen.genRand.NextBool(20) ||
                                (!Framing.GetTileSafely(x, y + 1).HasTile || protectedTiles.Contains(Framing.GetTileSafely(x, y + 1).TileType)) ||
                                ((!Framing.GetTileSafely(x, y + 2).HasTile || protectedTiles.Contains(Framing.GetTileSafely(x, y + 2).TileType)) && WorldGen.genRand.NextBool(2)) ||
                                ((!Framing.GetTileSafely(x, y + 3).HasTile || protectedTiles.Contains(Framing.GetTileSafely(x, y + 3).TileType)) && WorldGen.genRand.NextBool(3)))
                            {
                                tile.TileType = (ushort)TileType<FractalDuststoneTile>();
                            }
                        }
                        if (tile.WallType != 0)
                        {
                            tile.WallType = (ushort)WallType<FractalDuststoneWallNatural>();
                        }
                    }
                }
            }
        }

        private void FractalWastesGenpass(GenerationProgress progress)
        {
            progress.Message = "Wasting time";

            wastesDirection = (WorldGen.genRand.NextBool() ? 1 : -1);

            int centerX = Main.maxTilesX / 2 - WorldGen.genRand.Next(Main.maxTilesX / 8, 2 * Main.maxTilesX / 8) * wastesDirection;
            int centerY = WorldGen.genRand.Next(7 * Main.maxTilesY / 10, 8 * Main.maxTilesY / 10);
            int scale = (int)(WorldGen.genRand.Next(840, 960) * worldSizeMultiplierX);

            double rot = (WorldGen.genRand.NextDouble() * 2 - 1) * 0.25f;
            Vector2 vars = new Vector2(0.324f, 0.4f * wastesDirection);

            float varX = vars.X;
            float varY = vars.Y;

            for (int i = Math.Max(0, centerX - scale * 2); i <= Math.Min(Main.maxTilesX - 1, centerX + scale * 2); i++)
            {
                for (int j = Math.Max(0, centerY - scale * 2); j <= Math.Min(Main.maxTilesY - 4, centerY + scale * 2); j++)
                {
                    Tile tile = Framing.GetTileSafely(i, j);

                    if (tile.HasTile || tile.WallType != 0)
                    {
                        double x = (centerX - i) / (double)scale * Math.Cos(rot) - (centerY - j) / (double)scale * Math.Sin(rot);
                        double y = (centerY - j) / (double)scale * Math.Cos(rot) + (centerX - i) / (double)scale * Math.Sin(rot);

                        for (int iterations = 0; iterations < 20; iterations++)
                        {
                            double newX = x * x - y * y + varX + 0.001f * FindRandomVariance(i * 2, j * 2).X;
                            double newY = 2 * x * y + varY + 0.001f * FindRandomVariance(i * 2, j * 2).Y;
                            x = newX;
                            y = newY;

                            if (x * x + y * y > 4)
                            {
                                break;
                            }
                        }
                        if (x * x + y * y < 4)
                        {
                            if (tile.HasTile && !protectedTiles.Contains(tile.TileType))
                            {
                                tile.TileType = (ushort)TileType<FractalDustTile>();

                                if (WorldGen.genRand.NextBool(20) ||
                                    (!Framing.GetTileSafely(i, j + 1).HasTile || protectedTiles.Contains(Framing.GetTileSafely(i, j + 1).TileType)) ||
                                    ((!Framing.GetTileSafely(i, j + 2).HasTile || protectedTiles.Contains(Framing.GetTileSafely(i, j + 2).TileType)) && WorldGen.genRand.NextBool(2)) ||
                                    ((!Framing.GetTileSafely(i, j + 3).HasTile || protectedTiles.Contains(Framing.GetTileSafely(i, j + 3).TileType)) && WorldGen.genRand.NextBool(3)))
                                {
                                    tile.TileType = (ushort)TileType<FractalDuststoneTile>();
                                }
                            }
                            if (tile.WallType != 0)
                            {
                                tile.WallType = (ushort)WallType<FractalDuststoneWallNatural>();
                            }
                        }
                    }
                }
            }
        }

        private void FractalStrandsGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating strands";

            //strand forest
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    double x = (2 * i / (double)Width - 1) / 128 * Width / Height;
                    double y = (2 * j / (double)Height - 2) / 128;

                    Vector2 vars = new Vector2((float)x, (float)y) + FindRandomVariance(i, j) / 1024 * (j / (float)Height); //new Vector2((float)expInterpolate(-1.77985, -1.779, j / (double)Height, Math.Pow(FEIGENBAUMCONSTANT, 0.25)), 0) + FindRandomVariance(i, j) / (int)Math.Pow(2, 15) * (j / (float)Height);

                    if ((vars - new Vector2(0.02f * wastesDirection, -0.02f)).Length() < 0.015f)
                    {
                        if (Main.tile[i, j].HasTile && !protectedTiles.Contains(Main.tile[i, j].TileType))
                        {
                            WorldGen.PlaceTile(i, j, TileType<FractalStrandsTile>(), mute: true, forced: true);
                            if (Main.tile[i, j].WallType != 0) Main.tile[i, j].WallType = (ushort)WallType<FractalStrandsWallNatural>();
                        }
                    }
                }
            }

            for (int a = 0; a < NUMSTRANDS; a++)
            {
                progress.Set(a / (float)NUMSTRANDS);

                float xFrac;
                float yFrac;

                int centerX = 0;
                int centerY = 0;
                for (int tries = 0; tries < 256; tries++)
                {
                    centerX = WorldGen.genRand.Next(Main.maxTilesX);
                    centerY = WorldGen.genRand.Next(Main.maxTilesY);

                    if (Main.tile[centerX, centerY].HasTile && !protectedTiles.Contains(Main.tile[centerX, centerY].TileType))
                    {
                        xFrac = centerX / (float)Main.maxTilesX;
                        yFrac = centerY / (float)Main.maxTilesY;

                        //strands shouldn't generate in the wastes
                        if (yFrac > 2 - 3 * (2 * xFrac - 1) * (2 * xFrac - 1) || Framing.GetTileSafely(centerX, centerY).WallType != (ushort)WallType<FractalDuststoneWallNatural>())
                        {
                            break;
                        }
                    }
                }

                double xPos = (2 * centerX / (double)Width - 1) / 128 * Width / Height;
                double yPos = (2 * centerY / (double)Height - 2) / 128;

                Vector2 vars = new Vector2((float)xPos, (float)yPos) + FindRandomVariance(centerX, centerY) / 1024 * (centerY / (float)Height);

                int tileType = TileType<FractalStrandsTile>();
                int wallType = WallType<FractalStrandsWallNatural>();
                if ((vars - new Vector2(-0.02f * wastesDirection, -0.02f)).Length() < 0.015f)
                {
                    //in the forest, replace strands with tiles
                    tileType = TileType<FractalMatterTile>();
                    wallType = WallType<FractalMatterWallNatural>();
                }

                int scale = WorldGen.genRand.Next(76, 96);

                //strands should be made bigger in the oceans
                xFrac = centerX / (float)Main.maxTilesX;
                yFrac = centerY / (float)Main.maxTilesY;

                if (yFrac > 2 - 3 * (2 * xFrac - 1) * (2 * xFrac - 1))
                {
                    scale = WorldGen.genRand.Next(96, 120);
                }

                double rot = WorldGen.genRand.NextDouble() * Math.PI * 2;
                vars = MandelbrotBoundary(WorldGen.genRand.NextDouble() * Math.PI * 2);

                float varX = vars.X;
                float varY = vars.Y;

                for (int i = Math.Max(0, centerX - scale * 2); i <= Math.Min(Main.maxTilesX - 1, centerX + scale * 2); i++)
                {
                    for (int j = Math.Max(0, centerY - scale * 2); j <= Math.Min(Main.maxTilesY - 1, centerY + scale * 2); j++)
                    {
                        if (Main.tile[i, j].HasTile && !protectedTiles.Contains(Main.tile[i, j].TileType))
                        {
                            double x = (centerX - i) / (double)scale * Math.Cos(rot) - (centerY - j) / (double)scale * Math.Sin(rot);
                            double y = (centerY - j) / (double)scale * Math.Cos(rot) + (centerX - i) / (double)scale * Math.Sin(rot);

                            for (int iterations = 0; iterations < 20; iterations++)
                            {
                                double newX = x * x - y * y + varX;
                                double newY = 2 * x * y + varY;
                                x = newX;
                                y = newY;

                                if (x * x + y * y > 4)
                                {
                                    break;
                                }
                            }
                            if (x * x + y * y < 4)
                            {
                                WorldGen.PlaceTile(i, j, tileType, mute: true, forced: true);
                                if (Main.tile[i, j].WallType != 0) Main.tile[i, j].WallType = (ushort)wallType;
                            }
                        }
                    }
                }
            }
        }

        private void FractalOresGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating ores";

            for (int a = 0; a < NUM_LIGHTSLATE_VEINS; a++)
            {
                progress.Set(a / (float)NUM_LIGHTSLATE_VEINS / 3);

                int centerX = 0;
                int centerY = 0;
                for (int tries = 0; tries < 256; tries++)
                {
                    centerX = WorldGen.genRand.Next(Main.maxTilesX);
                    centerY = WorldGen.genRand.Next(Main.maxTilesY);

                    if (Main.tile[centerX, centerY].HasTile)
                    {
                        float xFrac = centerX / (float)Main.maxTilesX;
                        float yFrac = centerY / (float)Main.maxTilesY;

                        //lightslate should be more common in the oceans
                        if (WorldGen.genRand.NextBool() || yFrac > 2 - 3 * (2 * xFrac - 1) * (2 * xFrac - 1))
                        {
                            break;
                        }
                    }
                }
                int scale = WorldGen.genRand.Next(4, 7);

                float rad = 1;//Math.Max(WorldGen.genRand.NextFloat(),WorldGen.genRand.NextFloat());
                float rot = WorldGen.genRand.NextFloat() * MathHelper.TwoPi;

                Vector2 vars = new Vector2(rad, 0).RotatedBy(rot) + new Vector2(-1, 0);

                vars = new Vector2(1 - vars.X * vars.X + vars.Y * vars.Y, -2 * vars.X * vars.Y) / 4;

                float varX = vars.X;
                float varY = vars.Y;

                for (int i = Math.Max(0, centerX - scale * 2); i <= Math.Min(Main.maxTilesX - 1, centerX + scale * 2); i++)
                {
                    for (int j = Math.Max(0, centerY - scale * 2); j <= Math.Min(Main.maxTilesY - 4, centerY + scale * 2); j++)
                    {
                        if (Main.tile[i, j].HasTile && !protectedTiles.Contains(Main.tile[i, j].TileType))
                        {
                            double x = (centerX - i) / (double)scale * Math.Cos(rot) - (centerY - j) / (double)scale * Math.Sin(rot);
                            double y = (centerY - j) / (double)scale * Math.Cos(rot) + (centerX - i) / (double)scale * Math.Sin(rot);

                            for (int iterations = 0; iterations < 10; iterations++)
                            {
                                double newX = x * x - y * y + varX;
                                double newY = 2 * x * y + varY;
                                x = newX;
                                y = newY;

                                if (x * x + y * y > 4)
                                {
                                    break;
                                }
                            }
                            if (x * x + y * y < 4)
                            {
                                Tile tile = Framing.GetTileSafely(i, j);

                                tile.TileType = (ushort)TileType<LightslateTile>();

                                if ((!Framing.GetTileSafely(i, j + 1).HasTile || protectedTiles.Contains(Framing.GetTileSafely(i, j + 1).TileType)) ||
                                    ((!Framing.GetTileSafely(i, j + 2).HasTile || protectedTiles.Contains(Framing.GetTileSafely(i, j + 2).TileType)) && WorldGen.genRand.NextBool(2)) ||
                                    ((!Framing.GetTileSafely(i, j + 3).HasTile || protectedTiles.Contains(Framing.GetTileSafely(i, j + 3).TileType)) && WorldGen.genRand.NextBool(3)))
                                {
                                    tile.TileType = (ushort)TileType<FractalMatterTile>();
                                }
                            }
                        }
                    }
                }
            }

            for (int a = 0; a < NUM_FRACTAL_ORE_VEINS; a++)
            {
                progress.Set(0.33f + a / (float)NUM_FRACTAL_ORE_VEINS / 3);

                int centerX = 0;
                int centerY = 0;
                for (int tries = 0; tries < 256; tries++)
                {
                    centerX = WorldGen.genRand.Next(Main.maxTilesX);
                    centerY = WorldGen.genRand.Next(Main.maxTilesY);

                    if (Main.tile[centerX, centerY].HasTile)
                    {
                        break;
                    }
                }
                int scale = WorldGen.genRand.Next(3, 6);

                float rad = 1;//Math.Max(WorldGen.genRand.NextFloat(),WorldGen.genRand.NextFloat());
                float rot = WorldGen.genRand.NextFloat() * MathHelper.TwoPi;

                Vector2 vars = new Vector2(rad, 0).RotatedBy(rot) + new Vector2(-1, 0);

                vars = new Vector2(1 - vars.X * vars.X + vars.Y * vars.Y, -2 * vars.X * vars.Y) / 4;

                float varX = vars.X;
                float varY = vars.Y;

                for (int i = Math.Max(0, centerX - scale * 2); i <= Math.Min(Main.maxTilesX - 1, centerX + scale * 2); i++)
                {
                    for (int j = Math.Max(0, centerY - scale * 2); j <= Math.Min(Main.maxTilesY - 1, centerY + scale * 2); j++)
                    {
                        if (Main.tile[i, j].HasTile && !protectedTiles.Contains(Main.tile[i, j].TileType))
                        {
                            double x = (centerX - i) / (double)scale * Math.Cos(rot) - (centerY - j) / (double)scale * Math.Sin(rot);
                            double y = (centerY - j) / (double)scale * Math.Cos(rot) + (centerX - i) / (double)scale * Math.Sin(rot);

                            for (int iterations = 0; iterations < 10; iterations++)
                            {
                                double newX = x * x - y * y + varX;
                                double newY = 2 * x * y + varY;
                                x = newX;
                                y = newY;

                                if (x * x + y * y > 4)
                                {
                                    break;
                                }
                            }
                            if (x * x + y * y < 4)
                            {
                                WorldGen.PlaceTile(i, j, TileType<FractalOreTile>(), mute: true, forced: true);
                            }
                        }
                    }
                }
            }

            for (int a = 0; a < NUM_DENDRITIC_ORE_VEINS; a++)
            {
                progress.Set(0.66f + a / (float)NUM_DENDRITIC_ORE_VEINS / 3);

                int centerX = 0;
                int centerY = 0;
                for (int tries = 0; tries < 256; tries++)
                {
                    centerX = WorldGen.genRand.Next(Main.maxTilesX);
                    centerY = WorldGen.genRand.Next(skyHeight);

                    if (!Main.tile[centerX, centerY].HasTile)
                    {
                        break;
                    }
                }
                int scale = WorldGen.genRand.Next(12, 24);
                float rot = WorldGen.genRand.NextFloat() * MathHelper.TwoPi;

                //here have a bunch of pre-calculated misiurewicz points
                Vector2 vars = WorldGen.genRand.Next(new Vector2[] {
                        new Vector2(-2,0),
                        new Vector2(-1.839286755f,0),
                        new Vector2(-1.754877666f,0),
                        new Vector2(-1.543689013f,0),
                        new Vector2(0,1),
                        new Vector2(0,-1),
                        new Vector2(0.419643377f,0.606290729f),
                        new Vector2(0.419643377f,-0.606290729f),
                        new Vector2(0.228155493f,1.115142508f),
                        new Vector2(0.228155493f,-1.115142508f),
                        new Vector2(1.239225555f,-0.412602182f),
                        new Vector2(1.239225555f,0.412602182f),
                        new Vector2(-0.155788497f,1.1122171146f),
                        new Vector2(-0.155788497f,-1.1122171146f),
                        new Vector2(0.395014052f,0.555624571f),
                        new Vector2(0.395014052f,-0.555624571f)
                    });

                float varX = vars.X;
                float varY = vars.Y;

                for (int i = Math.Max(0, centerX - scale * 2); i <= Math.Min(Main.maxTilesX - 1, centerX + scale * 2); i++)
                {
                    for (int j = Math.Max(0, centerY - scale * 2); j <= Math.Min(Main.maxTilesY - 1, centerY + scale * 2); j++)
                    {
                        if (!Main.tile[i, j].HasTile)
                        {
                            double x = (centerX - i) / (double)scale * Math.Cos(rot) - (centerY - j) / (double)scale * Math.Sin(rot);
                            double y = (centerY - j) / (double)scale * Math.Cos(rot) + (centerX - i) / (double)scale * Math.Sin(rot);

                            for (int iterations = 0; iterations < 10; iterations++)
                            {
                                double newX = x * x - y * y + varX;
                                double newY = 2 * x * y + varY;
                                x = newX;
                                y = newY;

                                if (x * x + y * y > 4)
                                {
                                    break;
                                }
                            }
                            if (x * x + y * y < 4)
                            {
                                WorldGen.PlaceTile(i, j, TileType<DendriticEnergyTile>(), mute: true, forced: true);
                            }
                        }
                    }
                }
            }
        }

        private void SentinelCavesGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating suspicious caves";

            //PolaritiesSystem.sentinelCaves = new List<Vector2>();
            //PolaritiesSystem.sentinelCaveVars = new List<Vector2>();
            //PolaritiesSystem.sentinelCaveRots = new List<double>();

            //while (PolaritiesSystem.sentinelCaves.Count < NUM_SENTINEL_CAVES)
            //{
            //    progress.Set(PolaritiesSystem.sentinelCaves.Count / (float)NUM_SENTINEL_CAVES);

            //    TryGenSentinelCave();
            //}
        }

        private void SentinelVeinsGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating suspicious ore";

            //for (int c = 0; c < PolaritiesSystem.sentinelCaves.Count; c++)
            //{
            //    progress.Set(c / (float)PolaritiesSystem.sentinelCaves.Count);

            //    GenSentinelVein(PolaritiesSystem.sentinelCaves[c], PolaritiesSystem.sentinelCaveVars[c], PolaritiesSystem.sentinelCaveRots[c]);
            //}
        }

        private void FractalTrapsGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating traps";

            for (int i = 10; i < Main.maxTilesX - 10; i++)
            {
                for (int j = 10; j < Main.maxTilesY - 10; j++)
                {
                    progress.Set((i - 10) / (float)(Main.maxTilesX - 20) + (j - 10) / (float)((Main.maxTilesX - 20) * (Main.maxTilesY - 20)));

                    if (!Framing.GetTileSafely(i, j).HasTile &&
                        !Framing.GetTileSafely(i, j - 1).HasTile &&
                        !Framing.GetTileSafely(i, j - 2).HasTile &&
                        !Framing.GetTileSafely(i - 1, j - 1).HasTile &&
                        !Framing.GetTileSafely(i + 1, j - 1).HasTile &&
                        !Framing.GetTileSafely(i - 1, j).HasTile &&
                        !Framing.GetTileSafely(i + 1, j).HasTile &&
                        !Framing.GetTileSafely(i - 1, j - 2).HasTile &&
                        !Framing.GetTileSafely(i + 1, j - 2).HasTile &&
                        Framing.GetTileSafely(i, j + 1).HasTile &&
                        Main.tileSolid[Framing.GetTileSafely(i, j + 1).TileType] &&
                        !protectedTiles.Contains(Framing.GetTileSafely(i, j + 1).TileType) &&
                        Framing.GetTileSafely(i, j).WallType != 0)
                    {
                        if (WorldGen.genRand.NextBool(64))
                        {
                            int numSpaces = 0;
                            int maxNumSpaces = 0;
                            switch (WorldGen.genRand.Next(3))
                            {
                                case 0:
                                    {
                                        while (!Framing.GetTileSafely(i + maxNumSpaces, j - 1).HasTile || !Main.tileSolid[Framing.GetTileSafely(i + maxNumSpaces, j - 1).TileType])
                                        {
                                            maxNumSpaces++;
                                            if (i + maxNumSpaces > Main.maxTilesX - 10 || maxNumSpaces >= 100)
                                            {
                                                break;
                                            }
                                        }
                                        if (protectedTiles.Contains(Main.tile[i + maxNumSpaces, j - 1].TileType) || maxNumSpaces >= 100)
                                        {
                                            break;
                                        }
                                        WorldGen.PlaceTile(i, j, 135, mute: true, true, -1, 4);

                                        var tile = Main.tile[i, j];
                                        var tileAbove = Main.tile[i, j - 1];
                                        tile.RedWire = true;
                                        tileAbove.RedWire = true;
                                        //left-facing
                                        while (!Framing.GetTileSafely(i + numSpaces, j - 1).HasTile || !Main.tileSolid[Framing.GetTileSafely(i + numSpaces, j - 1).TileType])
                                        {
                                            numSpaces++;
                                            if (i + numSpaces > Main.maxTilesX - 10)
                                            {
                                                break;
                                            }
                                            tile = Main.tile[i + numSpaces, j - 1];
                                            tile.RedWire = true;
                                        }
                                        if (i + numSpaces <= Main.maxTilesX - 10)
                                        {
                                            tile = Main.tile[i + numSpaces, j - 1];
                                            tile.HasTile = true;
                                            tile.TileType = (ushort)TileType<FractalTrap>();
                                            tile.TileFrameX = 0;
                                            tile.TileFrameY = 0;
                                            tile.IsHalfBlock = false;
                                            tile.Slope = 0;
                                            if (WorldGen.genRand.NextBool())
                                            {
                                                Main.tile[i + numSpaces, j - 1].TileFrameY = 18;
                                            }
                                        }
                                    }
                                    break;
                                case 1:
                                    {
                                        while (!Framing.GetTileSafely(i - maxNumSpaces, j - 1).HasTile || !Main.tileSolid[Framing.GetTileSafely(i - maxNumSpaces, j - 1).TileType])
                                        {
                                            maxNumSpaces++;
                                            if (i - maxNumSpaces < 10 || maxNumSpaces >= 100)
                                            {
                                                break;
                                            }
                                        }
                                        if (protectedTiles.Contains(Main.tile[i - maxNumSpaces, j - 1].TileType) || maxNumSpaces >= 100)
                                        {
                                            break;
                                        }
                                        WorldGen.PlaceTile(i, j, 135, mute: true, true, -1, 4);

                                        var tile = Main.tile[i, j];
                                        var tileAbove = Main.tile[i, j - 1];
                                        tile.BlueWire = true;
                                        tileAbove.BlueWire = true;
                                        //right-facing
                                        while (!Framing.GetTileSafely(i - numSpaces, j - 1).HasTile || !Main.tileSolid[Framing.GetTileSafely(i - numSpaces, j - 1).TileType])
                                        {
                                            numSpaces++;
                                            if (i - numSpaces < 10)
                                            {
                                                break;
                                            }
                                            tile = Main.tile[i - numSpaces, j - 1];
                                            tile.BlueWire = true;
                                        }
                                        if (i - numSpaces >= 10)
                                        {
                                            tile = Main.tile[i - numSpaces, j - 1];
                                            tile.HasTile = true;
                                            tile.TileType = (ushort)TileType<FractalTrap>();
                                            tile.TileFrameX = 18;
                                            tile.TileFrameY = 0;
                                            tile.IsHalfBlock = false;
                                            tile.Slope = 0;
                                            if (WorldGen.genRand.NextBool())
                                            {
                                                Main.tile[i - numSpaces, j - 1].TileFrameY = 18;
                                            }
                                        }
                                    }
                                    break;
                                case 2:
                                    {
                                        while (!Framing.GetTileSafely(i, j - 1 - maxNumSpaces).HasTile || !Main.tileSolid[Framing.GetTileSafely(i, j - 1 - maxNumSpaces).TileType])
                                        {
                                            maxNumSpaces++;
                                            if (j - 1 - maxNumSpaces < 10 || maxNumSpaces >= 100)
                                            {
                                                break;
                                            }
                                        }
                                        if (protectedTiles.Contains(Main.tile[i, j - 1 - maxNumSpaces].TileType) || maxNumSpaces >= 100)
                                        {
                                            break;
                                        }
                                        WorldGen.PlaceTile(i, j, 135, mute: true, true, -1, 4);

                                        var tile = Main.tile[i, j];
                                        var tileAbove = Main.tile[i, j - 1];
                                        tile.GreenWire = true;
                                        tileAbove.GreenWire = true;
                                        //down-facing
                                        while (!Framing.GetTileSafely(i, j - 1 - numSpaces).HasTile || !Main.tileSolid[Framing.GetTileSafely(i, j - 1 - numSpaces).TileType])
                                        {
                                            numSpaces++;
                                            if (j - 1 - numSpaces < 10)
                                            {
                                                break;
                                            }
                                            tile = Main.tile[i, j - 1 - numSpaces];
                                            tile.GreenWire = true;
                                        }
                                        if (j - 1 - numSpaces >= 10)
                                        {
                                            tile = Main.tile[i, j - 1 - numSpaces];
                                            tile.HasTile = true;
                                            tile.TileType = (ushort)TileType<FractalTrap>();
                                            tile.TileFrameX = 54;
                                            tile.TileFrameY = 0;
                                            tile.IsHalfBlock = false;
                                            tile.Slope = 0;
                                            if (WorldGen.genRand.NextBool())
                                            {
                                                Main.tile[i, j - 1 - numSpaces].TileFrameY = 18;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void FractalPlantsGenpass(GenerationProgress progress)
        {
            progress.Message = "Growing fractal trees";

            for (int i = 10; i < Main.maxTilesX - 10; i++)
            {
                for (int j = Main.maxTilesY / 4; j < Main.maxTilesY - 10; j++)
                {
                    progress.Set(((i - 10) * (j - Main.maxTilesY / 4)) / ((Main.maxTilesY * 0.75f - 10) * (Main.maxTilesX - 20)));

                    WorldGen.GrowTree(i, j);
                }
            }

            List<Point> fractusHeads = new List<Point>();

            for (int x = 50; x < Main.maxTilesX - 50; x++)
            {
                for (int y = Main.maxTilesY / 2; y < Main.maxTilesY - 50; y++)
                {
                    if (!IsFractalOcean(x, y) && WorldGen.genRand.NextBool(100) &&
                        Main.tile[x, y + 1].TileType == TileType<FractalDustTile>() && Main.tile[x, y + 1].HasTile &&
                        !Main.tile[x, y].HasTile && Main.tile[x, y].LiquidAmount == 0 &&
                        !Main.tile[x, y - 1].HasTile &&
                        !Main.tile[x - 1, y - 1].HasTile &&
                        !Main.tile[x + 1, y - 1].HasTile &&
                        !Main.tile[x, y - 2].HasTile
                        )
                    {
                        WorldGen.PlaceTile(x, y, TileType<FractusBase>(), mute: true);
                        fractusHeads.Add(new Point(x, y));
                    }
                }
            }

            FractusHelper.QuickGrowFractus(fractusHeads);

            for (int x = 10; x < Main.maxTilesX - 10; x++)
            {
                for (int y = 10; y < Main.maxTilesY - 10; y++)
                {
                    if (IsFractalOcean(x, y) && WorldGen.genRand.NextBool(75) &&
                        !Main.tile[x, y].HasTile &&
                        Main.tile[x, y + 1].HasTile && (Main.tile[x, y + 1].TileType == (ushort)TileType<FractalDuststoneTile>() || Main.tile[x, y + 1].TileType == (ushort)TileType<FractalDustTile>())
                        )
                    {
                        WorldGen.PlaceTile(x, y, TileType<StromatolightTile>(), mute: true, style: WorldGen.genRand.Next(4));
                    }
                }
            }
        }

        private void HyphaeGenpass(GenerationProgress progress)
        {
            progress.Message = "Growing hyphae";

            for (int x = 1; x < Main.maxTilesX - 1; x++)
            {
                for (int y = Main.maxTilesY / 2; y < Main.maxTilesY - 1; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    if (tile.WallType != 0 && tile.HasTile)
                    {
                        bool tileFree = false;
                        for (int i = x - 1; i < x + 2; i++)
                        {
                            for (int j = y - 1; j < y + 2; j++)
                            {
                                if (!Main.tile[i, j].HasTile || !Main.tileSolid[Main.tile[i, j].TileType])
                                {
                                    tileFree = true;
                                }
                            }
                        }

                        if (tileFree)
                        {
                            int spread = 1000;

                            //chances are higher for the less common tile types
                            if (tile.TileType == (ushort)TileType<FractalMatterTile>())
                            {
                                if (WorldGen.genRand.NextBool(800))
                                {
                                    Tiles.Hyphae.SpreadCustomGrass(x, y, TileType<FractalMatterTile>(), TileType<Tiles.HyphaeFractalMatter>(), spread, spreadFilaments: true);
                                }
                            }
                            else if (tile.TileType == (ushort)TileType<FractalStrandsTile>())
                            {
                                if (WorldGen.genRand.NextBool(200))
                                {
                                    Tiles.Hyphae.SpreadCustomGrass(x, y, TileType<FractalStrandsTile>(), TileType<Tiles.HyphaeFractalStrands>(), spread, spreadFilaments: true);
                                }
                            }
                            else if (tile.TileType == (ushort)TileType<FractalDuststoneTile>())
                            {
                                if (WorldGen.genRand.NextBool(100))
                                {
                                    Tiles.Hyphae.SpreadCustomGrass(x, y, TileType<FractalDuststoneTile>(), TileType<Tiles.HyphaeFractalDuststone>(), spread, spreadFilaments: true);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FractalChestsGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating loot chests";

            ResetFractalChestData();

            for (int i = 30; i < Main.maxTilesX - 30; i++)
            {
                for (int j = 30; j < Main.maxTilesY - 30; j++)
                {
                    progress.Set(i * j / (float)((Main.maxTilesX - 20) * (Main.maxTilesY - 20)));

                    //loot chests
                    if (Framing.GetTileSafely(i, j).WallType != 0 &&
                    !Framing.GetTileSafely(i, j).HasTile &&
                    !Framing.GetTileSafely(i, j + 1).HasTile &&
                    !Framing.GetTileSafely(i + 1, j).HasTile &&
                    !Framing.GetTileSafely(i + 1, j + 1).HasTile &&
                    Framing.GetTileSafely(i, j + 2).HasTile &&
                    Framing.GetTileSafely(i + 1, j + 2).HasTile &&
                    WorldGen.genRand.NextBool(96))
                    {
                        //don't generate intersecting protected tiles
                        bool safeSpot = true;
                        for (int x = i - 3; x < i + 5; x++)
                        {
                            for (int y = j - 5; y < j + 4; y++)
                            {
                                if (protectedTiles.Contains(Main.tile[x, y].TileType) && Main.tile[x, y].HasTile)
                                {
                                    safeSpot = false;
                                    break;
                                }
                            }
                            if (!safeSpot) break;
                        }
                        if (!safeSpot) continue;

                        bool genShrine = WorldGen.genRand.NextBool();

                        GenFractalLootChest(i, j);

                        //try to shrine
                        if (genShrine)
                        {
                            int[,] shrineTiles = {
                                { 0, 1, 1, 1, 1, 1, 1, 0 },
                                { 1, 1, 0, 1, 1, 0, 1, 1 },
                                { 1, 0, 0, 0, 0, 0, 0, 1 },
                                { 1, 1, 0, 0, 0, 0, 1, 1 },
                                { 0, 0, 0, 0, 0, 0, 0, 0 },
                                { 0, 0, 0, 2, 2, 0, 0, 0 },
                                { 0, 0, 0, 2, 2, 0, 0, 0 },
                                { 1, 1, 1, 1, 1, 1, 1, 1 },
                                { 0, 1, 1, 1, 1, 1, 1, 0 }
                            };
                            int[,] wallTiles = {
                                { 0, 0, 0, 0, 0, 0, 0, 0 },
                                { 0, 0, 1, 1, 1, 1, 0, 0 },
                                { 0, 1, 1, 1, 1, 1, 1, 0 },
                                { 0, 1, 1, 1, 1, 1, 1, 0 },
                                { 0, 1, 1, 1, 1, 1, 1, 0 },
                                { 0, 1, 1, 1, 1, 1, 1, 0 },
                                { 0, 1, 1, 1, 1, 1, 1, 0 },
                                { 0, 0, 1, 1, 1, 1, 0, 0 },
                                { 0, 0, 0, 0, 0, 0, 0, 0 }
                            };

                            //generate shrine
                            for (int x = 0; x < 8; x++)
                            {
                                for (int y = 0; y < 9; y++)
                                {
                                    int realX = (i - 3) + x;
                                    int realY = (j - 5) + y;

                                    if (shrineTiles[y, x] == 1)
                                    {
                                        Framing.GetTileSafely(realX, realY).HasTile = true;
                                        Framing.GetTileSafely(realX, realY).TileType = (ushort)TileType<FractalBrickTile>();
                                    }
                                    else if (shrineTiles[y, x] == 0)
                                    {
                                        Framing.GetTileSafely(realX, realY).HasTile = false;
                                    }
                                    if (wallTiles[y, x] == 1)
                                    {
                                        Framing.GetTileSafely(realX, realY).WallType = (ushort)WallType<FractalBrickWallPlaced>();
                                    }
                                }
                            }
                            WorldGen.PlaceTile(i - 1, j - 2, ModContent.TileType<FractalTorchTile>(), mute: true);
                            WorldGen.PlaceTile(i + 2, j - 2, ModContent.TileType<FractalTorchTile>(), mute: true);
                        }
                    }
                }
            }
        }

        private void FractalHousesGenpass(GenerationProgress progress)
        {
            progress.Message = "Generating fractal cabins";

            for (int i = 30; i < Main.maxTilesX - 30; i++)
            {
                for (int j = 30; j < Main.maxTilesY - 30; j++)
                {
                    progress.Set(i / (float)(Main.maxTilesX - 60) + j / (float)((Main.maxTilesX - 60) * (Main.maxTilesY - 60)));

                    if (!Framing.GetTileSafely(i, j).HasTile && Framing.GetTileSafely(i, j + 1).HasTile && WorldGen.genRand.NextBool(2048))
                    {
                        GenHouse(i, j);
                    }
                }
            }
        }

        private void StabilizeGenpass(GenerationProgress progress)
        {
            progress.Message = "Stabilizing";

            for (int k = 0; k < Main.maxTilesX; k++)
            {
                for (int l = 0; l < Main.maxTilesY; l++)
                {
                    if (Main.tile[k, l].HasTile && !WorldGen.SolidTile(k, l + 1))
                    {
                        if (Main.tile[k, l].TileType == TileType<LightslateTile>())
                        {
                            Main.tile[k, l].TileType = (ushort)TileType<FractalMatterTile>();
                        }
                        if (Main.tile[k, l].TileType == TileType<FractalDustTile>())
                        {
                            Main.tile[k, l].TileType = (ushort)TileType<FractalDuststoneTile>();
                        }
                    }
                }
            }
        }



        private static void TryGenSentinelCave()
        {
            //const float ARENA_RADIUS = NPCs.SelfsimilarSentinel.SelfsimilarSentinel.ARENA_RADIUS;

            //Vector2 cavePosition = new Vector2(WorldGen.genRand.NextFloat(3 * ARENA_RADIUS, Main.maxTilesX * 16 - 3 * ARENA_RADIUS), WorldGen.genRand.NextFloat(3 * ARENA_RADIUS, Main.maxTilesY * 16 - 3 * ARENA_RADIUS));

            //foreach (Vector2 cavePosition2 in PolaritiesSystem.sentinelCaves)
            //{
            //    if ((cavePosition - cavePosition2).Length() < ARENA_RADIUS * 3) return;
            //}

            //int totalAdjacentTiles = 0;
            //int emptyAdjacentTiles = 0;

            ////check if cave is fully underground with margin and not intersecting protected tiles
            //for (int i = (int)(cavePosition.X - ARENA_RADIUS * 1.5f) / 16; i <= (int)(cavePosition.X + ARENA_RADIUS * 1.5f) / 16; i++)
            //{
            //    for (int j = (int)(cavePosition.Y - ARENA_RADIUS * 1.5f) / 16; j <= (int)(cavePosition.Y + ARENA_RADIUS * 1.5f) / 16; j++)
            //    {
            //        Vector2 tilePosition = new Vector2(i, j) * 16 + new Vector2(8);

            //        if ((tilePosition - cavePosition).Length() < ARENA_RADIUS * 1.5f)
            //        {
            //            //can't generate above ground
            //            if (Main.tile[i, j].wall == 0) return;

            //            //can't generate in ocean
            //            if (IsFractalOcean(i, j)) return;

            //            //can't generate intersecting protected tiles
            //            if ((tilePosition - cavePosition).Length() <= ARENA_RADIUS)
            //            {
            //                if (Main.tile[i, j].active() && protectedTiles.Contains(Main.tile[i, j].type)) return;
            //            }
            //        }
            //        if ((tilePosition - cavePosition).Length() >= ARENA_RADIUS)
            //        {
            //            if ((tilePosition - cavePosition + new Vector2(16, 0)).Length() < ARENA_RADIUS ||
            //                (tilePosition - cavePosition + new Vector2(-16, 0)).Length() < ARENA_RADIUS ||
            //                (tilePosition - cavePosition + new Vector2(0, 16)).Length() < ARENA_RADIUS ||
            //                (tilePosition - cavePosition + new Vector2(0, -16)).Length() < ARENA_RADIUS)
            //            {
            //                totalAdjacentTiles++;
            //                if (!Main.tile[i, j].active()) emptyAdjacentTiles++;
            //            }
            //        }
            //    }
            //}

            ////we need a certain amount of adjacent tiles to be empty so that we have some openings to enter through but not enough to make the arena unreadable
            //if (totalAdjacentTiles < emptyAdjacentTiles * 3 || totalAdjacentTiles > emptyAdjacentTiles * 4) return;


            ////we have successfully found a position!
            //PolaritiesSystem.sentinelCaves.Add(cavePosition);
            ////vars and rotation
            //double rot = WorldGen.genRand.NextDouble() * Math.PI * 2;

            //Vector2 bulbCenter = new Vector2(-0.1225611f, 0.7448617f);
            //Vector2 bulbEdge = new Vector2(-1 / 8f, 3 * (float)Math.Sqrt(3) / 8f);
            //Vector2 vars = bulbCenter + (bulbEdge - bulbCenter).RotatedBy(WorldGen.genRand.NextFloat(MathHelper.TwoPi)) * WorldGen.genRand.NextFloat(1f);
            //vars.Y *= (WorldGen.genRand.NextBool() ? 1 : -1);

            //PolaritiesSystem.sentinelCaveVars.Add(vars);
            //PolaritiesSystem.sentinelCaveRots.Add(rot);

            ////fill it all in with ore temporarily, this blocks other stuff from generating in to the cave
            //for (int i = (int)(cavePosition.X - ARENA_RADIUS) / 16; i <= (int)(cavePosition.X + ARENA_RADIUS) / 16; i++)
            //{
            //    for (int j = (int)(cavePosition.Y - ARENA_RADIUS) / 16; j <= (int)(cavePosition.Y + ARENA_RADIUS) / 16; j++)
            //    {
            //        Vector2 tilePosition = new Vector2(i, j) * 16 + new Vector2(8);

            //        if ((tilePosition - cavePosition).Length() < ARENA_RADIUS)
            //        {
            //            WorldGen.PlaceTile(i, j, TileType<Tiles.SelfsimilarOre>(), mute: true, forced: true);
            //        }
            //    }
            //}
        }

        private static void GenSentinelVein(Vector2 cavePosition, Vector2 vars, double rot, bool onlyGenVein = false)
        {
            //actually generate the thing

            //const float ARENA_RADIUS = NPCs.SelfsimilarSentinel.SelfsimilarSentinel.ARENA_RADIUS;

            //float varX = vars.X;
            //float varY = vars.Y;

            //double veinScale = ARENA_RADIUS / 5;

            //for (int i = (int)(cavePosition.X - ARENA_RADIUS) / 16; i <= (int)(cavePosition.X + ARENA_RADIUS) / 16; i++)
            //{
            //    for (int j = (int)(cavePosition.Y - ARENA_RADIUS) / 16; j <= (int)(cavePosition.Y + ARENA_RADIUS) / 16; j++)
            //    {
            //        Vector2 tilePosition = new Vector2(i, j) * 16 + new Vector2(8);

            //        if ((tilePosition - cavePosition).Length() < ARENA_RADIUS)
            //        {
            //            //remove walls
            //            if (!onlyGenVein)
            //            {
            //                for (int a = -1; a <= 1; a++)
            //                {
            //                    for (int b = -1; b <= 1; b++)
            //                    {
            //                        Main.tile[i + a, j + b].wall = 0;
            //                    }
            //                }
            //            }

            //            //check if it's in the julia set for ore vein
            //            double x = (cavePosition.X - tilePosition.X) / (double)veinScale * Math.Cos(rot) - (cavePosition.Y - tilePosition.Y) / (double)veinScale * Math.Sin(rot);
            //            double y = (cavePosition.Y - tilePosition.Y) / (double)veinScale * Math.Cos(rot) + (cavePosition.X - tilePosition.X) / (double)veinScale * Math.Sin(rot);

            //            for (int iterations = 0; iterations < 16; iterations++)
            //            {
            //                double newX = x * x - y * y + varX;
            //                double newY = 2 * x * y + varY;
            //                x = newX;
            //                y = newY;

            //                if (x * x + y * y > 4)
            //                {
            //                    //funny converging wall pattern
            //                    //I'll only be able to do this if I get it to work with walls
            //                    //So probably not
            //                    /*if (!onlyGenVein)
            //                    {
            //                        if (Main.tile[i, j].wall == WallType<Walls.FractalDuststoneWallNatural>())
            //                        {
            //                            Main.tile[i, j].wall = (ushort)((iterations % 2 == 0) ? WallType<Walls.FractalDuststoneWallNatural>() : WallType<Walls.FractalStrandsWallNatural>());
            //                        }
            //                        else
            //                        {
            //                            Main.tile[i, j].wall = (ushort)((iterations % 2 == 0) ? WallType<Walls.FractalMatterWallNatural>() : WallType<Walls.FractalStrandsWallNatural>());
            //                        }
            //                    }*/

            //                    break;
            //                }
            //            }
            //            if (x * x + y * y <= 4)
            //            {
            //                WorldGen.PlaceTile(i, j, TileType<Tiles.SelfsimilarOre>(), mute: true);
            //            }
            //            else if (!onlyGenVein)
            //            {
            //                Main.tile[i, j].active(false);
            //            }
            //        }
            //    }
            //}

            //if (!onlyGenVein)
            //{
            //    for (int a = -1; a <= 1; a += 2)
            //    {
            //        int j = (int)(cavePosition.Y + a * ARENA_RADIUS / 3) / 16;

            //        bool genLeft = false;
            //        bool genRight = false;
            //        for (int i = (int)(cavePosition.X - ARENA_RADIUS) / 16; i <= (int)(cavePosition.X + ARENA_RADIUS) / 16; i++)
            //        {
            //            if ((new Vector2(i * 16 + 8, j * 16 + 8) - cavePosition).Length() >= ARENA_RADIUS && (new Vector2((i - 1) * 16 + 8, j * 16 + 8) - cavePosition).Length() < ARENA_RADIUS)
            //            {
            //                //on the right side
            //                if (Main.tile[i, j].active() && Main.tileSolid[Main.tile[i, j].type])
            //                {
            //                    genRight = true;
            //                }
            //            }
            //            else if ((new Vector2(i * 16 + 8, j * 16 + 8) - cavePosition).Length() >= ARENA_RADIUS && (new Vector2((i + 1) * 16 + 8, j * 16 + 8) - cavePosition).Length() < ARENA_RADIUS)
            //            {
            //                //on the left side
            //                if (Main.tile[i, j].active() && Main.tileSolid[Main.tile[i, j].type])
            //                {
            //                    genLeft = true;
            //                }
            //            }
            //        }

            //        bool genMiddle = WorldGen.genRand.NextBool();

            //        float leftInwards = WorldGen.genRand.NextFloat(64, ARENA_RADIUS / 3 - 64);
            //        float rightInwards = WorldGen.genRand.NextFloat(64, ARENA_RADIUS / 3 - 64);

            //        float maxLeft = genLeft ? (cavePosition.X - ARENA_RADIUS) :
            //            (cavePosition.X - ARENA_RADIUS / 3 - leftInwards);
            //        float maxRight = genRight ? (cavePosition.X + ARENA_RADIUS) :
            //            (cavePosition.X + ARENA_RADIUS / 3 + rightInwards);

            //        float middleLeft = (cavePosition.X - ARENA_RADIUS / 3 + leftInwards);
            //        float middleRight = (cavePosition.X + ARENA_RADIUS / 3 - rightInwards);

            //        for (int i = (int)(cavePosition.X - ARENA_RADIUS) / 16; i <= (int)(cavePosition.X + ARENA_RADIUS) / 16; i++)
            //        {
            //            Vector2 tilePosition = new Vector2(i, j) * 16 + new Vector2(8);

            //            if ((tilePosition - cavePosition).Length() < ARENA_RADIUS)
            //            {
            //                if ((i > maxLeft / 16 && i < maxRight / 16) &&
            //                    (genMiddle || i < middleLeft / 16 || i > middleRight / 16))
            //                {
            //                    WorldGen.PlaceTile(i, j, TileType<FractalPlatformTile>(), mute: true);
            //                }
            //            }
            //        }

            //        //Generate columns if needed
            //        if (!genLeft || !genMiddle)
            //        {
            //            int i = (int)(cavePosition.X - ARENA_RADIUS / 3) / 16;
            //            int j2 = (a == -1) ? (j - 1) : j;

            //            //generate column
            //            while (j2 < Main.maxTilesY - 1 && j2 > 0 && Main.tile[i, j2].wall == 0)
            //            {
            //                Main.tile[i, j2].wall = (ushort)WallType<Walls.FractalFence>();
            //                j2 += a;
            //            }
            //        }
            //        if (!genRight || !genMiddle)
            //        {
            //            int i = (int)(cavePosition.X + ARENA_RADIUS / 3) / 16;
            //            int j2 = (a == -1) ? (j - 1) : j;

            //            //generate column
            //            while (j2 < Main.maxTilesY - 1 && j2 > 0 && Main.tile[i, j2].wall == 0)
            //            {
            //                Main.tile[i, j2].wall = (ushort)WallType<Walls.FractalFence>();
            //                j2 += a;
            //            }
            //        }
            //    }
            //}
        }


        public static void GenHouse(int x, int y)
        {
            const int minSize = 7;
            const int maxSize = 14;

            List<Rectangle> rooms = new List<Rectangle>
            {
                new Rectangle(x, y, WorldGen.genRand.Next(minSize, maxSize), WorldGen.genRand.Next(minSize, maxSize))
            };
            rooms[0] = new Rectangle(rooms[0].X - rooms[0].Width / 2, rooms[0].Y - rooms[0].Height / 2, rooms[0].Width, rooms[0].Height);

            ModLoader.GetMod("Polarities").Logger.Debug("HouseA");

            Rectangle roomsHull;
            void GetRoomsHull()
            {
                //get the hull of the rooms, every time we add a new room we need to preserve this
                roomsHull = new Rectangle(rooms[0].X, rooms[0].Y, rooms[0].Width, rooms[0].Height);
                foreach (Rectangle room in rooms)
                {
                    if (room.X < roomsHull.X)
                    {
                        roomsHull.Width += roomsHull.X - room.X;
                        roomsHull.X = room.X;
                    }
                    if (room.X + room.Width > roomsHull.X + roomsHull.Width)
                    {
                        roomsHull.Width = room.X + room.Width - roomsHull.X;
                    }

                    if (room.Y < roomsHull.Y)
                    {
                        roomsHull.Height += roomsHull.Y - room.Y;
                        roomsHull.Y = room.Y;
                    }
                    if (room.Y + room.Height > roomsHull.Y + roomsHull.Height)
                    {
                        roomsHull.Height = room.Y + room.Height - roomsHull.Y;
                    }
                }
            }

            bool RoomValid(Rectangle newRoom, ref bool failure, bool movingVertical = false)
            {
                failure = true;
                bool output = true;
                foreach (Rectangle room in rooms)
                {
                    if (room.Intersects(newRoom))
                    {
                        if (movingVertical)
                        {
                            failure = failure && !new Rectangle(room.X + 3, room.Y, room.Width - 6, room.Height).Intersects(newRoom);
                        }
                        else
                        {
                            failure = failure && !new Rectangle(room.X, room.Y + 3, room.Width, room.Height - 6).Intersects(newRoom);
                        }

                        output = false;
                    }
                }
                return output;
            }
            ModLoader.GetMod("Polarities").Logger.Debug("HouseB");

            int numExtraRooms = WorldGen.genRand.Next(1, 5) + WorldGen.genRand.Next(1, 5);
            int roomTries = 0;
            for (int i = 0; i < numExtraRooms && roomTries < 100; i++)
            {
                //we can't do more than 100 attempts to prevent getting stuck
                roomTries++;

                GetRoomsHull();
                //add a room
                Rectangle newRoom = new Rectangle(0, 0, WorldGen.genRand.Next(minSize, maxSize), WorldGen.genRand.Next(minSize, maxSize));
                bool failure = false;
                switch (WorldGen.genRand.Next(4))
                {
                    case 0:
                        newRoom.X = roomsHull.X - newRoom.Width;
                        newRoom.Y = WorldGen.genRand.Next(roomsHull.Y - newRoom.Height + 4, roomsHull.Y + roomsHull.Height - 3);

                        int tries = 0;
                        while (RoomValid(newRoom, ref failure) && tries < roomsHull.Width)
                        {
                            newRoom.X++;
                            tries++;
                        }
                        newRoom.X--;
                        break;
                    case 1:
                        newRoom.X = roomsHull.X + roomsHull.Width;
                        newRoom.Y = WorldGen.genRand.Next(roomsHull.Y - newRoom.Height + 4, roomsHull.Y + roomsHull.Height - 3);

                        tries = 0;
                        while (RoomValid(newRoom, ref failure) && tries < roomsHull.Width)
                        {
                            newRoom.X--;
                            tries++;
                        }
                        newRoom.X++;
                        break;
                    case 2:
                        newRoom.Y = roomsHull.Y - newRoom.Height;
                        newRoom.X = WorldGen.genRand.Next(roomsHull.X - newRoom.Width + 4, roomsHull.X + roomsHull.Width - 3);

                        tries = 0;
                        while (RoomValid(newRoom, ref failure, true) && tries < roomsHull.Height)
                        {
                            newRoom.Y++;
                            tries++;
                        }
                        newRoom.Y--;
                        break;
                    case 3:
                        newRoom.Y = roomsHull.Y + roomsHull.Height;
                        newRoom.X = WorldGen.genRand.Next(roomsHull.X - newRoom.Width + 4, roomsHull.X + roomsHull.Width - 3);

                        tries = 0;
                        while (RoomValid(newRoom, ref failure, true) && tries < roomsHull.Height)
                        {
                            newRoom.Y--;
                            tries++;
                        }
                        newRoom.Y++;
                        break;
                }
                if (!failure)
                {
                    rooms.Add(newRoom);
                }
                else
                {
                    i--;
                }
            }
            ModLoader.GetMod("Polarities").Logger.Debug("HouseC");

            //stop if we're out of the world
            GetRoomsHull();
            if (!WorldGen.InWorld(roomsHull.X, roomsHull.Y, 15) || !WorldGen.InWorld(roomsHull.X + roomsHull.Width, roomsHull.Y + roomsHull.Height, 15)) return;

            ModLoader.GetMod("Polarities").Logger.Debug("HouseD");
            //stop if we're intersecting the outside or a protected tile or a container
            foreach (Rectangle room in rooms)
            {
                for (int i = 0; i <= room.Width; i++)
                {
                    for (int j = 0; j <= room.Height; j++)
                    {
                        if (Framing.GetTileSafely(room.X + i, room.Y + j).WallType == 0 || (protectedTiles.Contains(Framing.GetTileSafely(room.X + i, room.Y + j).TileType) && Framing.GetTileSafely(room.X + i, room.Y + j).HasTile) || Main.tileContainer[Framing.GetTileSafely(room.X + i, room.Y + j).TileType])
                        {
                            return;
                        }
                    }
                }
            }
            ModLoader.GetMod("Polarities").Logger.Debug("HouseE");

            void FillRoom(Rectangle room)
            {
                for (int i = 0; i <= room.Width; i++)
                {
                    for (int j = 0; j <= room.Height; j++)
                    {
                        Framing.GetTileSafely(room.X + i, room.Y + j).HasTile = false;
                        Framing.GetTileSafely(room.X + i, room.Y + j).Slope = 0;
                        Framing.GetTileSafely(room.X + i, room.Y + j).IsHalfBlock = false;
                        if (WorldGen.genRand.NextBool() && i > 0 && i < room.Width && j > 0 && j < room.Height)
                        {
                            Framing.GetTileSafely(room.X + i, room.Y + j).WallType = 0;
                            WorldGen.PlaceWall(room.X + i, room.Y + j, WallType<FractalBrickWallPlaced>(), true);
                        }
                    }
                }
                for (int i = 0; i <= room.Width; i++)
                {
                    Tile tile = Main.tile[room.X + i, room.Y];
                    tile.HasTile = true;
                    tile.IsHalfBlock = false;
                    tile.Slope = 0;
                    tile.TileType = (ushort)TileType<FractalBrickTile>();

                    tile = Main.tile[room.X + i, room.Y + room.Height];
                    tile.HasTile = true;
                    tile.IsHalfBlock = false;
                    tile.Slope = 0;
                    tile.TileType = (ushort)TileType<FractalBrickTile>();
                }
                for (int j = 0; j <= room.Height; j++)
                {
                    Tile tile = Main.tile[room.X, room.Y + j];
                    tile.HasTile = true;
                    tile.IsHalfBlock = false;
                    tile.Slope = 0;
                    tile.TileType = (ushort)TileType<FractalBrickTile>();

                    tile = Main.tile[room.X + room.Width, room.Y + j];
                    tile.HasTile = true;
                    tile.IsHalfBlock = false;
                    tile.Slope = 0;
                    tile.TileType = (ushort)TileType<FractalBrickTile>();
                }
            }

            foreach (Rectangle room in rooms)
            {
                FillRoom(room);
            }
            ModLoader.GetMod("Polarities").Logger.Debug("HouseF");

            //keep track of what directions are available for doors
            //order is bottom, top, right, left
            Dictionary<Rectangle, bool[]> availabilities = new Dictionary<Rectangle, bool[]>();
            foreach (Rectangle room in rooms)
            {
                availabilities.Add(room, new bool[] { true, true, true, true });
            }

            ModLoader.GetMod("Polarities").Logger.Debug("HouseG");
            //for each room, find which rooms it's adjacent to, and create a door/platform between them
            foreach (Rectangle room in rooms)
            {
                foreach (Rectangle room2 in rooms)
                {
                    if (room != room2)
                    {
                        //rooms are different
                        if (new Rectangle(room.X, room.Y, room.Width + 1, room.Height).Intersects(room2))
                        {
                            //rooms are bordering, room is to the left of room2
                            //add in a door
                            int minY = Math.Max(room.Y, room2.Y) + 3;
                            int maxY = Math.Min(room.Y + room.Height, room2.Y + room2.Height);

                            if (minY < maxY)
                            {
                                int doorHeight = WorldGen.genRand.Next(minY, maxY);
                                WorldGen.KillTile(room2.X, doorHeight, noItem: true);
                                WorldGen.KillTile(room2.X, doorHeight - 1, noItem: true);
                                WorldGen.KillTile(room2.X, doorHeight - 2, noItem: true);
                                WorldGen.PlaceObject(room2.X, doorHeight - 1, ModContent.TileType<FractalDoorClosed>(), true);
                            }

                            availabilities[room][2] = false;
                            availabilities[room2][3] = false;
                        }
                        if (new Rectangle(room.X, room.Y, room.Width, room.Height + 1).Intersects(room2))
                        {
                            //rooms are bordering, room is above room2
                            //add in a platform
                            int minX = Math.Max(room.X, room2.X) + 3;
                            int maxX = Math.Min(room.X + room.Width, room2.X + room2.Width);

                            if (minX < maxX)
                            {
                                int platformX = WorldGen.genRand.Next(minX, maxX);
                                WorldGen.KillTile(platformX, room2.Y, noItem: true);
                                WorldGen.KillTile(platformX - 1, room2.Y, noItem: true);
                                WorldGen.KillTile(platformX - 2, room2.Y, noItem: true);
                                WorldGen.PlaceTile(platformX, room2.Y, TileType<FractalPlatformTile>(), true, forced: true);
                                WorldGen.PlaceTile(platformX - 1, room2.Y, TileType<FractalPlatformTile>(), true, forced: true);
                                WorldGen.PlaceTile(platformX - 2, room2.Y, TileType<FractalPlatformTile>(), true, forced: true);
                            }

                            availabilities[room][0] = false;
                            availabilities[room2][1] = false;
                        }
                    }
                }
            }
            ModLoader.GetMod("Polarities").Logger.Debug("HouseH");

            //doors/platforms to the exterior
            foreach (Rectangle room in rooms)
            {
                if (availabilities[room][0])
                {
                    //bottom is available
                    int minX = room.X + 3;
                    int maxX = room.X + room.Width;
                    int platformX = WorldGen.genRand.Next(minX, maxX);
                    int platformY = room.Y + room.Height;
                    WorldGen.KillTile(platformX, platformY, noItem: true);
                    WorldGen.KillTile(platformX - 1, platformY, noItem: true);
                    WorldGen.KillTile(platformX - 2, platformY, noItem: true);
                    WorldGen.PlaceTile(platformX, platformY, TileType<FractalPlatformTile>(), true, forced: true);
                    WorldGen.PlaceTile(platformX - 1, platformY, TileType<FractalPlatformTile>(), true, forced: true);
                    WorldGen.PlaceTile(platformX - 2, platformY, TileType<FractalPlatformTile>(), true, forced: true);
                }
                if (availabilities[room][1])
                {
                    //top is available
                    int minX = room.X + 3;
                    int maxX = room.X + room.Width;
                    int platformX = WorldGen.genRand.Next(minX, maxX);
                    int platformY = room.Y;
                    WorldGen.KillTile(platformX, platformY, noItem: true);
                    WorldGen.KillTile(platformX - 1, platformY, noItem: true);
                    WorldGen.KillTile(platformX - 2, platformY, noItem: true);
                    WorldGen.PlaceTile(platformX, platformY, TileType<FractalPlatformTile>(), true, forced: true);
                    WorldGen.PlaceTile(platformX - 1, platformY, TileType<FractalPlatformTile>(), true, forced: true);
                    WorldGen.PlaceTile(platformX - 2, platformY, TileType<FractalPlatformTile>(), true, forced: true);
                }
                if (availabilities[room][2])
                {
                    //right is available
                    int minY = room.Y + 3;
                    int maxY = room.Y + room.Height;
                    int doorX = room.X + room.Width;
                    int doorY = WorldGen.genRand.Next(minY, maxY);
                    WorldGen.KillTile(doorX, doorY, noItem: true);
                    WorldGen.KillTile(doorX, doorY - 1, noItem: true);
                    WorldGen.KillTile(doorX, doorY - 2, noItem: true);
                    WorldGen.PlaceObject(doorX, doorY - 1, ModContent.TileType<FractalDoorClosed>(), true);
                }
                if (availabilities[room][3])
                {
                    //left is available
                    int minY = room.Y + 3;
                    int maxY = room.Y + room.Height;
                    int doorX = room.X;
                    int doorY = WorldGen.genRand.Next(minY, maxY);
                    WorldGen.KillTile(doorX, doorY, noItem: true);
                    WorldGen.KillTile(doorX, doorY - 1, noItem: true);
                    WorldGen.KillTile(doorX, doorY - 2, noItem: true);
                    WorldGen.PlaceObject(doorX, doorY - 1, ModContent.TileType<FractalDoorClosed>(), true);
                }
            }
            ModLoader.GetMod("Polarities").Logger.Debug("HouseI");

            //random stairways
            foreach (Rectangle room in rooms)
            {
                if (WorldGen.genRand.NextBool())
                {
                    int stairwayX = WorldGen.genRand.Next(room.X + 2, room.X + room.Width);
                    int stairwayY = WorldGen.genRand.Next(room.Y + 2, room.Y + room.Height);
                    int stairwayDirection = WorldGen.genRand.NextBool() ? 1 : -1;

                    int slope = 2;
                    if (stairwayDirection == 1)
                    {
                        slope = 1;
                    }

                    while (stairwayX > room.X && stairwayX < room.X + room.Width && stairwayY > room.Y && stairwayY < room.Y + room.Height)
                    {
                        stairwayX -= stairwayDirection;
                        stairwayY--;
                    }
                    stairwayX += stairwayDirection;
                    stairwayY++;
                    while (stairwayX > room.X && stairwayX < room.X + room.Width && stairwayY > room.Y && stairwayY < room.Y + room.Height)
                    {
                        WorldGen.PlaceTile(stairwayX, stairwayY, TileType<FractalPlatformTile>(), true, forced: true);
                        WorldGen.SlopeTile(stairwayX, stairwayY, slope);
                        WorldGen.TileFrame(stairwayX, stairwayY);

                        if (stairwayY == room.Y + 1 && stairwayX - stairwayDirection > room.X && stairwayX - stairwayDirection < room.X + room.Width)
                        {
                            WorldGen.PlaceTile(stairwayX - stairwayDirection, stairwayY, TileType<FractalPlatformTile>(), true, forced: true);
                        }

                        stairwayX += stairwayDirection;
                        stairwayY++;
                    }
                }
            }
            ModLoader.GetMod("Polarities").Logger.Debug("HouseJ");

            //loot chests, at least one is guaranteed
            Rectangle guaranteedChestRoom = WorldGen.genRand.Next<Rectangle>(rooms);
            foreach (Rectangle room in rooms)
            {
                if (room == guaranteedChestRoom || WorldGen.genRand.NextBool())
                {
                    //generate a loot chest
                    bool gen = false;
                    int attempts = 0;
                    while (!gen && attempts < 1000)
                    {
                        attempts++;
                        int chestX = WorldGen.genRand.Next(room.X + 1, room.X + room.Width - 2);
                        int chestY = room.Y + room.Height - 2;

                        if (!Framing.GetTileSafely(chestX, chestY).HasTile &&
                            !Framing.GetTileSafely(chestX, chestY + 1).HasTile &&
                            !Framing.GetTileSafely(chestX + 1, chestY).HasTile &&
                            !Framing.GetTileSafely(chestX + 1, chestY + 1).HasTile &&
                            Framing.GetTileSafely(chestX, chestY + 2).HasTile &&
                            Framing.GetTileSafely(chestX + 1, chestY + 2).HasTile)
                        {
                            gen = true;
                            GenFractalLootChest(WorldGen.genRand.Next(room.X + 1, room.X + room.Width - 2), room.Y + room.Height - 2);
                        }
                    }
                }
            }
            ModLoader.GetMod("Polarities").Logger.Debug("HouseK");

            //TODO: furniture
        }

        private static int unlockedChestItemIndex = 0;
        private static int lockedChestItemIndex = 0;
        private static int[] itemsToPlaceInUnlockedFractalChests = { };
        private static int[] itemsToPlaceInLockedFractalChests = { };

        //this is initialized in InitGenpass
        private static int[] protectedTiles;

        private static void ResetFractalChestData()
        {
            unlockedChestItemIndex = 0;
            lockedChestItemIndex = 0;
            itemsToPlaceInUnlockedFractalChests = new int[] {
                ItemID.Zenith,
                //ItemType<Items.Weapons.Sawrang>(),
                //    ItemType<Items.Weapons.Gosperian>(),
                //    ItemType<Items.Accessories.FractalAntenna>(),
                //    ItemType<Items.Accessories.AntoinesCharm>(),
                //    ItemType<Items.Accessories.TwistedMirror>(),
                //    ItemType<Items.Weapons.OrthogonalStaff>(),
                //    ItemType<Items.Weapons.MindTwister>(),
                //    ItemType<Items.Weapons.Fractlatl>()
                };
            itemsToPlaceInLockedFractalChests = new int[] {
                //ItemType<Items.Accessories.Wings.FractalWings>(),
                //    ItemType<Items.Weapons.CBow>(),
                //    ItemType<Items.Weapons.EnergyLash>(),
                //    ItemType<Items.Accessories.FractalEye>(),
                //    ItemType<Items.Weapons.BinaryFlux>(),
                //    ItemType<Items.Weapons.CaliperBlades>(),
                //    ItemType<Items.Accessories.FractalAbsorber>(),
                    ItemType<Items.Accessories.ChaosFlower>()
                };
        }

        private static void GenFractalLootChest(int x, int y)
        {
            switch (WorldGen.genRand.Next(2))
            {
                case 0:
                    int chestIndex = WorldGen.PlaceChest(x, y + 1, (ushort)ModContent.TileType<FractalChestTile>(), true, style: 0);
                    if (chestIndex != -1)
                    {
                        //put items in chest
                        Chest chest = Main.chest[chestIndex];
                        chest.item[0].SetDefaults(itemsToPlaceInUnlockedFractalChests[unlockedChestItemIndex]);
                        chest.item[0].Prefix(-1);
                        chest.item[1].SetDefaults(ItemType<FractalBar>());
                        chest.item[1].stack = WorldGen.genRand.Next(2, 7);
                        chest.item[2].SetDefaults(ItemType<Lightslate>());
                        chest.item[2].stack = WorldGen.genRand.Next(5, 16);
                        unlockedChestItemIndex = (unlockedChestItemIndex + 1) % itemsToPlaceInUnlockedFractalChests.Length;
                    }
                    break;
                case 1:
                    int chestIndexB = WorldGen.PlaceChest(x, y + 1, (ushort)ModContent.TileType<FractalChestTile>(), true, style: 1);
                    if (chestIndexB != -1)
                    {
                        //put items in chest
                        Chest chest = Main.chest[chestIndexB];
                        chest.item[0].SetDefaults(itemsToPlaceInLockedFractalChests[lockedChestItemIndex]);
                        chest.item[0].Prefix(-1);
                        chest.item[1].SetDefaults(ItemType<SelfsimilarBar>());
                        chest.item[1].stack = WorldGen.genRand.Next(3, 7);
                        chest.item[2].SetDefaults(ItemType<DendriticEnergy>());
                        chest.item[2].stack = WorldGen.genRand.Next(11, 23);
                        chest.item[3].SetDefaults(ItemType<Lightslate>());
                        chest.item[3].stack = WorldGen.genRand.Next(5, 16);
                        lockedChestItemIndex = (lockedChestItemIndex + 1) % itemsToPlaceInLockedFractalChests.Length;
                    }
                    break;
            }
        }

        public static bool IsFractalOcean(int x, int y)
        {
            float xFrac = (x + ((x > Main.maxTilesX / 2) ? 20 : -20)) / (float)Main.maxTilesX;
            float yFrac = (y + 20) / (float)Main.maxTilesY;

            return yFrac > 2 - 3 * (2 * xFrac - 1) * (2 * xFrac - 1);
        }

        public double expInterpolate(double start, double finish, double progress, double baseVal)
        {
            return (Math.Pow(baseVal, progress) - 1) / (baseVal - 1) * (finish - start) + start;
        }

        public Vector2 FindRandomVariance(int i, int j)
        {
            float x = (float)(Math.Sin(randOffsets[0] + i / 80f) + Math.Sin(randOffsets[1] + j / 80f) + Math.Sin(randOffsets[2] + (i + j) / (80f * Math.Sqrt(2))) + Math.Sin(randOffsets[3] + (i - j) / (80f * Math.Sqrt(2))));
            float y = (float)(Math.Sin(randOffsets[4] + i / 80f) + Math.Sin(randOffsets[5] + j / 80f) + Math.Sin(randOffsets[6] + (i + j) / (80f * Math.Sqrt(2))) + Math.Sin(randOffsets[7] + (i - j) / (80f * Math.Sqrt(2))));
            return new Vector2(x, y);
        }

        private BigRational UCoefficient(int n, int k)
        {
            if (knownUCoefficients[n, k])
            {
                return uCoefficients[n, k];
            }

            knownUCoefficients[n, k] = true;

            if (k == Math.Pow(2, n) - 1)
            {
                uCoefficients[n, k] = new BigRational(1, 1);
                return new BigRational(1, 1);
            }
            else if (k < Math.Pow(2, n) - 1)
            {
                BigRational outVal = new BigRational(0, 1);
                for (int j = 0; j <= k; j++)
                {
                    outVal += UCoefficient(n - 1, j) * UCoefficient(n - 1, k - j);
                }
                uCoefficients[n, k] = outVal;
                return outVal;
            }
            else if (k < Math.Pow(2, n + 1) - 1)
            {
                uCoefficients[n, k] = new BigRational(0, 1);
                return new BigRational(0, 1);
            }
            else
            {
                BigRational val = new BigRational(0, 1);
                for (int j = 1; j <= k - 1; j++)
                {
                    val += UCoefficient(n, j) * UCoefficient(n, k - j);
                }
                uCoefficients[n, k] = (UCoefficient(n + 1, k) - val) / 2;
                return (UCoefficient(n + 1, k) - val) / 2;
            }
        }

        private Vector2 MandelbrotBoundary(double angle)
        {
            Vector2 unit = new Vector2(1, 0);
            Vector2 output = unit.RotatedBy(-angle);

            for (int i = 0; i < COEFFICIENTCOUNT; i++)
            {
                output += (float)bCoefficients[i] * unit.RotatedBy(i * angle).SafeNormalize(Vector2.Zero);
            }

            return output;
        }

        //fractal spawn conditions!

        //biome conditions
        public static float SpawnConditionFractalWaters(NPCSpawnInfo info)
        {
            return info.Water ? 1f : 0f;
        }
        public static float SpawnConditionFractalCoasts(NPCSpawnInfo info)
        {
            return (IsFractalOcean(info.SpawnTileX, info.SpawnTileY) && !info.Water) ? 1f : 0f;
        }
        public static float SpawnConditionFractalWastes(NPCSpawnInfo info)
        {
            return ((Main.tile[info.SpawnTileX, info.SpawnTileY].WallType == WallType<FractalDuststoneWallNatural>() || info.SpawnTileType == TileType<FractalDustTile>() || info.SpawnTileType == TileType<FractalDuststoneTile>() || info.SpawnTileType == TileType<HyphaeFractalDuststone>()) && !IsFractalOcean(info.SpawnTileX, info.SpawnTileY) && !info.Water) ? 1f : 0f;
        }
        public static float SpawnConditionFractalNormal(NPCSpawnInfo info)
        {
            return (!(Main.tile[info.SpawnTileX, info.SpawnTileY].WallType == WallType<FractalDuststoneWallNatural>() || info.SpawnTileType == TileType<FractalDustTile>() || info.SpawnTileType == TileType<FractalDuststoneTile>() || info.SpawnTileType == TileType<HyphaeFractalDuststone>()) && !IsFractalOcean(info.SpawnTileX, info.SpawnTileY) && !info.Water) ? 1f : 0f;
        }

        //depth conditions
        public static float SpawnConditionFractalSky(NPCSpawnInfo info)
        {
            return info.SpawnTileY < skyHeight ? 1f : 0f;
        }
        public static float SpawnConditionFractalOverworld(NPCSpawnInfo info)
        {
            return (Main.tile[info.SpawnTileX, info.SpawnTileY].WallType == 0 && info.SpawnTileY >= skyHeight) ? 1f : 0f;
        }
        public static float SpawnConditionFractalUnderground(NPCSpawnInfo info)
        {
            return ((Main.tile[info.SpawnTileX, info.SpawnTileY].WallType != 0 && info.SpawnTileY >= skyHeight)) ? 1f : 0f;
        }

        //are we in a sentinel cave
        public static bool InSentinelCave(Vector2 position)
        {
            //Vector2 worldPosition = NPCs.SelfsimilarSentinel.SelfsimilarSentinel.GetNearestArenaPosition(position);

            //if ((worldPosition - position).Length() > NPCs.SelfsimilarSentinel.SelfsimilarSentinel.ARENA_RADIUS)
            //{
            //    return true;
            //}
            return false;
        }


        //reset dimension
        public static void ResetDimension()
        {
            //despawn fractal enemies
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].townNPC)
                    Main.npc[i].active = false;
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (!Main.projPet[Main.projectile[i].type])
                    Main.projectile[i].active = false;
            }

            //regenerate sentinel veins
            //for (int i = 0; i < PolaritiesSystem.sentinelCaves.Count; i++)
            //{
            //    GenSentinelVein(PolaritiesSystem.sentinelCaves[i], PolaritiesSystem.sentinelCaveVars[i], PolaritiesSystem.sentinelCaveRots[i], onlyGenVein: true);
            //}
        }
    }

    public class FractalSystem : ModSystem
    {
        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            FractalWastesBiome.TileCount = tileCounts[ModContent.TileType<FractalDustTile>()] + tileCounts[ModContent.TileType<FractalDuststoneTile>()] + tileCounts[ModContent.TileType<HyphaeFractalDuststone>()];
        }
    }
}