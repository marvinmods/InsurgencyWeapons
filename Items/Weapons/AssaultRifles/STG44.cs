﻿using InsurgencyWeapons.Helpers;
using InsurgencyWeapons.Projectiles.AssaultRifles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InsurgencyWeapons.Items.Weapons.AssaultRifles
{
    internal class STG44 : AssaultRifle
    {
        private int STGType => ModContent.ProjectileType<STG44Held>();

        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.knockBack = 4f;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = Item.useTime = 6;
            Item.width = 72;
            Item.height = 24;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.damage = 9;
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = Item.sellPrice(0, 4, 0, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.DamageType = DamageClass.Ranged;
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[STGType] < 1)
            {
                int damage = (int)player.GetTotalDamage(DamageClass.Ranged).ApplyTo(Item.damage);
                Projectile gun = Projectile.NewProjectileDirect(player.GetSource_ItemUse_WithPotentialAmmo(Item, Item.useAmmo), player.Center, Vector2.Zero, STGType, Item.damage, Item.knockBack, player.whoAmI);
                gun.originalDamage = damage;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }

        public override void AddRecipes()
        {
            this.RegisterINS2RecipeWeapon(290);
        }
    }
}