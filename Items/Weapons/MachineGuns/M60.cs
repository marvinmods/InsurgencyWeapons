﻿using InsurgencyWeapons.Items.Ammo;
using InsurgencyWeapons.Projectiles.MachineGuns;

namespace InsurgencyWeapons.Items.Weapons.MachineGuns
{
    public class M60 : LightMachineGun
    {
        public override void SetStaticDefaults()
        {
            AmmoItem.AddRelationShip(ModContent.ItemType<Bullet76251>(), Type);
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Item.knockBack = 4f;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = Item.useTime = 6;
            Item.width = 80;
            Item.height = 22;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.damage = 13;
            Item.shootSpeed = 11f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Ranged;
            WeaponHeldProjectile = ModContent.ProjectileType<M60Held>();
            MoneyCost = 540;
            base.SetDefaults();
        }
    }
}