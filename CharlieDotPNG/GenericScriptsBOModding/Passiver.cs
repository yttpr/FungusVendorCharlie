using BrutalAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CharlieDotPNG
{
    public static class Passiver
    {
        public static BasePassiveAbilitySO Fleeting(int amount)
        {
            FleetingPassiveAbility baseParent = Passives.Fleeting as FleetingPassiveAbility;
            FleetingPassiveAbility flee = ScriptableObject.Instantiate<FleetingPassiveAbility>(baseParent);
            flee._passiveName = "Fleeting (" + amount.ToString() + ")";
            flee._characterDescription = "After " + amount.ToString() + " rounds this party member will flee... Coward.";
            flee._enemyDescription = "After " + amount.ToString() + " rounds this enemy will flee.";
            flee._turnsBeforeFleeting = amount;
            return flee;
        }
        public static BasePassiveAbilitySO Overexert(int amount)
        {
            IntegerReferenceOverEqualValueEffectorCondition instance = ScriptableObject.CreateInstance<IntegerReferenceOverEqualValueEffectorCondition>();
            instance.compareValue = amount;
            BasePassiveAbilitySO baseParent = LoadedAssetsHandler.GetEnemy("Scrungie_EN").passiveAbilities[2];
            BasePassiveAbilitySO passive = ScriptableObject.Instantiate<BasePassiveAbilitySO>(baseParent);
            passive._passiveName = "Overexert (" + amount.ToString() + ")";
            passive._characterDescription = "Won't work with this version.";
            passive._enemyDescription = "Upon receiving " + amount.ToString() + " or more direct damage, cancel 1 of this enemy's actions.";
            passive.conditions = new EffectorConditionSO[] { instance };
            return passive;
        }
        public static BasePassiveAbilitySO Leaky(int amount)
        {
            PerformEffectPassiveAbility leaky = ScriptableObject.CreateInstance<PerformEffectPassiveAbility>();
            leaky._passiveName = "Leaky (" + amount.ToString() + ")";
            leaky.passiveIcon = Passives.Leaky.passiveIcon;
            leaky._enemyDescription = "Upon receiving direct damage, this enemy generates " + amount.ToString() + " extra pigment of its health color.";
            leaky._characterDescription = "Upon receiving direct damage, this character generates " + amount.ToString() + " extra pigment of its health color.";
            leaky.type = PassiveAbilityTypes.Leaky;
            leaky.doesPassiveTriggerInformationPanel = true;
            leaky._triggerOn = new TriggerCalls[] { TriggerCalls.OnDirectDamaged };
            leaky.effects = ExtensionMethods.ToEffectInfoArray(new Effect[]
            {
                    new Effect(ScriptableObject.CreateInstance<GenerateCasterHealthManaEffect>(), amount, null, Slots.Self)
            });
            return leaky;
        }
        public static BasePassiveAbilitySO Multiattack(int amount, bool fool = false)
        {
            if (!fool)
            {
                IntegerSetterPassiveAbility baseParent = Passives.Multiattack as IntegerSetterPassiveAbility;
                IntegerSetterPassiveAbility ret = ScriptableObject.Instantiate<IntegerSetterPassiveAbility>(baseParent);
                ret._passiveName = "Multi Attack (" + amount.ToString() + ")";
                ret._characterDescription = "won't work boowomp";
                ret._enemyDescription = "This enemy will perform " + amount.ToString() + " actions each turn.";
                ret.integerValue = amount - 1;
                return ret;
            }
            else
            {
                PerformDoubleEffectPassiveAbility instance1 = ScriptableObject.CreateInstance<PerformDoubleEffectPassiveAbility>();
                instance1._passiveName = "MultiAttack (" + amount.ToString() + ")";
                instance1.passiveIcon = Passives.Multiattack.passiveIcon;
                instance1.type = PassiveAbilityTypes.MultiAttack;
                instance1._enemyDescription = "This shouldn't be on an enemy.";
                instance1._characterDescription = "This party member can perform " + amount.ToString() + " abilities per turn.";
                instance1.specialStoredValue = (UnitStoredValueNames)77889;
                CasterSetStoredValueEffect instance2 = ScriptableObject.CreateInstance<CasterSetStoredValueEffect>();
                instance2._valueName = (UnitStoredValueNames)77889;
                instance1._triggerOn = new TriggerCalls[1]
                {
        TriggerCalls.OnTurnStart
                };
                instance1.effects = ExtensionMethods.ToEffectInfoArray(new Effect[1]
                {
        new Effect((EffectSO) instance2, amount - 1, new IntentType?(), Slots.Self)
                });
                RefreshIfStoredValueNotZero instance3 = ScriptableObject.CreateInstance<RefreshIfStoredValueNotZero>();
                instance3._valueName = (UnitStoredValueNames)77889;
                ScriptableObject.CreateInstance<CasterLowerStoredValueEffect>()._valueName = (UnitStoredValueNames)77889;
                instance1._secondTriggerOn = new TriggerCalls[1]
                {
        TriggerCalls.OnAbilityUsed
                };
                instance1._secondEffects = ExtensionMethods.ToEffectInfoArray(new Effect[1]
                {
        new Effect((EffectSO) instance3, 1, new IntentType?(), Slots.Self)
                });
                instance1.doesPassiveTriggerInformationPanel = false;
                instance1._secondDoesPerformPopUp = false;
                return instance1;
            }
        }
        public static BasePassiveAbilitySO Inferno(int amount)
        {
            PerformEffectPassiveAbility burn = ScriptableObject.CreateInstance<PerformEffectPassiveAbility>();
            burn._passiveName = "Inferno (" + amount.ToString() + ")";
            burn.passiveIcon = Passives.Inferno.passiveIcon;
            burn._enemyDescription = "On turn start, this enemy inflicts " + amount.ToString() + " Fire on their position.";
            burn._characterDescription = "On turn start, this character inflicts " + amount.ToString() + " Fire on their position.";
            burn.type = PassiveAbilityTypes.Inferno;
            burn.doesPassiveTriggerInformationPanel = true;
            burn._triggerOn = new TriggerCalls[] { TriggerCalls.OnTurnStart };
            burn.effects = ExtensionMethods.ToEffectInfoArray(new Effect[]
            {
                    new Effect(ScriptableObject.CreateInstance<ApplyFireSlotEffect>(), amount, null, Slots.Self)
            });
            return burn;
        }
        public static BasePassiveAbilitySO Abomination => LoadedAssetsHandler.GetEnemy("OneManBand_EN").passiveAbilities[1];
        
        static BasePassiveAbilitySO _noStallWithering;
        public static BasePassiveAbilitySO NoStallWithering
        {
            get
            {
                if (_noStallWithering == null)
                {
                    _noStallWithering = ScriptableObject.CreateInstance<NoStallWItheringPassiveAbility>();
                    _noStallWithering._passiveName = Passives.Withering._passiveName;
                    _noStallWithering.passiveIcon = Passives.Withering.passiveIcon;
                    _noStallWithering._characterDescription = "If all remaining party members also have Withering, this party member will die.\nThis check is run repeatedly throughout combat.";
                    _noStallWithering._enemyDescription = "If all remaining enemies also have Withering, this enemy will die.\nThis check is run repeatedly throughout combat.";
                    _noStallWithering._triggerOn = new List<TriggerCalls>(Passives.Withering._triggerOn) { TriggerCalls.OnTurnStart, TriggerCalls.OnTurnFinished, TriggerCalls.OnPlayerTurnEnd_ForEnemy, TriggerCalls.OnMiscPlayerTurnStart, TriggerCalls.OnRoundFinished }.ToArray();
                    _noStallWithering.type = Passives.Withering.type;
                    _noStallWithering.doesPassiveTriggerInformationPanel = false;
                    _noStallWithering.conditions = new EffectorConditionSO[0];
                }
                return _noStallWithering;
            }
        }
    }

    public class NoStallWItheringPassiveAbility : BasePassiveAbilitySO
    {
        public override bool DoesPassiveTrigger => true;
        public override bool IsPassiveImmediate => true;
        public override void TriggerPassive(object sender, object args)
        {
            if (sender is IUnit unit)
            {
                if (unit.IsUnitCharacter) CombatManager.Instance.AddRootAction(new CharacterWitheringAction());
                else CombatManager.Instance.AddRootAction(new EnemyWitheringAction());
            }
        }
        public override void OnPassiveConnected(IUnit unit)
        {
            if (unit.IsUnitCharacter) CombatManager.Instance.AddRootAction(new CharacterWitheringAction());
            else CombatManager.Instance.AddRootAction(new EnemyWitheringAction());
        }
        public override void OnPassiveDisconnected(IUnit unit)
        {
        }
    }
    public class CasterSetStoredValueEffect : EffectSO
    {
        [SerializeField]
        public UnitStoredValueNames _valueName = UnitStoredValueNames.PunchA;

        public override bool PerformEffect(
          CombatStats stats,
          IUnit caster,
          TargetSlotInfo[] targets,
          bool areTargetSlots,
          int entryVariable,
          out int exitAmount)
        {
            exitAmount = 0;
            caster.SetStoredValue(this._valueName, entryVariable);
            return exitAmount > 0;
        }
    }
    public class RefreshIfStoredValueNotZero : EffectSO
    {
        [SerializeField]
        public bool _doesExhaustInstead;
        [SerializeField]
        public UnitStoredValueNames _valueName = UnitStoredValueNames.PunchA;

        public override bool PerformEffect(
          CombatStats stats,
          IUnit caster,
          TargetSlotInfo[] targets,
          bool areTargetSlots,
          int entryVariable,
          out int exitAmount)
        {
            exitAmount = 0;
            if (caster.GetStoredValue(this._valueName) != 0)
            {
                for (int index = 0; index < targets.Length; ++index)
                {
                    if (targets[index].HasUnit && (this._doesExhaustInstead ? targets[index].Unit.ExhaustAbilityUse() : targets[index].Unit.RefreshAbilityUse()))
                    {
                        ++exitAmount;
                        int newValue = caster.GetStoredValue(this._valueName) - entryVariable;
                        if (newValue < 0)
                            newValue = 0;
                        caster.SetStoredValue(this._valueName, newValue);
                    }
                }
            }
            return exitAmount > 0;
        }
    }
    public class CasterLowerStoredValueEffect : EffectSO
    {
        [SerializeField]
        public UnitStoredValueNames _valueName = UnitStoredValueNames.PunchA;

        public override bool PerformEffect(
          CombatStats stats,
          IUnit caster,
          TargetSlotInfo[] targets,
          bool areTargetSlots,
          int entryVariable,
          out int exitAmount)
        {
            exitAmount = 0;
            int newValue = caster.GetStoredValue(this._valueName) - entryVariable;
            if (newValue < 0)
                newValue = 0;
            caster.SetStoredValue(this._valueName, newValue);
            return exitAmount > 0;
        }
    }
}
