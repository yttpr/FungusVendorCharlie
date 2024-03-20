using BrutalAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CharlieDotPNG
{
    public static class AbilityNameFix
    {
        public static CharacterAbility CharacterAbility(Func<Ability, CharacterAbility> orig, Ability self)
        {
            CharacterAbility ability = orig(self);
            ability.ability._abilityName = self.name;
            ability.ability._description = self.description;
            ability.ability.name = self.name;
            return ability;
        }
        public static EnemyAbilityInfo EnemyAbility(Func<Ability, EnemyAbilityInfo> orig, Ability self)
        {
            EnemyAbilityInfo ability = orig(self);
            ability.ability._abilityName = self.name;
            ability.ability._description = self.description;
            ability.ability.name = self.name;
            return ability;
        }
        public static void Setup()
        {
            IDetour chara = new Hook(typeof(Ability).GetMethod(nameof(Ability.CharacterAbility), ~BindingFlags.Default), typeof(AbilityNameFix).GetMethod(nameof(CharacterAbility), ~BindingFlags.Default));
            IDetour enemy = new Hook(typeof(Ability).GetMethod(nameof(Ability.EnemyAbility), ~BindingFlags.Default), typeof(AbilityNameFix).GetMethod(nameof(EnemyAbility), ~BindingFlags.Default));
        }
    }
}
