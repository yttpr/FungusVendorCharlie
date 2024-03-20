using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CharlieDotPNG
{
    public static class PigmentUsedCollector
    {
        public static List<ManaColorSO> lastUsed;
        public static int ID;
        public static void UseAbility(Action<CharacterCombat, int, FilledManaCost[]> orig, CharacterCombat self, int abilityID, FilledManaCost[] filledCost)
        {
            if (lastUsed == null) lastUsed = new List<ManaColorSO>();
            lastUsed.Clear();
            ID = self.ID;
            foreach (FilledManaCost mana in filledCost)
            {
                lastUsed.Add(mana.Mana);
            }
            orig(self, abilityID, filledCost);
        }
        public static void FinalizeAbilityActions(Action<CharacterCombat> orig, CharacterCombat self)
        {
            orig(self);
            ID = -1;
            lastUsed.Clear();
        }
        public static void Setup()
        {
            lastUsed = new List<ManaColorSO>();
            IDetour hook1 = new Hook(typeof(CharacterCombat).GetMethod(nameof(CharacterCombat.UseAbility), ~BindingFlags.Default), typeof(PigmentUsedCollector).GetMethod(nameof(UseAbility), ~BindingFlags.Default));
            IDetour hook2 = new Hook(typeof(CharacterCombat).GetMethod(nameof(CharacterCombat.FinalizeAbilityActions), ~BindingFlags.Default), typeof(PigmentUsedCollector).GetMethod(nameof(FinalizeAbilityActions), ~BindingFlags.Default));
        }
    }
}
