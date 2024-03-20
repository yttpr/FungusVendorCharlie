using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CharlieDotPNG
{
    public class TargettingClosestUnits : BaseCombatTargettingSO
    {
        public bool getAllies;

        public bool ignoreCastSlot = true;

        public override bool AreTargetAllies => getAllies;

        public override bool AreTargetSlots => true;

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            CombatSlot greaterest = null;
            CombatSlot lesserest = null;
            if ((isCasterCharacter && getAllies) || (!isCasterCharacter && !getAllies))
            {
                foreach (CombatSlot slot in slots.CharacterSlots)
                {
                    if ((slot.HasUnit && slot.SlotID > casterSlotID) && (!ignoreCastSlot || casterSlotID != slot.SlotID))
                    {
                        if (greaterest == null)
                        {
                            greaterest = slot;
                        }
                        else if (slot.SlotID < greaterest.SlotID)
                        {
                            greaterest = slot;
                        }
                    }
                    else if (slot.HasUnit && (slot.SlotID < casterSlotID) && (!ignoreCastSlot || casterSlotID != slot.SlotID))
                    {
                        if (lesserest == null)
                        {
                            lesserest = slot;
                        }
                        else if (slot.SlotID > lesserest.SlotID)
                        {
                            lesserest = slot;
                        }
                    }
                }
            }
            else
            {
                foreach (CombatSlot slot in slots.EnemySlots)
                {
                    if ((slot.HasUnit && slot.SlotID > casterSlotID) && (!ignoreCastSlot || casterSlotID != slot.SlotID))
                    {
                        if (greaterest == null)
                        {
                            greaterest = slot;
                        }
                        else if (slot.SlotID < greaterest.SlotID)
                        {
                            greaterest = slot;
                        }
                    }
                    else if (slot.HasUnit && (slot.SlotID < casterSlotID) && (!ignoreCastSlot || casterSlotID != slot.SlotID))
                    {
                        if (lesserest == null)
                        {
                            lesserest = slot;
                        }
                        else if (slot.SlotID > lesserest.SlotID)
                        {
                            lesserest = slot;
                        }
                    }
                }
            }
            if (greaterest != null)
            {
                targets.Add(greaterest.TargetSlotInformation);
            }
            if (lesserest != null)
            {
                targets.Add(lesserest.TargetSlotInformation);
            }
            return targets.ToArray();
        }
    }
    public class TargettingFarthestUnits : BaseCombatTargettingSO
    {
        public bool getAllies;

        public bool ignoreCastSlot = true;

        public override bool AreTargetAllies => getAllies;

        public override bool AreTargetSlots => true;

        public bool LeftOnly = false;
        public bool RightOnly = false;
        public bool FarthestOnly = false;

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            CombatSlot greaterest = null;
            CombatSlot lesserest = null;
            if ((isCasterCharacter && getAllies) || (!isCasterCharacter && !getAllies))
            {
                foreach (CombatSlot slot in slots.CharacterSlots)
                {
                    if ((slot.HasUnit && slot.SlotID > casterSlotID) && (!ignoreCastSlot || casterSlotID != slot.SlotID))
                    {
                        if (greaterest == null)
                        {
                            greaterest = slot;
                        }
                        else if (slot.SlotID > greaterest.SlotID)
                        {
                            greaterest = slot;
                        }
                    }
                    else if (slot.HasUnit && (slot.SlotID < casterSlotID) && (!ignoreCastSlot || casterSlotID != slot.SlotID))
                    {
                        if (lesserest == null)
                        {
                            lesserest = slot;
                        }
                        else if (slot.SlotID < lesserest.SlotID)
                        {
                            lesserest = slot;
                        }
                    }
                }
            }
            else
            {
                foreach (CombatSlot slot in slots.EnemySlots)
                {
                    if ((slot.HasUnit && slot.SlotID > casterSlotID) && (!ignoreCastSlot || casterSlotID != slot.SlotID))
                    {
                        if (greaterest == null)
                        {
                            greaterest = slot;
                        }
                        else if (slot.SlotID > greaterest.SlotID)
                        {
                            greaterest = slot;
                        }
                    }
                    else if (slot.HasUnit && (slot.SlotID < casterSlotID) && (!ignoreCastSlot || casterSlotID != slot.SlotID))
                    {
                        if (lesserest == null)
                        {
                            lesserest = slot;
                        }
                        else if (slot.SlotID < lesserest.SlotID)
                        {
                            lesserest = slot;
                        }
                    }
                }
            }
            if (greaterest != null && !LeftOnly)
            {
                targets.Add(greaterest.TargetSlotInformation);
            }
            if (lesserest != null && !RightOnly)
            {
                targets.Add(lesserest.TargetSlotInformation);
            }
            if (FarthestOnly && greaterest != null && lesserest != null)
            {
                int right = greaterest.SlotID - casterSlotID;
                int left = casterSlotID - lesserest.SlotID;
                if (right != left) targets.Clear();
                if (right > left) targets.Add(greaterest.TargetSlotInformation);
                else if (left > right) targets.Add(lesserest.TargetSlotInformation);
            }
            return targets.ToArray();
        }
    }
    public class TargettingRandomUnit : BaseCombatTargettingSO
    {
        public bool getAllies;

        public bool ignoreCastSlot = false;

        public override bool AreTargetAllies => getAllies;

        public override bool AreTargetSlots => false;

        public static bool IsUnitAlreadyContained(List<TargetSlotInfo> targets, TargetSlotInfo target)
        {
            foreach (TargetSlotInfo targe in targets)
            {
                if (targe.Unit == target.Unit)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCastSlot(int caster, TargetSlotInfo target)
        {
            if (!ignoreCastSlot) { return false; }
            else if (caster != target.SlotID) { return false; }
            else return true;
        }

        public static TargetSlotInfo LastRandom = null;

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            if ((getAllies && isCasterCharacter) || (!getAllies && !isCasterCharacter))
            {
                foreach (CombatSlot slot in slots.CharacterSlots)
                {
                    TargetSlotInfo targ = slot.TargetSlotInformation;
                    if (targ != null && targ.HasUnit && !IsUnitAlreadyContained(targets, targ) && !IsCastSlot(casterSlotID, targ))
                    {
                        targets.Add(targ);
                    }
                }
            }
            else
            {
                foreach (CombatSlot slot in slots.EnemySlots)
                {
                    TargetSlotInfo targ = slot.TargetSlotInformation;
                    if (targ != null && targ.HasUnit && !IsUnitAlreadyContained(targets, targ) && !IsCastSlot(casterSlotID, targ))
                    {
                        targets.Add(targ);
                    }
                }
            }
            if (targets.Count <= 0)
            {
                LastRandom = null;
                return new TargetSlotInfo[0];
            }
            TargetSlotInfo goy = targets[UnityEngine.Random.Range(0, targets.Count)];
            LastRandom = goy;
            return new TargetSlotInfo[] { goy };
        }
    }
    public class TargettingUnitsWithStatusEffectSide : Targetting_ByUnit_Side
    {
        public StatusEffectType targetStatus;

        public static bool IsUnitAlreadyContained(List<TargetSlotInfo> targets, TargetSlotInfo target)
        {
            foreach (TargetSlotInfo targe in targets)
            {
                if (targe.Unit == target.Unit)
                {
                    return true;
                }
            }
            return false;
        }

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            foreach (TargetSlotInfo targ in base.GetTargets(slots, casterSlotID, isCasterCharacter))
            {
                if (targ != null && targ.HasUnit && !IsUnitAlreadyContained(targets, targ) && targ.Unit.ContainsStatusEffect(targetStatus))
                {
                    targets.Add(targ);
                }
            }
            return targets.ToArray();
        }
    }
    public class TargettingUnitsWithStatusEffectAll : BaseCombatTargettingSO
    {
        public bool ignoreCastSlot = false;

        public StatusEffectType targetStatus;

        public override bool AreTargetAllies => false;

        public override bool AreTargetSlots => false;

        public static bool IsUnitAlreadyContained(List<TargetSlotInfo> targets, TargetSlotInfo target)
        {
            foreach (TargetSlotInfo targe in targets)
            {
                if (targe.Unit == target.Unit)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCastSlot(int caster, TargetSlotInfo target)
        {
            if (!ignoreCastSlot) { return false; }
            else if (caster != target.SlotID) { return false; }
            else return true;
        }

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            foreach (CombatSlot slot in slots.CharacterSlots)
            {
                TargetSlotInfo targ = slot.TargetSlotInformation;
                if (targ != null && targ.HasUnit && !IsUnitAlreadyContained(targets, targ) && !IsCastSlot(casterSlotID, targ) && targ.Unit.ContainsStatusEffect(targetStatus))
                {
                    targets.Add(targ);
                }
            }
            foreach (CombatSlot slot in slots.EnemySlots)
            {
                TargetSlotInfo targ = slot.TargetSlotInformation;
                if (targ != null && targ.HasUnit && !IsUnitAlreadyContained(targets, targ) && !IsCastSlot(casterSlotID, targ) && targ.Unit.ContainsStatusEffect(targetStatus))
                {
                    targets.Add(targ);
                }
            }
            return targets.ToArray();
        }
    }
    public class TargettingAllUnits : BaseCombatTargettingSO
    {
        public bool ignoreCastSlot = false;

        public override bool AreTargetAllies => false;

        public override bool AreTargetSlots => false;

        public static bool IsUnitAlreadyContained(List<TargetSlotInfo> targets, TargetSlotInfo target)
        {
            foreach (TargetSlotInfo targe in targets)
            {
                if (targe.Unit == target.Unit)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCastSlot(int caster, TargetSlotInfo target)
        {
            if (!ignoreCastSlot) { return false; }
            else if (caster != target.SlotID) { return false; }
            else return true;
        }

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            foreach (CombatSlot slot in slots.CharacterSlots)
            {
                TargetSlotInfo targ = slot.TargetSlotInformation;
                if (targ != null && targ.HasUnit && !IsUnitAlreadyContained(targets, targ) && !IsCastSlot(casterSlotID, targ))
                {
                    targets.Add(targ);
                }
            }
            foreach (CombatSlot slot in slots.EnemySlots)
            {
                TargetSlotInfo targ = slot.TargetSlotInformation;
                if (targ != null && targ.HasUnit && !IsUnitAlreadyContained(targets, targ) && !IsCastSlot(casterSlotID, targ))
                {
                    targets.Add(targ);
                }
            }
            return targets.ToArray();
        }
    }
    public class TargettingAllSlots : BaseCombatTargettingSO
    {
        public override bool AreTargetAllies => false;

        public override bool AreTargetSlots => false;

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            foreach (CombatSlot slot in slots.CharacterSlots)
            {
                TargetSlotInfo targ = slot.TargetSlotInformation;
                if (targ != null)
                {
                    targets.Add(targ);
                }
            }
            foreach (CombatSlot slot in slots.EnemySlots)
            {
                TargetSlotInfo targ = slot.TargetSlotInformation;
                if (targ != null)
                {
                    targets.Add(targ);
                }
            }
            return targets.ToArray();
        }
    }
    public class TargettingWeakestUnit : Targetting_ByUnit_Side
    {
        public bool OnlyOne;

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            foreach (TargetSlotInfo targ in base.GetTargets(slots, casterSlotID, isCasterCharacter))
            {
                if (targ != null && targ.HasUnit)
                {
                    if (targets.Count <= 0)
                    {
                        targets.Add(targ);
                    }
                    else if (targets[0].Unit.CurrentHealth > targ.Unit.CurrentHealth)
                    {
                        targets.Clear();
                        targets.Add(targ);
                    }
                    else if (targets[0].Unit.CurrentHealth == targ.Unit.CurrentHealth)
                    {
                        targets.Add(targ);
                    }
                }
            }
            if (targets.Count <= 0) return new TargetSlotInfo[0];
            if (OnlyOne) return new TargetSlotInfo[] { targets.GetRandom() };
            else return targets.ToArray();
        }
    }
    public class TargettingStrongestUnit : Targetting_ByUnit_Side
    {
        public bool OnlyOne;

        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            List<TargetSlotInfo> targets = new List<TargetSlotInfo>();
            foreach (TargetSlotInfo targ in base.GetTargets(slots, casterSlotID, isCasterCharacter))
            {
                if (targ != null && targ.HasUnit)
                {
                    if (targets.Count <= 0)
                    {
                        targets.Add(targ);
                    }
                    else if (targets[0].Unit.CurrentHealth < targ.Unit.CurrentHealth)
                    {
                        targets.Clear();
                        targets.Add(targ);
                    }
                    else if (targets[0].Unit.CurrentHealth == targ.Unit.CurrentHealth)
                    {
                        targets.Add(targ);
                    }
                }
            }
            if (targets.Count <= 0) return new TargetSlotInfo[0];
            if (OnlyOne) return new TargetSlotInfo[] { targets.GetRandom() };
            else return targets.ToArray();
        }
    }
    public class MultiTargetting : BaseCombatTargettingSO
    {
        public BaseCombatTargettingSO first;
        public BaseCombatTargettingSO second;
        public override bool AreTargetAllies => first.AreTargetAllies && second.AreTargetAllies;
        public override bool AreTargetSlots => first.AreTargetSlots && second.AreTargetSlots;
        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            TargetSlotInfo[] one = first.GetTargets(slots, casterSlotID, isCasterCharacter);
            TargetSlotInfo[] two = second.GetTargets(slots, casterSlotID, isCasterCharacter);
            TargetSlotInfo[] ret = new TargetSlotInfo[one.Length + two.Length];
            Array.Copy(one, ret, one.Length);
            Array.Copy(two, 0, ret, one.Length, two.Length);
            return ret;
        }

        public static MultiTargetting Create(BaseCombatTargettingSO first, BaseCombatTargettingSO second)
        {
            MultiTargetting ret = ScriptableObject.CreateInstance<MultiTargetting>();
            ret.first = first;
            ret.second = second;
            return ret;
        }
    }
    public class TargettingByHasUnit : BaseCombatTargettingSO
    {
        public BaseCombatTargettingSO source;
        public override bool AreTargetAllies => source.AreTargetAllies;
        public override bool AreTargetSlots => source.AreTargetSlots;
        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            TargetSlotInfo[] orig = source.GetTargets(slots, casterSlotID, isCasterCharacter);
            List<TargetSlotInfo> list = new List<TargetSlotInfo>();
            foreach (TargetSlotInfo target in orig)
            {
                if (target.HasUnit) list.Add(target);
            }
            return list.ToArray();
        }
        public static TargettingByHasUnit Create(BaseCombatTargettingSO orig)
        {
            TargettingByHasUnit ret = ScriptableObject.CreateInstance<TargettingByHasUnit>();
            ret.source = orig;
            return ret;
        }
    }
    public class TargettingByTargetting : BaseCombatTargettingSO
    {
        public BaseCombatTargettingSO first;
        public BaseCombatTargettingSO second;
        public bool OnlyIfUnit;
        public override bool AreTargetSlots => second.AreTargetSlots;
        public override bool AreTargetAllies => (first.AreTargetAllies && second.AreTargetAllies) || (!first.AreTargetAllies && !second.AreTargetAllies);
        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            TargetSlotInfo[] orig = first.GetTargets(slots, casterSlotID, isCasterCharacter);
            List<TargetSlotInfo> ret = new List<TargetSlotInfo>();
            foreach (TargetSlotInfo target in orig)
            {
                if (!target.HasUnit && OnlyIfUnit) continue;
                foreach (TargetSlotInfo add in second.GetTargets(slots, target.HasUnit ? target.Unit.SlotID : target.SlotID, target.IsTargetCharacterSlot))
                {
                    ret.Add(add);
                }
            }
            return ret.ToArray();
        }
        public static TargettingByTargetting Create(BaseCombatTargettingSO first, BaseCombatTargettingSO second)
        {
            TargettingByTargetting ret = ScriptableObject.CreateInstance<TargettingByTargetting>();
            ret.first = first;
            ret.second = second;
            return ret;
        }
    }
    public class TargettingByConditionStatus : BaseCombatTargettingSO
    {
        public BaseCombatTargettingSO orig;

        public StatusEffectType status = StatusEffectType.Frail;

        public bool Has;
        public bool EmptySlots;
        public override bool AreTargetAllies => orig.AreTargetAllies;
        public override bool AreTargetSlots => orig.AreTargetSlots;
        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            TargetSlotInfo[] targets = orig.GetTargets(slots, casterSlotID, isCasterCharacter);
            List<TargetSlotInfo> ret = new List<TargetSlotInfo>();
            foreach (TargetSlotInfo target in targets)
            {
                if (target.HasUnit && (Has == target.Unit.ContainsStatusEffect(status))) ret.Add(target);
                else if (!target.HasUnit && EmptySlots) ret.Add(target);
            }
            return ret.ToArray();
        }

        public static TargettingByConditionStatus Create(BaseCombatTargettingSO orig, StatusEffectType status, bool Has = true, bool empties = false)
        {
            TargettingByConditionStatus ret = ScriptableObject.CreateInstance<TargettingByConditionStatus>();
            ret.orig = orig;
            ret.status = status;
            ret.Has = Has;
            ret.EmptySlots = empties;
            return ret;
        }
    }
}
