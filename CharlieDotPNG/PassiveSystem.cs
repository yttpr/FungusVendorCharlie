using BrutalAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CharlieDotPNG
{
    public static class PassiveSystem
    {
        public static PassiveAbilityTypes Type => (PassiveAbilityTypes)2951742;
        public static TriggerCalls Post => (TriggerCalls)8102472;
    }
    public static class CharStensions
    {
        public static int After = 0;
        public static List<T> Firsten<T>(this List<T> list, T add)
        {
            List<T> newer = new List<T>() { add };
            foreach (T og in list) newer.Add(og);
            return newer;
        }
        public static void MoveToFirst(this TimelineSlotGroup self)
        {
            if (Charlie.Debugging) Debug.Log(self + " move to first");
            self.slot.transform.SetSiblingIndex(2);
            self.intent.transform.SetSiblingIndex(2);
        }
        public static TimelineSlotGroup PrepareFrontUnusedSlot(this TimelineZoneLayout self, Sprite enemy, Sprite[] intents, Color[] intentColors)
        {
            if (Charlie.Debugging) Debug.Log(self + " prepare front unused slot");
            if (self._unusedSlots.Count <= 0)
            {
                self.GenerateUnusedSlot();
            }

            TimelineSlotGroup timelineSlotGroup = self._unusedSlots.Dequeue();
            timelineSlotGroup.MoveToFirst();
            timelineSlotGroup.SetInformation(self._slotsInUse.Count, enemy, intents, intentColors);
            timelineSlotGroup.SetActivation(enable: true);
            self._slotsInUse = self._slotsInUse.Firsten(timelineSlotGroup);
            //self._slotsInUse.Add(timelineSlotGroup);
            self._pointerRect.SetAsLastSibling();
            return timelineSlotGroup;
        }
        public static IEnumerator AddFrontTimelineSlots(this TimelineZoneLayout self, Sprite[] turnSprites, AbilitySO[] abilities)
        {
            if (Charlie.Debugging) Debug.Log(self + " add front timeline slots");
            int count = self._slotsInUse.Count;
            count = 0;
            for (int i = 0; i < turnSprites.Length; i++)
            {
                Sprite enemy;
                Sprite[] intents;
                Color[] spriteColors;
                if (turnSprites[i] == null)
                {
                    enemy = self._blindTimelineIcon;
                    intents = new Sprite[0];
                    spriteColors = new Color[0];
                }
                else
                {
                    enemy = turnSprites[i];
                    intents = self.IntentHandler.GenerateSpritesFromAbility(abilities[i], casterIsCharacter: false, out spriteColors);
                }

                TimelineSlotGroup timelineSlotGroup = self.PrepareFrontUnusedSlot(enemy, intents, spriteColors);
                timelineSlotGroup.SetSlotScale(grow: false);
                timelineSlotGroup.SetActivation(enable: false);
            }

            for (int j = count; j < self._slotsInUse.Count && j < turnSprites.Length; j++)
            {
                self._slotsInUse[j].GenerateTweenScale(grow: true, self._timelineMoveTime);
                self._slotsInUse[j].SetActivation(enable: true);
            }

            self.UpdateTimelineContentSize(self._slotsInUse.Count + 1);
            yield return self.UpdateTimelineBackgroundSize(self._slotsInUse.Count + 1);
        }
        public static void AddTimelineFrontTurn(this EnemyCombatUIInfo self, TurnUIInfo turn)
        {
            if (Charlie.Debugging) Debug.Log(self + " add tiemline front turn");
            if (!turn.isSecret && turn.abilitySlotID >= 0 && turn.abilitySlotID < self.AbilityTimelineSlots.Count)
            {
                self.AbilityTimelineSlots[turn.abilitySlotID].Add(0);//turn.timeSlotID
            }
        }
        public static IEnumerator AddFrontTimelineSlots(this CombatVisualizationController self, TurnUIInfo[] enemyTurns)
        {
            if (Charlie.Debugging) Debug.Log(self + " add front timeline slots");
            Sprite[] array = new Sprite[enemyTurns.Length];
            AbilitySO[] array2 = new AbilitySO[enemyTurns.Length];
            for (int i = 0; i < enemyTurns.Length; i++)
            {
                TurnUIInfo turnUIInfo = enemyTurns[i];
                //List<TimelineInfo> gap = new List<TimelineInfo>(self._timelineSlotInfo);
                //self._timelineSlotInfo.Clear();
                //self._timelineSlotInfo.Add(new TimelineInfo(turnUIInfo));
                //for (int b = 0; b < gap.Count; b++) self._timelineSlotInfo.Add(gap[b]);
                self._timelineSlotInfo.Add(new TimelineInfo(turnUIInfo));
                foreach (EnemyCombatUIInfo uiin in self._enemiesInCombat.Values)
                {
                    foreach (List<int> dual in uiin.AbilityTimelineSlots)
                    {
                        List<int> newer = new List<int>(dual);
                        dual.Clear();
                        foreach (int inni in newer) dual.Add(inni + 1);
                    }
                }
                EnemyCombatUIInfo enemyCombatUIInfo = self._enemiesInCombat[turnUIInfo.enemyID];
                enemyCombatUIInfo.AddTimelineFrontTurn(turnUIInfo);
                array[i] = (turnUIInfo.isSecret ? null : enemyCombatUIInfo.Portrait);
                array2[i] = enemyCombatUIInfo.Abilities[turnUIInfo.abilitySlotID].ability;
            }
            //self.ReadOutUI(self._timelineSlotInfo);
            yield return self._timeline.AddFrontTimelineSlots(array, array2);
            if (!self._isInfoFromCharacter && self._unitInfoID != -1)
            {
                self.TryUpdateEnemyIDInformation(self._unitInfoID);
            }
        }
        public static void AddFrontExtraEnemyTurns(this Timeline self, List<EnemyCombat> units, List<int> abilitySlots)
        {
            if (Charlie.Debugging) Debug.Log(self + " add front extra enemy turns");
            TurnUIInfo[] list = new TurnUIInfo[units.Count];
            for (int i = 0; i < units.Count; i++)
            {
                if (self.Enemies.Contains(units[i]))
                {
                    TurnInfo item = new TurnInfo(units[i], abilitySlots[i], player: false);
                    List<TurnInfo> gap = new List<TurnInfo>(self.Round);
                    self.Round.Clear();
                    self.Round.Add(gap[0]);
                    self.Round.Add(item);
                    for (int w = 1; w < gap.Count; w++) self.Round.Add(gap[w]);
                    list[units.Count - (i + 1)] = item.GenerateTurnUIInfo(1, self.IsConfused);//units.Count - (i + 1)
                }
            }
            //ReadOutRound(self.Round);
            CombatManager.Instance.AddUIAction(new AddedSlotsFrontTimelineUIAction(list.ToArray()));
        }
        public static void TryAddNewFrontExtraEnemyTurns(this Timeline self, ITurn unit, int turnsToAdd)
        {
            if (Charlie.Debugging) Debug.Log(self + " try add new front extra enemy turns");
            if (self.Enemies.Contains(unit))
            {
                TurnUIInfo[] list = new TurnUIInfo[turnsToAdd];
                for (int i = 0; i < turnsToAdd; i++)
                {
                    int singleAbilitySlotUsage = unit.GetSingleAbilitySlotUsage(-1);
                    TurnInfo item = new TurnInfo(unit, singleAbilitySlotUsage, player: false);
                    List<TurnInfo> gap = new List<TurnInfo>(self.Round);
                    self.Round.Clear();
                    self.Round.Add(gap[0]);
                    self.Round.Add(item);
                    for (int w = 1; w < gap.Count; w++) self.Round.Add(gap[w]);
                    list[turnsToAdd - (i + 1)] = item.GenerateTurnUIInfo(1, self.IsConfused);
                }
                //ReadOutRound(self.Round);
                CombatManager.Instance.AddUIAction(new AddedSlotsFrontTimelineUIAction(list.ToArray()));
            }
        }

        public static void ReadOutRound(List<TurnInfo> list)
        {
            if (Charlie.Debugging) Debug.Log("IN GAME TIMELINE");
            foreach (TurnInfo turn in list)
            {
                if (Charlie.Debugging) Debug.Log("TURN: " + turn);
                if (turn.turnUnit is EnemyCombat enemy) Debug.Log(turn.turnUnit + " " + enemy._currentName + " Slot: " + turn.turnUnit.SlotID);
                else Debug.Log(turn.turnUnit);
                //try { if (turn.turnUnit is EnemyCombat EN) Debug.Log(EN.Abilities[turn.abilitySlot]); }
                //catch (Exception ex) { }
            }
        }
        public static void ReadOutUI(this CombatVisualizationController self, List<TimelineInfo> list)
        {
            if (Charlie.Debugging) Debug.Log("UI TIMELINE");
            foreach (TimelineInfo turn in list)
            {
                if (Charlie.Debugging) Debug.Log("TURN: " + turn);
                if (self._enemiesInCombat.TryGetValue(turn.enemyID, out EnemyCombatUIInfo enemy)) Debug.Log(enemy + " " + enemy.EnemyBase._enemyName + " Slot: " + enemy.SlotID);
                else Debug.LogWarning("Didn't find the enemy for: " + turn.enemyID);
            }
        }

        public static int GetCharlieAbilityIDFromName(this EnemyCombat self, string abilityName)
        {
            if (Charlie.Debugging) Debug.Log(abilityName);
            for (int num = self.Abilities.Count - 1; num >= 0; num--)
            {
                string abil = self.Abilities[num].ability._abilityName;
                if (Charlie.Debugging) Debug.Log(abil);
                if (TheEnemy.L_Slay.Contains(abil) && TheEnemy.L_Slay.Contains(abilityName)) return num;
                else if (TheEnemy.R_Slay.Contains(abil) && TheEnemy.R_Slay.Contains(abilityName)) return num;
                else if (TheEnemy.R_Splice.Contains(abil) && TheEnemy.R_Splice.Contains(abilityName)) return num;
                else if (TheEnemy.L_Splice.Contains(abil) && TheEnemy.L_Splice.Contains(abilityName)) return num;
                else if (abilityName == abil) return num;
            }

            return self.GetLastAbilityIDFromName(abilityName);
        }
    }
    public class AddedSlotsFrontTimelineUIAction : CombatAction
    {
        public TurnUIInfo[] _enemyTurns;

        public AddedSlotsFrontTimelineUIAction(TurnUIInfo[] enemyTurns)
        {
            _enemyTurns = enemyTurns;
        }

        public override IEnumerator Execute(CombatStats stats)
        {
            Debug.Log("Added slots front timeline ui action");
            yield return stats.combatUI.AddFrontTimelineSlots(_enemyTurns);
        }
    }
    public class AddCharlieSpecificAbilityEnemyTimelineAction : CombatAction
    {
        public EnemyCombat enemy;
        public string Ability;
        public AddCharlieSpecificAbilityEnemyTimelineAction(EnemyCombat enemy, string Ability)
        {
            this.enemy = enemy;
            this.Ability = Ability;
        }
        public override IEnumerator Execute(CombatStats stats)
        {
            List<EnemyCombat> units = new List<EnemyCombat>();
            List<int> abilitySlots = new List<int>();
            int abilityIdFromName = enemy.GetCharlieAbilityIDFromName(Ability);
            if (abilityIdFromName >= 0)
            {
                units.Add(enemy);
                abilitySlots.Add(abilityIdFromName);
            }
            try
            {
                if (stats.IsPlayerTurn)
                {
                    if (abilitySlots.Count > 0) stats.timeline.AddFrontExtraEnemyTurns(units, abilitySlots);
                    else stats.timeline.TryAddNewFrontExtraEnemyTurns(enemy, 1);
                }
                else
                {
                    if (abilitySlots.Count > 0) stats.timeline.AddExtraEnemyTurns(units, abilitySlots);
                    else stats.timeline.TryAddNewExtraEnemyTurns(enemy, 1);
                }
            }
            catch
            {
                Debug.LogError("failed to add CHarlie to front of timeline");
                if (abilitySlots.Count > 0) stats.timeline.AddExtraEnemyTurns(units, abilitySlots);
                else stats.timeline.TryAddNewExtraEnemyTurns(enemy, 1);
            }
            yield return null;
        }
    }
    public class AddCharlieEnemyAbilityFrontEffect : EffectSO
    {
        [SerializeField]
        public string ability;
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            exitAmount = 0;
            foreach (TargetSlotInfo target in targets)
            {
                if (target.HasUnit && target.Unit is EnemyCombat enemy)
                    CombatManager.Instance.AddPrioritySubAction(new AddCharlieSpecificAbilityEnemyTimelineAction(enemy, ability));
            }
            return true;
        }
        public static AddCharlieEnemyAbilityFrontEffect Create(string ability)
        {
            AddCharlieEnemyAbilityFrontEffect ret = ScriptableObject.CreateInstance<AddCharlieEnemyAbilityFrontEffect>();
            ret.ability = ability;
            return ret;
        }
    }
    public class TargettingByPassive : Targetting_ByUnit_Side
    {
        public PassiveAbilityTypes type;
        public override TargetSlotInfo[] GetTargets(SlotsCombat slots, int casterSlotID, bool isCasterCharacter)
        {
            TargetSlotInfo[] targets = base.GetTargets(slots, casterSlotID, isCasterCharacter);
            List<TargetSlotInfo> ret = new List<TargetSlotInfo>();
            foreach (TargetSlotInfo target in targets) if (target.HasUnit && target.Unit.ContainsPassiveAbility(type)) ret.Add(target);
            return ret.ToArray();
        }
    }
    public class AllyCondition : EffectorConditionSO
    {
        public static UnitStoredValueNames value => (UnitStoredValueNames)740213;
        public override bool MeetCondition(IEffectorChecks effector, object args)
        {
            List<int> IDs = new List<int>();
            List<bool> bools = new List<bool>();
            List<string> strings = new List<string>();
            List<Sprite> sprites = new List<Sprite>();

            CombatStats stats = CombatManager.Instance._stats;

             if (args is DamageReceivedValueChangeException && effector is IUnit unit) { unit.SetStoredValue(value, 1); return false; }
            else if (args is CanHealReference && effector is IUnit unor) { unor.SetStoredValue(value, 0); return false; }
            else
            {
                foreach (CharacterCombat chara in stats.CharactersOnField.Values) if (chara.ContainsPassiveAbility(PassiveSystem.Type) && chara.IsAlive)
                    {
                        IDs.Add(chara.ID);
                        bools.Add(chara.IsUnitCharacter);
                        strings.Add(TheEnemy.Ally._passiveName);
                        sprites.Add(TheEnemy.Ally.passiveIcon);
                    }
                foreach (EnemyCombat enemy in stats.EnemiesOnField.Values) if (enemy.ContainsPassiveAbility(PassiveSystem.Type) && enemy.IsAlive)
                    {
                        IDs.Add(enemy.ID);
                        bools.Add(enemy.IsUnitCharacter);
                        strings.Add(TheEnemy.Ally._passiveName);
                        sprites.Add(TheEnemy.Ally.passiveIcon);
                    }

                CombatManager.Instance.AddUIAction(new ShowMultiplePassiveInformationUIAction(IDs.ToArray(), bools.ToArray(), strings.ToArray(), sprites.ToArray()));
            }

            if (args is DeathReference reff && !reff.witheringDeath && effector is IUnit iu && iu.GetStoredValue(value) != 100)
            {
                foreach (CharacterCombat chara in stats.CharactersOnField.Values) if (chara.ContainsPassiveAbility(PassiveSystem.Type) && chara.IsAlive && chara.CurrentHealth > 0 && chara != iu) chara.DirectDeath(null);
                foreach (EnemyCombat enemy in stats.EnemiesOnField.Values) if (enemy.ContainsPassiveAbility(PassiveSystem.Type) && enemy.IsAlive && enemy.CurrentHealth > 0 && enemy != iu) enemy.DirectDeath(null);
            }
            else if (args is IntegerReference reffe && effector is IUnit unii)
            {
                int amount = unii.GetStoredValue(value) == 1 ? reffe.value * -1 : reffe.value;
                unii.SetStoredValue(value, 0);
                foreach (CharacterCombat chara in stats.CharactersOnField.Values)
                    if (chara.ContainsPassiveAbility(PassiveSystem.Type) && chara.IsAlive && chara.CurrentHealth > 0 && chara != unii)
                    {
                        if (chara.CurrentHealth + amount > 0) chara.SetHealthTo(chara.CurrentHealth + amount);
                        else
                        {
                            chara.SetStoredValue(value, 100);
                            chara.DirectDeath(null);
                        }
                    }
                foreach (EnemyCombat enemy in stats.EnemiesOnField.Values)
                    if (enemy.ContainsPassiveAbility(PassiveSystem.Type) && enemy.IsAlive && enemy.CurrentHealth > 0 && enemy != unii)
                    {
                        if (enemy.CurrentHealth + amount > 0) enemy.SetHealthTo(enemy.CurrentHealth + amount);
                        else
                        {
                            enemy.SetStoredValue(value, 100);
                            enemy.DirectDeath(null);
                        }
                    }
            }
            else
            {
                foreach (CharacterCombat chara in stats.CharactersOnField.Values)
                    if (chara.ContainsPassiveAbility(PassiveSystem.Type) && chara.IsAlive && chara != effector)
                    {
                        if (effector.CurrentHealth > 0) chara.SetHealthTo(effector.CurrentHealth);
                        else
                        {
                            chara.SetStoredValue(value, 100);
                            chara.DirectDeath(null);
                        }
                    }
                foreach (EnemyCombat enemy in stats.EnemiesOnField.Values)
                    if (enemy.ContainsPassiveAbility(PassiveSystem.Type) && enemy.IsAlive && enemy != effector)
                    {
                        if (effector.CurrentHealth > 0) enemy.SetHealthTo(effector.CurrentHealth);
                        else
                        {
                            enemy.SetStoredValue(value, 100);
                            enemy.DirectDeath(null);
                        }
                    }
            }
            return false;
        }
    }
    public class IsAliveCondition : EffectorConditionSO
    {
        public override bool MeetCondition(IEffectorChecks effector, object args)
        {
            return effector.CurrentHealth > 0;
        }
    }
    public class HiddenMultiattackZeroCondition : EffectorConditionSO
    {
        public static UnitStoredValueNames value => (UnitStoredValueNames)2299114;
        public override bool MeetCondition(IEffectorChecks effector, object args)
        {
            if (effector is IUnit unit)
            {
                if (args is DamageReceivedValueChangeException)
                {
                    unit.SetStoredValue(value, 1);
                    return false;
                }
                else if (args is IntegerReference && unit.GetStoredValue(value) == 1)
                {
                    unit.SetStoredValue(value, 0);
                    return effector.CurrentHealth > 0;
                }
                else if (args is IntegerReference) { }
                else
                {
                    unit.SetStoredValue(value, 0);
                    return false;
                }
            }
            CharStensions.After = 0;
            if (args is IntegerReference reff)
            {
                reff.value--;
                return false;
            }
            return effector.CurrentHealth > 0;
        }
    }
    public class SetToCasterHealthAndMaxEffect : EffectSO
    {
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            exitAmount = 0;
            foreach (TargetSlotInfo target in targets)
            {
                if (target.HasUnit)
                {
                    if (target.Unit.MaximumHealth != caster.MaximumHealth)
                    {
                        exitAmount += Math.Abs(target.Unit.MaximumHealth - caster.MaximumHealth);
                        target.Unit.MaximizeHealth(caster.MaximumHealth);
                    }
                    if (target.Unit.CurrentHealth != caster.CurrentHealth)
                    {
                        exitAmount += Math.Abs(target.Unit.CurrentHealth - caster.CurrentHealth);
                        target.Unit.SetHealthTo(caster.CurrentHealth);
                    }
                }
            }
            return exitAmount > 0;
        }
    }
    public class SetCasterHealthToTargetHealthAndMaxEffect : EffectSO
    {
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            exitAmount = 0;
            foreach (TargetSlotInfo target in targets)
            {
                if (target.HasUnit)
                {
                    if (target.Unit.MaximumHealth < caster.MaximumHealth)
                    {
                        exitAmount += Math.Abs(target.Unit.MaximumHealth - caster.MaximumHealth);
                        caster.MaximizeHealth(target.Unit.MaximumHealth);
                    }
                    if (target.Unit.CurrentHealth < caster.CurrentHealth)
                    {
                        exitAmount += Math.Abs(target.Unit.CurrentHealth - caster.CurrentHealth);
                        caster.SetHealthTo(target.Unit.CurrentHealth);
                    }
                }
            }
            return exitAmount > 0;
        }
    }
    public class FleeFirstInLineEffect : FleeTargetEffect
    {
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            exitAmount = 0;
            foreach (TargetSlotInfo target in targets) if (target.HasUnit) return base.PerformEffect(stats, caster, target.SelfArray(), areTargetSlots, entryVariable, out exitAmount);
            return false;
        }
    }
    public class SpawnEnemyFrontThenAnyEffect : SpawnEnemyAnywhereEffect
    {
        public string GetEnemy;
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            enemy = LoadedAssetsHandler.GetEnemy(GetEnemy);
            for (int i = 0; i < entryVariable; i++)
            {
                CombatManager.Instance.AddSubAction(new SpawnEnemyAction(enemy, caster.SlotID, givesExperience, trySpawnAnyways: true, _spawnType));
            }

            exitAmount = entryVariable;
            return true;
        }
        public static SpawnEnemyFrontThenAnyEffect Create(string en)
        {
            SpawnEnemyFrontThenAnyEffect ret = ScriptableObject.CreateInstance<SpawnEnemyFrontThenAnyEffect>();
            ret.GetEnemy = en;
            return ret;
        }
    }
    public class ScarsIfNoScarsEffect : ApplyScarsEffect
    {
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            exitAmount = 0;
            foreach (TargetSlotInfo target in targets)
            {
                if (target.HasUnit)
                {
                    if (target.Unit.ContainsStatusEffect(StatusEffectType.Scars)) continue;
                    base.PerformEffect(stats, caster, target.SelfArray(), areTargetSlots, entryVariable, out int exi);
                    exitAmount += exi;
                }
            }
            return exitAmount > 0;
        }
    }
    public class FleeExtraCharliesCondition : EffectConditionSO
    {
        public override bool MeetCondition(IUnit caster, EffectInfo[] effects, int currentIndex)
        {
            int otherenemies = 0;
            int extracharlies = 0;
            List<EnemyCombat> charlies = new List<EnemyCombat>();
            foreach (EnemyCombat enemy in CombatManager.Instance._stats.EnemiesOnField.Values)
            {
                if (enemy.ContainsPassiveAbility(TheEnemy.Ally.type)) charlies.Add(enemy);
                else otherenemies++;
            }
            foreach (CharacterCombat chara in CombatManager.Instance._stats.CharactersOnField.Values)
            {
                if (chara.ContainsPassiveAbility(TheEnemy.Ally.type)) extracharlies++;
            }
            if (otherenemies > 0 || extracharlies > 0) return true;
            else foreach (EnemyCombat charlie in charlies)
                {
                    charlie.UnitWillFlee();
                    CombatManager.Instance.AddSubAction(new FleetingUnitAction(charlie.ID, charlie.IsUnitCharacter));
                }
            return false;
        }
    }
    public class SwapCharlieAbilitiesEffect : SwapCasterAbilitiesEffect
    {
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            int rank = 0;
            if (caster is CharacterCombat chara) rank = chara.Rank;
            rank = Math.Min(rank, 3);
            rank = Math.Max(rank, 0);
            _abilitiesToSwap = TheCharacter.Control[rank];
            
            
            return base.PerformEffect(stats, caster, targets, areTargetSlots, entryVariable, out exitAmount);
        }
    }
    public class SpawnCharlieEffect : SpawnEnemyFrontThenAnyEffect
    {
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            int rank = 0;
            if (caster is CharacterCombat chara) rank = chara.Rank;
            if (Charlie.Debugging) Debug.Log(rank);
            //if (TheEnemy.IDs.Length > 0 && rank < TheEnemy.IDs.Length) GetEnemy = TheEnemy.IDs[rank];
            //else GetEnemy = TheEnemy.IDs[0];
            try
            {
                GetEnemy = TheEnemy.IDs[rank];
                if (Charlie.Debugging) Debug.Log(GetEnemy);
            }
            catch { exitAmount = 0; return false; };
            return base.PerformEffect(stats, caster, targets, areTargetSlots, entryVariable, out exitAmount);
        }
    }
    public class AddCharlieSpecificAbilityEnemyTimelineBackAction : CombatAction
    {
        public EnemyCombat enemy;
        public string Ability;
        public AddCharlieSpecificAbilityEnemyTimelineBackAction(EnemyCombat enemy, string Ability)
        {
            this.enemy = enemy;
            this.Ability = Ability;
        }
        public override IEnumerator Execute(CombatStats stats)
        {
            List<EnemyCombat> units = new List<EnemyCombat>();
            List<int> abilitySlots = new List<int>();
            int abilityIdFromName = enemy.GetCharlieAbilityIDFromName(Ability);
            if (abilityIdFromName >= 0)
            {
                units.Add(enemy);
                abilitySlots.Add(abilityIdFromName);
            }
            if (abilitySlots.Count > 0) stats.timeline.AddExtraEnemyTurns(units, abilitySlots);
            else stats.timeline.TryAddNewExtraEnemyTurns(enemy, 1);
            yield return null;
        }
    }
    public class AddCharlieEnemyAbilityBackEffect : EffectSO
    {
        [SerializeField]
        public string ability;
        public override bool PerformEffect(CombatStats stats, IUnit caster, TargetSlotInfo[] targets, bool areTargetSlots, int entryVariable, out int exitAmount)
        {
            exitAmount = 0;
            foreach (TargetSlotInfo target in targets)
            {
                if (target.HasUnit && target.Unit is EnemyCombat enemy)
                    CombatManager.Instance.AddPrioritySubAction(new AddCharlieSpecificAbilityEnemyTimelineBackAction(enemy, ability));
            }
            return true;
        }
        public static AddCharlieEnemyAbilityBackEffect Create(string ability)
        {
            AddCharlieEnemyAbilityBackEffect ret = ScriptableObject.CreateInstance<AddCharlieEnemyAbilityBackEffect>();
            ret.ability = ability;
            return ret;
        }
    }
}
