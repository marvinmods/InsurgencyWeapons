﻿using InsurgencyWeapons.Helpers;
using InsurgencyWeapons.Projectiles.AssaultRifles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InsurgencyWeapons.Items.Weapons.AssaultRifles
{
    internal class ASVAL : AssaultRifle
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.knockBack = 4f;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = Item.useTime = 4;
            Item.width = 76;
            Item.height = 22;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.damage = 7;
            Item.shootSpeed = 11f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = Item.sellPrice(0, 4, 0, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.DamageType = DamageClass.Ranged;
            WeaponHeldProjectile = ModContent.ProjectileType<ASVALHeld>();
        }

        public override void AddRecipes()
        {
            this.RegisterINS2RecipeWeapon(230);
        }
    }
}