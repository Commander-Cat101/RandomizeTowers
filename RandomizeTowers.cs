using MelonLoader;
using BTD_Mod_Helper;
using RandomizeTowers;
using Il2CppAssets.Scripts.Unity.Achievements.List;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Models.Towers;
using System;
using Il2CppAssets.Scripts.Unity;
using System.Linq;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppSystem.Linq;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Models.Bloons;

[assembly: MelonInfo(typeof(RandomizeTowers.RandomTowers), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace RandomizeTowers;

public class RandomTowers : BloonsTD6Mod
{
    public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
    {
        base.OnTowerUpgraded(tower, upgradeName, newBaseTowerModel);
        if (!tower.towerModel.isSubTower)
        {
            tower.RandomizeBase();
        }

    }
    public override void OnTowerCreated(Tower tower, Entity target, Model modelToUse)
    {
        base.OnTowerCreated(tower, target, modelToUse);
        if (!tower.towerModel.isSubTower)
        {
            tower.RandomizeBase();
        }
    }
    public override void OnTitleScreen()
    {
        foreach (var tower in Game.instance.model.towers.Where(tower => !tower.HasTiers()))
        {
            try
            {
                tower.SetMaxAmount(1);
            }
            catch (Exception e)
            {

            }
        }
    }
    public override void OnMatchStart()
    {
        base.OnMatchStart();
        
    }

}
public static class RandomizeExt
{
    public static string[] AllowedTowers = { TowerType.DartMonkey, TowerType.BoomerangMonkey, TowerType.TackShooter, TowerType.BombShooter, TowerType.IceMonkey, TowerType.GlueGunner,
        TowerType.SniperMonkey, TowerType.MonkeySub, TowerType.MonkeyBuccaneer, TowerType.MonkeyAce, TowerType.HeliPilot, TowerType.MortarMonkey, TowerType.DartlingGunner,
        TowerType.WizardMonkey, TowerType.SuperMonkey, TowerType.NinjaMonkey, TowerType.Alchemist, TowerType.Druid,
        TowerType.BananaFarm, TowerType.SpikeFactory, TowerType.MonkeyVillage, TowerType.EngineerMonkey };
    public static string[] AllowedAbilities = { TowerType.DartMonkey, TowerType.BoomerangMonkey, TowerType.TackShooter, TowerType.BombShooter, TowerType.IceMonkey, TowerType.GlueGunner,
        TowerType.SniperMonkey, TowerType.MonkeySub, TowerType.MonkeyBuccaneer, TowerType.MonkeyAce, TowerType.HeliPilot, TowerType.MortarMonkey, TowerType.DartlingGunner,
        TowerType.WizardMonkey, TowerType.SuperMonkey, TowerType.NinjaMonkey, TowerType.Alchemist, TowerType.Druid,
        TowerType.BananaFarm, TowerType.SpikeFactory, TowerType.MonkeyVillage, TowerType.EngineerMonkey };
    public static Tower RandomizeTower(this Tower tower, uint Attempts)
    {
        float worth = tower.worth;
        TowerModel randomtower = tower.towerModel;
        float CostDifference = 999999;
        for (int count = 0; count < Attempts; count++)
        {
            try
            {
                var attempttower = Game.instance.model.GetTowerFromId(GetRandomTower());

                float attemptdifference = attempttower.cost - randomtower.cost;

                if (attemptdifference < 0) { attemptdifference = -attemptdifference; }

                if (CostDifference > attemptdifference)
                {
                    randomtower = attempttower;
                    CostDifference = attemptdifference;
                }
            }
            catch
            {
            }
            
        }
        tower.UpdateRootModel(randomtower);
        return tower;
    }
    public static string GetRandomTower()
    {
        Random random = new();
        UpgradePath MainPath = (UpgradePath)random.Next(0, 3);
        UpgradePath SecondPath = (UpgradePath)random.Next(0, 3);

        while (MainPath == SecondPath)
        {
            SecondPath = (UpgradePath)random.Next(0, 3);
        }

        int MainPathUpgrades = random.Next(0, 6);
        int SecondPathUpgrades = random.Next(0, 3);

        int[] upgrades = { 0, 0, 0 };

        upgrades[((int)MainPath)] = MainPathUpgrades;
        upgrades[((int)SecondPath)] = SecondPathUpgrades;

        string Tower = AllowedTowers[random.Next(0, AllowedTowers.Length - 1)];
        string FinalTower = Tower + "-" + upgrades[0] + upgrades[1] + upgrades[2];
        return FinalTower;
    }
    public static Tower RandomizeBase(this Tower tower)
    {
        Random random = new();
        string id = AllowedTowers[random.Next(0, AllowedTowers.Length - 1)];
        string upgrades = "-";
        bool hasuppgrades = false;
        string returntower;
        foreach (var path in tower.towerModel.tiers)
        {
            if (path > 0)
            {
                hasuppgrades = true;
            }
            upgrades += path.ToString();
        }
        if (hasuppgrades)
        {
            returntower = id + upgrades;
        }
        else
        {
            returntower = id;
        }
        MelonLogger.Msg(returntower);
        tower.UpdateRootModel(Game.instance.model.GetTowerFromId(returntower));

        tower.RandomizeTowerAbilities();
        tower.RandomizeTowerProjectiles();
        return tower;
    }
    public static Tower RandomizeTowerAbilities(this Tower tower)
    {
        Random random = new();
        TowerModel model = tower.rootModel.Duplicate().Cast<TowerModel>();
        if (model.HasBehavior<AbilityModel>())
        {
            foreach (var ability in model.behaviors)
            {
                if (ability.IsType<AbilityModel>())
                {
                    model.behaviors.RemoveItem(ability);
                }
            }
            string Tower = AllowedAbilities[random.Next(0, AllowedAbilities.Length - 1)];
            int upgrade = random.Next(4, 6);
            var Ability = Game.instance.model.GetTowerFromId(Tower + "-0" + upgrade + "0").GetAbility(0).Duplicate();
            model.AddBehavior(Ability);
        }

        tower.UpdateRootModel(model);
        return tower;
    }
    public static Tower RandomizeTowerProjectiles(this Tower tower)
    {
        Random random = new();
        TowerModel model = tower.rootModel.Duplicate().Cast<TowerModel>();
        foreach (var weapon in model.GetDescendants<WeaponModel>().ToArray())
        {
            string randomtower = GetRandomTower();
            int projamount = Game.instance.model.GetTowerFromId(randomtower).GetDescendants<ProjectileModel>().ToArray().Count;
            weapon.projectile = Game.instance.model.GetTowerFromId(randomtower).GetDescendants<ProjectileModel>().ToArray()[random.Next(0, projamount)].Duplicate();
        }

        tower.UpdateRootModel(model);
        return tower;
    }
}
enum UpgradePath
{
    Top = 0,
    Middle = 1,
    Bottom = 2
}