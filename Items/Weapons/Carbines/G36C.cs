﻿using InsurgencyWeapons.Items.Ammo;
using InsurgencyWeapons.Projectiles.Carbines;

namespace InsurgencyWeapons.Items.Weapons.Carbines
{
    public class G36C : Carbine
    {
        public override void SetStaticDefaults()
        {
            AmmoItem.AddRelationShip(ModContent.ItemType<Bullet556>(), Type);
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Item.knockBack = 4f;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = Item.useTime = 5;
            Item.width = 62;
            Item.height = 28;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.damage = 8;
            Item.shootSpeed = 11f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Ranged;
            WeaponHeldProjectile = ModContent.ProjectileType<G36CHeld>();
            MoneyCost = 250;
            base.SetDefaults();
        }
    }
}