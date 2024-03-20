using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using UnityEngine;

namespace CharlieDotPNG
{
    public static class HooksGeneral
    {
        public static void Setup()
        {
            IDetour hook1 = new Hook(typeof(CharacterCombat).GetMethod(nameof(CharacterCombat.Damage), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(DamageCH), ~BindingFlags.Default));
            IDetour hook2 = new Hook(typeof(EnemyCombat).GetMethod(nameof(EnemyCombat.Damage), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(DamageEN), ~BindingFlags.Default));
            IDetour hook3 = new Hook(typeof(CharacterCombat).GetMethod(nameof(CharacterCombat.WillApplyDamage), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(WillApplyDamageCH), ~BindingFlags.Default));
            IDetour hook4 = new Hook(typeof(EnemyCombat).GetMethod(nameof(EnemyCombat.WillApplyDamage), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(WillApplyDamageEN), ~BindingFlags.Default));
            IDetour hook5 = new Hook(typeof(MainMenuController).GetMethod(nameof(MainMenuController.Start), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(StartMenu), ~BindingFlags.Default));
            IDetour hook6 = new Hook(typeof(CombatManager).GetMethod(nameof(CombatManager.InitializeCombat), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(InitializeCombat), ~BindingFlags.Default));
            IDetour hook7 = new Hook(typeof(CombatStats).GetMethod(nameof(CombatStats.PlayerTurnStart), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(PlayerTurnStart), ~BindingFlags.Default));
            IDetour hook8 = new Hook(typeof(CombatStats).GetMethod(nameof(CombatStats.PlayerTurnEnd), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(PlayerTurnEnd), ~BindingFlags.Default));
            IDetour hook9 = new Hook(typeof(CombatManager).GetMethod(nameof(CombatManager.PostNotification), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(PostNotification), ~BindingFlags.Default));
            IDetour hook10 = new Hook(typeof(EffectAction).GetMethod(nameof(EffectAction.Execute), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(EffectActionExecute), ~BindingFlags.Default));
            IDetour hook11 = new Hook(typeof(TooltipTextHandlerSO).GetMethod(nameof(TooltipTextHandlerSO.ProcessStoredValue), ~BindingFlags.Default), typeof(HooksGeneral).GetMethod(nameof(AddStoredValue), ~BindingFlags.Default));
        }

        public static DamageInfo DamageCH(Func<CharacterCombat, int, IUnit, DeathType, int, bool, bool, bool, DamageType, DamageInfo> orig, CharacterCombat self, int amount, IUnit killer, DeathType deathType, int targetSlotOffset = -1, bool addHealthMana = true, bool directDamage = true, bool ignoresShield = false, DamageType specialDamage = DamageType.None)
        {
            DamageInfo ret = orig(self, amount, killer, deathType, targetSlotOffset, addHealthMana, directDamage, ignoresShield, specialDamage);

            return ret;
        }
        public static DamageInfo DamageEN(Func<EnemyCombat, int, IUnit, DeathType, int, bool, bool, bool, DamageType, DamageInfo> orig, EnemyCombat self, int amount, IUnit killer, DeathType deathType, int targetSlotOffset = -1, bool addHealthMana = true, bool directDamage = true, bool ignoresShield = false, DamageType specialDamage = DamageType.None)
        {
            DamageInfo ret = orig(self, amount, killer, deathType, targetSlotOffset, addHealthMana, directDamage, ignoresShield, specialDamage);

            return ret;
        }
        public static int WillApplyDamageCH(Func<CharacterCombat, int, IUnit, int> orig, CharacterCombat self, int amount, IUnit targetUnit)
        {
            int ret = orig(self, amount, targetUnit);

            return ret;
        }
        public static int WillApplyDamageEN(Func<EnemyCombat, int, IUnit, int> orig, EnemyCombat self, int amount, IUnit targetUnit)
        {
            int ret = orig(self, amount, targetUnit);

            return ret;
        }
        public static void StartMenu(Action<MainMenuController> orig, MainMenuController self)
        {
            orig(self);
        }
        public static void InitializeCombat(Action<CombatManager> orig, CombatManager self)
        {
            orig(self);
        }
        public static void PlayerTurnStart(Action<CombatStats> orig, CombatStats self)
        {
            orig(self);
        }
        public static void PlayerTurnEnd(Action<CombatStats> orig, CombatStats self)
        {
            orig(self);
        }
        public static void PostNotification(Action<CombatManager, string, object, object> orig, CombatManager self, string call, object sender, object args)
        {
            orig(self, call, sender, args);
        }
        public static IEnumerator EffectActionExecute(Func<EffectAction, CombatStats, IEnumerator> orig, EffectAction self, CombatStats stats)
        {
            IEnumerator ret = orig(self, stats);

            return ret;
        }
        public static string AddStoredValue(Func<TooltipTextHandlerSO, UnitStoredValueNames, int, string> orig, TooltipTextHandlerSO self, UnitStoredValueNames storedValue, int value)
        {
            string result;
            if (storedValue == (UnitStoredValueNames)77889 && value > 0)
            {
                string str = "Multiattack" + string.Format(" +{0}", value);
                string str3 = "<color=#" + ColorUtility.ToHtmlStringRGB(self._positiveSTColor) + ">";
                string str4 = "</color>";
                result = str3 + str + str4;
            }
            else
            {
                result = orig(self, storedValue, value);
            }
            return result;
        }
    }
}
