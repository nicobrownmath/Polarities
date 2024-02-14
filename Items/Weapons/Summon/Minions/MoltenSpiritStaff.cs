using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Polarities.Buffs;
using Terraria.DataStructures;
using Polarities.Items.Placeable.Bars;
using System.Collections.Generic;
using Polarities.Effects;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;

namespace Polarities.Items.Weapons.Summon.Minions
{
	public class MoltenSpiritStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(80, 3, 0);
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;

			Item.width = 58;
			Item.height = 58;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 1;
			Item.noMelee = true;

			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;
			Item.buffType = BuffType<MoltenSpiritMinionBuff>();
			Item.shoot = ProjectileType<MoltenSpiritMinion>();

			Item.value = 100000;
			Item.rare = ItemRarityID.Yellow;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 18000, true);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback);
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<MantellarBar>(), 12)
				.AddIngredient(ItemID.ImpStaff)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class MoltenSpiritMinionBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ProjectileType<MoltenSpiritMinion>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	public class MoltenSpiritMinion : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 4;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			DrawOffsetX = -4;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 0;

			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minionSlots = 1f;
			Projectile.friendly = true;
			Projectile.tileCollide = false;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (!player.active)
			{
				Projectile.active = false;
				return;
			}
			if (player.dead)
			{
				player.ClearBuff(BuffType<MoltenSpiritMinionBuff>());
			}
			if (player.HasBuff(BuffType<MoltenSpiritMinionBuff>()))
			{
				Projectile.timeLeft = 2;
			}

			//ai[0] corresponds to the timer
			//ai[1] corresponds to the target
			//localAI[0] corresponds to which 'side' of the enemy we're on

			//set target
			int targetID = -1;
			Projectile.Minion_FindTargetInRange(750, ref targetID, false);
			Projectile.ai[1] = targetID;

			int index = 0;
			int ownedProjectiles = 0;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].ai[1] == Projectile.ai[1])
				{
					ownedProjectiles++;
					if (i < Projectile.whoAmI)
					{
						index++;
					}
				}
			}

			float advancePerAttack = 4;

			Vector2 goalPosition = player.Center;
			Vector2 goalVelocity = player.velocity;
			bool doAttacks = false;
			float goalAngle = PolaritiesSystem.timer * (MathHelper.TwoPi / 20f / advancePerAttack) + index * MathHelper.TwoPi / ownedProjectiles;

			if (Projectile.ai[1] != -1)
			{
				goalPosition = Main.npc[(int)Projectile.ai[1]].Center;
				goalVelocity = Main.npc[(int)Projectile.ai[1]].velocity;
				doAttacks = true;
				goalAngle = PolaritiesSystem.timer * (MathHelper.TwoPi / 20f / advancePerAttack) + index * MathHelper.TwoPi / advancePerAttack / ownedProjectiles + MathHelper.TwoPi / advancePerAttack * Projectile.localAI[0];
			}

			//motion code
			if (doAttacks)
			{
				if (Projectile.ai[0] < 0)
					Projectile.ai[0]++;

				if (Projectile.ai[0] == 0 && Main.rand.NextBool(60))
				{
					//do attack
					Projectile.localAI[0] = (Projectile.localAI[0] + 1) % advancePerAttack;

					Projectile.ai[0] = -20;
					Projectile.velocity = goalVelocity + (goalPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * 16;
				}
			}
			else
			{
				Projectile.ai[0] = 0;
				Projectile.localAI[0] = Main.rand.Next((int)advancePerAttack);
			}

			if (Projectile.ai[0] >= 0)
			{
				goalPosition += new Vector2(160, 0).RotatedBy(goalAngle);

				goalVelocity += (goalPosition - Projectile.Center) / 8;
				Projectile.velocity += (goalVelocity - Projectile.velocity) / 8;
			}

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if ((Projectile.Center - player.Center).Length() > 2000)
			{
				Projectile.position = player.Center + (Projectile.position - Projectile.Center);
			}

			Projectile.frameCounter++;
			if (Projectile.frameCounter == 4)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 4;
			}

			Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3());
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return Projectile.ai[0] < 0;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.ai[1] = 0;

			target.AddBuff(BuffID.OnFire, 240);

			target.immune[Projectile.owner] = 0;

			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

			Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<MoltenSpiritMinionExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float drawAlpha = 0.5f;

			Texture2D texture = Textures.Glow256.Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
			Rectangle mainFrame = mainTexture.Frame(1, 4, 0, Projectile.frame);
			Vector2 mainCenter = mainFrame.Size() / 2;

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] != Vector2.Zero)
				{
					float progress = 1 - i / (float)Projectile.oldPos.Length;
					Color color = ModUtils.ConvectiveFlameColor(progress * progress / 2f) * drawAlpha * progress;
					Main.EntitySpriteDraw(mainTexture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, mainFrame, color, Projectile.oldRot[i], mainCenter, progress, SpriteEffects.None, 0);
				}
			}

			for (int i = 1; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] != Vector2.Zero && (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() < 1000)
				{
					float progress = 1 - i / (float)Projectile.oldPos.Length;
					Vector2 scale = new Vector2(progress * 0.15f, (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() * 0.02f);
					Color color = ModUtils.ConvectiveFlameColor(progress * progress / 2f) * drawAlpha * progress;
					Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color, Projectile.oldRot[i], center, scale, SpriteEffects.None, 0);
				}
			}

			return false;
		}
	}

	public class MoltenSpiritMinionExplosion : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Glow256";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.scale = 0f;
			Projectile.timeLeft = 10;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.DamageType = DamageClass.Summon;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Vector2 oldCenter = Projectile.Center;

			Projectile.scale = 1 - Projectile.timeLeft / 10f;
			Projectile.width = (int)(128 * Projectile.scale);
			Projectile.height = (int)(128 * Projectile.scale);

			Projectile.Center = oldCenter;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

		public override bool PreDraw(ref Color lightColor)
		{
			float progress = Projectile.timeLeft / 10f;

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			for (int i = 1; i <= 4; i++)
			{
				Color color = ModUtils.ConvectiveFlameColor(progress * progress * i / 4f) * (progress * 2f);
				float drawScale = Projectile.width / 128f * i / 4f;
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, color, 0f, center, drawScale, SpriteEffects.None, 0);
			}

			return false;
		}
	}
}