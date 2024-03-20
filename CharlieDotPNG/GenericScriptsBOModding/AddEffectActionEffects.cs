using BrutalAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CharlieDotPNG
{
    public class SubActionEffect : EffectSO
    {
        public Effect[] effects;

        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            EffectInfo[] info = ExtensionMethods.ToEffectInfoArray(effects);
            exitAmount = 0;
            foreach (TargetSlotInfo target in targets)
            {
                if (target.HasUnit)
                {
                    CombatManager.Instance.AddSubAction(new EffectAction(info, target.Unit));
                    exitAmount++;
                }
            }
            return exitAmount > 0;
        }

        public static SubActionEffect Create(Effect[] e)
        {
            SubActionEffect ret = ScriptableObject.CreateInstance<SubActionEffect>();
            ret.effects = e;
            return ret;
        }
    }
    public class CasterSubActionEffect : EffectSO
    {
        public Effect[] effects;

        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            EffectInfo[] info = ExtensionMethods.ToEffectInfoArray(effects);
            exitAmount = 0;
            CombatManager.Instance.AddSubAction(new EffectAction(info, caster));
            return true;
        }

        public static CasterSubActionEffect Create(Effect[] e)
        {
            CasterSubActionEffect ret = ScriptableObject.CreateInstance<CasterSubActionEffect>();
            ret.effects = e;
            return ret;
        }
    }
    public class RootActionEffect : EffectSO
    {
        public Effect[] effects;

        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            EffectInfo[] info = ExtensionMethods.ToEffectInfoArray(effects);
            exitAmount = 0;
            foreach (TargetSlotInfo target in targets)
            {
                if (target.HasUnit)
                {
                    CombatManager.Instance.AddRootAction(new EffectAction(info, target.Unit));
                    exitAmount++;
                }
            }
            return exitAmount > 0;
        }

        public static RootActionEffect Create(Effect[] e)
        {
            RootActionEffect ret = ScriptableObject.CreateInstance<RootActionEffect>();
            ret.effects = e;
            return ret;
        }
    }
    public class CasterRootActionEffect : EffectSO
    {
        public Effect[] effects;

        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            EffectInfo[] info = ExtensionMethods.ToEffectInfoArray(effects);
            exitAmount = 0;
            CombatManager.Instance.AddRootAction(new EffectAction(info, caster));
            return true;
        }

        public static CasterRootActionEffect Create(Effect[] e)
        {
            CasterRootActionEffect ret = ScriptableObject.CreateInstance<CasterRootActionEffect>();
            ret.effects = e;
            return ret;
        }
    }
    public class RootActionAction : CombatAction
    {
        public RootActionAction(CombatAction a)
        {
            ex = a;
        }
        public CombatAction ex;
        public override IEnumerator Execute(CombatStats stats)
        {
            CombatManager.Instance.AddRootAction(ex);
            yield return null;
        }
    }
    public class SubActionAction : CombatAction
    {
        public SubActionAction(CombatAction a)
        {
            ex = a;
        }
        public CombatAction ex;
        public override IEnumerator Execute(CombatStats stats)
        {
            CombatManager.Instance.AddSubAction(ex);
            yield return null;
        }
    }
}
