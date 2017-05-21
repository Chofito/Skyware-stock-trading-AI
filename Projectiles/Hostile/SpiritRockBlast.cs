﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Projectiles.Hostile
{
    public class SpiritRockBlast : ModProjectile
    {
        public override void SetDefaults()
        {
			projectile.hostile = true;
			projectile.name = "Rock Bolt";
			projectile.width = 28;
			projectile.height = 28;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 14);
            {
                for (int num621 = 0; num621 < 15; num621++)
                {
                    int num622 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 109, 0f, 0f, 100, default(Color), 2f);
                }
            }
        }
        
        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
            projectile.velocity.Y *= 1.01f;
            projectile.velocity.X *= 1.01f;
            {
                if (projectile.owner == Main.myPlayer && projectile.timeLeft <= 3)
                {
                    projectile.tileCollide = false;
                    projectile.ai[1] = 0f;
                    projectile.alpha = 255;
                    projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
                    projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
                    projectile.width = 30;
                    projectile.height = 30;
                    projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
                    projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
                    int lmao = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 207, 0f, 0f, 100, default(Color), 2f);
                    projectile.knockBack = 4f;
                }
            }
        }
    }
}