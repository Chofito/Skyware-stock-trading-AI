using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Projectiles.Sword.Artifact
{
    public class Thanos4Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Thanos Afterimage");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 22;
            projectile.aiStyle = 113;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = 4;
            projectile.timeLeft = 600;
            projectile.alpha = 255;
            projectile.extraUpdates = 1;
            projectile.light = 0;
            aiType = ProjectileID.Shuriken;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.25f, projectile.height * 0.25f);
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, projectile.gfxOffY);
                Color color = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, color, projectile.rotation, drawOrigin, projectile.scale, SpriteEffects.None, 0f);
            }
            return true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Main.rand.Next(15) == 0)
            {
                target.AddBuff(mod.BuffType("Crystallize"), 240, true);
            }
            if (Main.rand.Next(6) == 0)
            {
                int n = 6;
                int deviation = Main.rand.Next(0, 300);
                for (int i = 0; i < n; i++)
                {
                    float rotation = MathHelper.ToRadians(270 / n * i + deviation);
                    Vector2 perturbedSpeed = new Vector2(projectile.velocity.X, projectile.velocity.Y).RotatedBy(rotation);
                    perturbedSpeed.Normalize();
                    perturbedSpeed.X *= 5.5f;
                    perturbedSpeed.Y *= 5.5f;
                    Projectile.NewProjectile(target.Center.X, target.Center.Y, perturbedSpeed.X, perturbedSpeed.Y, mod.ProjectileType("AncientCrystal"), 34, 2, projectile.owner);
                }
            }
        }
        public override bool PreAI()
        {
            projectile.rotation += 0.3f;
            return true;
        }

        public override void AI()
        {
            projectile.tileCollide = true;
            int dust2 = Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, mod.DustType("Crystal"), 0f, 0f);
            Main.dust[dust2].scale = 1.0f;
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].velocity *= 0f;
            int dust3 = Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, 187, 0f, 0f);
            Main.dust[dust3].scale = 1.0f;
            Main.dust[dust3].noGravity = true;
            Main.dust[dust3].velocity *= 0f;

        }
        public override void Kill(int timeLeft)
        {
            if (Main.rand.Next(6) == 0)
            {
                Player player = Main.player[projectile.owner];
                int amount = Main.rand.Next(1, 3);
                for (int i = 0; i < amount; ++i)
                {
                    Vector2 position = new Vector2(player.position.X + player.width * 0.5f + Main.rand.Next(-200, 201), player.Center.Y - 600f);
                    position.X = (position.X * 10f + player.position.X) / 11f + (float)Main.rand.Next(-100, 101);
                    position.Y -= 150;
                    float speedX = player.position.X + player.width * 0.5f + Main.rand.Next(-200, 201) - position.X;
                    float speedY = player.Center.Y - position.Y;
                    if (speedY < 0f)
                        speedY *= -1f;
                    if (speedY < 20f)
                        speedY = 20f;

                    float length = (float)Math.Sqrt((double)(speedX * speedX + speedY * speedY));
                    length = 12 / length;
                    speedX *= length;
                    speedY *= length;
                    speedX = speedX + (float)Main.rand.Next(-40, 41) * 0.03f;
                    speedY = speedY + (float)Main.rand.Next(-40, 41) * 0.03f;
                    speedX *= (float)Main.rand.Next(75, 150) * 0.01f;
                    position.X += (float)Main.rand.Next(-50, 51);
                    Projectile.NewProjectile(position.X, position.Y, speedX, speedY, mod.ProjectileType("PrimordialEnergy"), 50, 1, player.whoAmI);
                }
            }
                Main.PlaySound(0, (int)projectile.position.X, (int)projectile.position.Y, 0);
            int newDust = Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Crystal"), 0f, 0f, 0, default(Color), 1f);
            Main.dust[newDust].scale = 2f;
            int newDust1 = Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Crystal"), 0f, 0f, 0, default(Color), 1f);
            Main.dust[newDust1].scale = 2f;
            int newDust2 = Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Crystal"), 0f, 0f, 0, default(Color), 1f);
            Main.dust[newDust2].scale = 2f;
            int newDust3 = Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Crystal"), 0f, 0f, 0, default(Color), 1f);
            Main.dust[newDust3].scale = 2f;
        }
    }
}
