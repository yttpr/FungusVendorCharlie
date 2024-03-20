using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BrutalAPI;
using System.Reflection;
using System.Collections;

namespace CharlieDotPNG
{
    public static class EZExtensions
    {
        public static T GetRandom<T>(this T[] array)
        {
            if (array.Length <= 0) return default;
            return array[UnityEngine.Random.Range(0, array.Length)];
        }
        public static T GetRandom<T>(this List<T> list)
        {
            if (list.Count <= 0) return default;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T[] SelfArray<T>(this T self)
        {
            return new T[] { self };
        }

        public static int GetStatus(this IUnit self, StatusEffectType type)
        {
            if (self is IStatusEffector effector)
            {
                foreach (IStatusEffect status in effector.StatusEffects)
                {
                    if (status.EffectType == type) return status.StatusContent;
                }
            }
            return 0;
        }
        public static void AddToDollPool(WearableStaticModifierSetterSO abil)
        {
            CasterAddRandomExtraAbilityEffect effect = (LoadedAssetsHandler.GetCharcater("Doll_CH").passiveAbilities[0] as Connection_PerformEffectPassiveAbility).connectionEffects[1].effect as CasterAddRandomExtraAbilityEffect;
            if (abil is BasicAbilityChange_Wearable_SMS slap)
            {
                effect._slapData = new List<BasicAbilityChange_Wearable_SMS>(effect._slapData)
                {
                    slap
                }.ToArray();
            }
            else if (abil is ExtraAbility_Wearable_SMS extra)
            {
                effect._extraData = new List<ExtraAbility_Wearable_SMS>(effect._extraData)
                {
                    extra
                }.ToArray();
            }
        }

        public static Type[] GetAllDerived(Type baze)
        {
            List<Type> ret = new List<Type>();
            Assembly[] all = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < all.Length; i++)
            {
                Assembly a = all[i];
                foreach (Type t in a.GetTypes())
                {
                    if (baze.IsAssignableFrom(t) && !ret.Contains(t) && t != baze) ret.Add(t);
                }
            }
            return ret.ToArray();
        }

        public static bool PCall(Action orig, string name = null)
        {
            try { orig(); }
            catch { Debug.LogError(name != null ? name + " failed" : orig.ToString() + " failed"); return false; }
            return true;
        }
    }
    public static class EZEffects
    {
        public static PreviousEffectCondition DidThat<T>(bool success, int prev = 1) where T : PreviousEffectCondition
        {
            PreviousEffectCondition ret = ScriptableObject.CreateInstance<T>();
            ret.wasSuccessful = success;
            ret.previousAmount = prev;
            return ret;
        }
        public static AnimationVisualsEffect GetVisuals<T>(string visuals, bool isChara, BaseCombatTargettingSO targets) where T : AnimationVisualsEffect
        {
            AnimationVisualsEffect ret = ScriptableObject.CreateInstance<T>();
            if (isChara) ret._visuals = LoadedAssetsHandler.GetCharacterAbility(visuals).visuals;
            else ret._visuals = LoadedAssetsHandler.GetEnemyAbility(visuals).visuals;
            ret._animationTarget = targets;
            return ret;
        }
        public static Targetting_ByUnit_Side TargetSide<T>(bool allies, bool allSlots, bool ignoreCast = false) where T : Targetting_ByUnit_Side
        {
            Targetting_ByUnit_Side ret = ScriptableObject.CreateInstance<T>();
            ret.ignoreCastSlot = ignoreCast;
            ret.getAllies = allies;
            ret.getAllUnitSlots = allSlots;
            return ret;
        }
        public static SwapToOneSideEffect GoSide<T>(bool right) where T : SwapToOneSideEffect
        {
            SwapToOneSideEffect ret = ScriptableObject.CreateInstance<T>();
            ret._swapRight = right;
            return ret;
        }
    }
    public class GenericItem<T> : Item where T : BaseWearableSO
    {
        public T Item;
        public override BaseWearableSO Wearable()
        {
            T ret = ScriptableObject.CreateInstance<T>();
            ret.BaseWearable(this);
            Item = ret;
            return ret;
        }
    }
    public class MultiEffectorCondition : EffectConditionSO
    {
        public EffectConditionSO[] conditions;
        public bool And = true;
        public override bool MeetCondition(IUnit caster, EffectInfo[] effects, int currentIndex)
        {
            foreach (EffectConditionSO cond in conditions)
            {
                bool hit = cond.MeetCondition(caster, effects, currentIndex);
                if (And && !hit) return false;
                else if (!And && hit) return true;
            }
            if (And) return true;
            return false;
        }
        public static MultiEffectorCondition Create(EffectConditionSO[] cond, bool and = true)
        {
            MultiEffectorCondition ret = ScriptableObject.CreateInstance<MultiEffectorCondition>();
            ret.conditions = cond;
            ret.And = and;
            return ret;
        }
        public static MultiEffectorCondition Create(EffectConditionSO first, EffectConditionSO second, bool and = true)
        {
            MultiEffectorCondition ret = ScriptableObject.CreateInstance<MultiEffectorCondition>();
            ret.conditions = new EffectConditionSO[] { first, second };
            ret.And = and;
            return ret;
        }
    }
    public class ObjectHolder
    {
        public object args;
        public ObjectHolder(object args)
        {
            this.args = args;
        }
    }
    public class MultiplyFloatModifier : IntValueModifier
    {
        public float num;
        public bool roundUp;
        public bool doNegative;
        public MultiplyFloatModifier(float num, bool roundUp, bool dealing, bool doNegative = false) : base(dealing ? 20 : 70)
        {
            this.num = num;
            this.roundUp = roundUp;
            this.doNegative = doNegative;
        }
        public override int Modify(int value)
        {
            float gap = value;
            gap *= num;
            int rounded = roundUp ? (int)Math.Ceiling(gap) : (int)Math.Floor(gap);
            return doNegative ? rounded : Math.Max(0, rounded);
        }
    }
    public class AnimationVisualsByGivenEffect : AnimationVisualsEffect
    {
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            CombatManager.Instance.AddUIAction(new PlayAbilityAnimationGivenAction(_visuals, targets, caster, areTargetSlots));
            exitAmount = 0;
            return true;
        }
    }
    public class PlayAbilityAnimationGivenAction : CombatAction
    {
        public TargetSlotInfo[] _targetting;

        public AttackVisualsSO _visuals;

        public IUnit _caster;
        public bool aretargetSlots;

        public PlayAbilityAnimationGivenAction(AttackVisualsSO visuals, TargetSlotInfo[] targetting, IUnit caster, bool aretargetSlots)
        {
            _visuals = visuals;
            _targetting = targetting;
            _caster = caster;
            this.aretargetSlots = aretargetSlots;
        }

        public override IEnumerator Execute(CombatStats stats)
        {
            TargetSlotInfo[] targets = null;
            bool areTargetSlots = true;
            if (_targetting != null)
            {
                targets = _targetting;
                areTargetSlots = aretargetSlots;
            }

            yield return stats.combatUI.PlayAbilityAnimation(_visuals, targets, areTargetSlots);
        }
    }
}
