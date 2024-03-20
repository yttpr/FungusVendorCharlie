using BrutalAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CharlieDotPNG
{
    public static class FieldEffectFixHook
    {
        public static bool ApplySlotStatusEffect(Func<CombatSlot, ISlotStatusEffect, int, bool> orig, CombatSlot self, ISlotStatusEffect statusEffect, int amount)
        {
            bool hasIt = false;
            int index = -1;
            for (int i = 0; i < self.StatusEffects.Count; i++)
            {
                if (self.StatusEffects[i].EffectType == statusEffect.EffectType && self.StatusEffects[i].GetType() != statusEffect.GetType())
                {
                    hasIt = true;
                    index = i;
                    break;
                }
            }
            if (hasIt)
            {
                ConstructorInfo[] constructors = self.StatusEffects[index].GetType().GetConstructors();
                foreach (ConstructorInfo constructor in constructors)
                {
                    ParameterInfo[] paras = constructor.GetParameters();
                    if (paras.Length == 4 && paras[0].ParameterType == typeof(int) && paras[1].ParameterType == typeof(int) && paras[2].ParameterType == typeof(bool) && paras[3].ParameterType == typeof(int))
                    {
                        statusEffect = (ISlotStatusEffect)Activator.CreateInstance(self.StatusEffects[index].GetType(), self.SlotID, amount, self.IsCharacter, statusEffect.Restrictor);
                    }
                    else if (paras.Length == 3 && paras[0].ParameterType == typeof(int) && paras[1].ParameterType == typeof(int) && paras[2].ParameterType == typeof(int))
                    {
                        statusEffect = (ISlotStatusEffect)Activator.CreateInstance(self.StatusEffects[index].GetType(), self.SlotID, amount, statusEffect.Restrictor);
                    }
                }
            }
            try
            {
                return orig(self, statusEffect, amount);
            }
            catch
            {
                Debug.LogError("super epic field effect compatibility failure!");
                return false;
            }
        }
        public static void Setup()
        {
            IDetour hook = new Hook(typeof(CombatSlot).GetMethod(nameof(CombatSlot.ApplySlotStatusEffect), ~BindingFlags.Default), typeof(FieldEffectFixHook).GetMethod(nameof(ApplySlotStatusEffect), ~BindingFlags.Default));
        }
    }
}
