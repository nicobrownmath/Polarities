using Microsoft.Xna.Framework;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Consumables
{
    public class TwistedRecallPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 20;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.RecallPotion);
        }

        public override void HoldItem(Player player)
        {
            if (player.itemAnimation > 0)
            {
                if (player.itemTime == 0)
                {
                    player.itemTime = CombinedHooks.TotalUseTime(Item.useTime, player, Item);
                }
                else if (player.itemTime == 2)
                {
                    for (int num357 = 0; num357 < 70; num357++)
                    {
                        Dust obj28 = Main.dust[Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, Color.Cyan, 1.2f)];
                        obj28.velocity *= 0.5f;
                    }
                    player.grappling[0] = -1;
                    player.grapCount = 0;
                    for (int num356 = 0; num356 < 1000; num356++)
                    {
                        if (Main.projectile[num356].active && Main.projectile[num356].owner == player.whoAmI && Main.projectile[num356].aiStyle == 7)
                        {
                            Main.projectile[num356].Kill();
                        }
                    }
                    bool flag27 = player.immune;
                    int num355 = player.immuneTime;

                    Spawn(player);
                    FractalSubworld.ResetDimension();
                    //if (player.HasBuff(BuffType<FractalSubworldDebuff>()))
                    //    player.DelBuff(player.FindBuffIndex(BuffType<FractalSubworldDebuff>()));
                    //player.GetModPlayer<PolaritiesPlayer>().fractalSubworldDebuffTimer = 0;

                    player.immune = flag27;
                    player.immuneTime = num355;
                    for (int num354 = 0; num354 < 70; num354++)
                    {
                        Dust obj29 = Main.dust[Dust.NewDust(player.position, player.width, player.height, 15, 0f, 0f, 150, Color.Cyan, 1.2f)];
                        obj29.velocity *= 0.5f;
                    }
                    if (ItemLoader.ConsumeItem(Item, player) && Item.stack > 0)
                    {
                        Item.stack--;
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater)
                .AddIngredient<Mirrorfish>()
                .AddIngredient<FractalOre>()
                .AddTile(TileID.Bottles)
                .Register();
        }

        private void Spawn(Player player)
        {
            if (!FractalSubworld.Active)
            {
                FractalSubworld.DoEnter();
            }
            //Main.InitLifeBytes();
            if (player.whoAmI == Main.myPlayer)
            {
                if (Main.mapTime < 5)
                {
                    Main.mapTime = 5;
                }
                //Main.quickBG = 10;
                player.FindSpawn();
                if (!Player.CheckSpawn(player.SpawnX, player.SpawnY))
                {
                    player.SpawnX = -1;
                    player.SpawnY = -1;
                }
                Main.maxQ = true;
            }
            if (Main.netMode == 1 && player.whoAmI == Main.myPlayer)
            {
                NetMessage.SendData(12, -1, -1, null, Main.myPlayer);
                Main.gameMenu = false;
            }
            player.headPosition = Vector2.Zero;
            player.bodyPosition = Vector2.Zero;
            player.legPosition = Vector2.Zero;
            player.headRotation = 0f;
            player.bodyRotation = 0f;
            player.legRotation = 0f;
            player.lavaTime = player.lavaMax;
            if (player.statLife <= 0)
            {
                int num = player.statLifeMax2 / 2;
                player.statLife = 100;
                if (num > player.statLife)
                {
                    player.statLife = num;
                }
                player.breath = player.breathMax;
                if (player.spawnMax)
                {
                    player.statLife = player.statLifeMax2;
                    player.statMana = player.statManaMax2;
                }
            }
            player.immune = true;
            if (player.dead)
            {
                PlayerLoader.OnRespawn(player);
            }
            player.dead = false;
            player.immuneTime = 0;
            player.active = true;
            if (player.SpawnX >= 0 && player.SpawnY >= 0)
            {
                player.position.X = player.SpawnX * 16 + 8 - player.width / 2;
                player.position.Y = player.SpawnY * 16 - player.height;
            }
            else
            {
                player.position.X = Main.spawnTileX * 16 + 8 - player.width / 2;
                player.position.Y = Main.spawnTileY * 16 - player.height;
                for (int k = Main.spawnTileX - 1; k < Main.spawnTileX + 2; k++)
                {
                    for (int i = Main.spawnTileY - 3; i < Main.spawnTileY; i++)
                    {
                        if (Main.tile[k, i] != null)
                        {
                            if (Main.tileSolid[Main.tile[k, i].TileType] && !Main.tileSolidTop[Main.tile[k, i].TileType])
                            {
                                WorldGen.KillTile(k, i);
                            }
                            if (Main.tile[k, i].LiquidType > 0)
                            {
                                var tile = Main.tile[k, i];
                                tile.LiquidType = 0;
                                tile.LiquidAmount = 0;
                                WorldGen.SquareTileFrame(k, i);
                            }
                        }
                    }
                }
            }
            player.wet = false;
            player.wetCount = 0;
            player.lavaWet = false;
            player.fallStart = (int)(player.position.Y / 16f);
            player.fallStart2 = player.fallStart;
            player.velocity.X = 0f;
            player.velocity.Y = 0f;
            for (int j = 0; j < 3; j++)
            {
                player.UpdateSocialShadow();
            }
            player.oldPosition = player.position + player.BlehOldPositionFixer;
            player.SetTalkNPC(-1);
            if (player.whoAmI == Main.myPlayer)
            {
                Main.npcChatCornerItem = 0;
            }
            if (player.pvpDeath)
            {
                player.pvpDeath = false;
                player.immuneTime = 300;
                player.statLife = player.statLifeMax;
            }
            else
            {
                player.immuneTime = 60;
            }
            if (player.whoAmI == Main.myPlayer)
            {
                Main.BlackFadeIn = 255;
                Main.renderNow = true;
                //if (Main.netMode == 1)
                //{
                //    Netplay.newRecent();
                //}
                Main.screenPosition.X = player.position.X + player.width / 2 - Main.screenWidth / 2;
                Main.screenPosition.Y = player.position.Y + player.height / 2 - Main.screenHeight / 2;
            }
        }
    }
}
