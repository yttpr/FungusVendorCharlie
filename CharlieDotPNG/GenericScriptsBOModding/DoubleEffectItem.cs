using UnityEngine;

namespace BrutalAPI
{
    public class DoubleEffectItem : Item
    {
        public Effect[] firstEffects = new Effect[0];
        public Effect[] secondEffects = new Effect[0];
        public bool _firsteEffectImmediate = false;
        public bool _secondImmediateEffect = false;
        public TriggerCalls[] SecondTrigger = new TriggerCalls[0];
        public TriggerCalls[] FirstTrigger = new TriggerCalls[0];
        public bool firstPopUp = true;
        public bool secondPopUp = true;
        public EffectorConditionSO[] secondTriggerConditions = new EffectorConditionSO[0];

        public override BaseWearableSO Wearable()
        {
            CustomDoublePerformEffectWearable instance = ScriptableObject.CreateInstance<CustomDoublePerformEffectWearable>();
            instance.BaseWearable((Item)this);
            instance._firstEffects = ExtensionMethods.ToEffectInfoArray(this.firstEffects);
            instance._firstImmediateEffect = this._firsteEffectImmediate;
            //instance.triggerOn = this.FirstTrigger;
            instance.doesItemPopUp = this.firstPopUp;
            instance._secondEffects = ExtensionMethods.ToEffectInfoArray(this.secondEffects);
            instance._secondImmediateEffect = this._firsteEffectImmediate;
            instance._secondPerformTriggersOn = this.SecondTrigger;
            instance._secondDoesPerformItemPopUp = this.secondPopUp;
            instance._secondPerformConditions = this.secondTriggerConditions;
            return (BaseWearableSO)instance;
        }
    }
}