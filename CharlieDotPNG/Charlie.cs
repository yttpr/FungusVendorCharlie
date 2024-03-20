using BepInEx;
using BrutalAPI;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CharlieDotPNG
{
    [BepInPlugin("FungusVendor.Charlie", "Charlie Mod", "1.0.5")]
    [BepInDependency("Bones404.BrutalAPI", BepInDependency.DependencyFlags.HardDependency)]
    public class Charlie : BaseUnityPlugin
    {
        public static bool Debugging = false;
        public static AssetBundle Assets;
        public void Awake()
        {
            try
            {
                Assets = AssetBundle.LoadFromMemory(ResourceLoader.ResourceBinary("friendship"));
            }
            catch
            {
                Debug.LogError("THE MOD FUCKING BROKE!!!!");
                return;
            }
            EZExtensions.PCall(AbilityNameFix.Setup, "ability name fixinator");
            EZExtensions.PCall(TheEnemy.L1, "level 1 enemy charlie");
            EZExtensions.PCall(TheEnemy.L2, "level 2 enemy charlie");
            EZExtensions.PCall(TheEnemy.L3, "level 3 enemy charlie");
            EZExtensions.PCall(TheEnemy.L4, "level 4 enemy charlie");
            EZExtensions.PCall(TheCharacter.Add, "character charlie");
            EZExtensions.PCall(CustomIntentIconSystem.Setup, "Custom intents");
            Logger.LogInfo("ITS FUCKING LOADED!!");
        }
    }

    public static class TheEnemy
    {
        public static string[] IDs => new string[] { "CharlieLv1_EN", "CharlieLv2_EN", "CharlieLv3_EN", "CharlieLv4_EN" };
        static EnemyInFieldLayout _prefab;
        public static EnemyInFieldLayout Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = Charlie.Assets.LoadAsset<GameObject>("Assets/Charlie.prefab").AddComponent<EnemyInFieldLayout>();
                    _prefab._gibs = Charlie.Assets.LoadAsset<GameObject>("Assets/Gibs_Charlie.prefab").GetComponent<ParticleSystem>();
                    _prefab.SetDefaultParams();
                }
                return _prefab;
            }
        }
        static Sprite[] _icons;
        public static Sprite[] Icons
        {
            get
            {
                if (_icons == null || _icons.Length <= 0)
                {
                    _icons = new Sprite[5];
                    _icons[0] = ResourceLoader.LoadSprite("LeftSlay_icon.png");
                    _icons[1] = ResourceLoader.LoadSprite("LeftSplice_icon.png");
                    _icons[2] = ResourceLoader.LoadSprite("RightSlay_icon.png");
                    _icons[3] = ResourceLoader.LoadSprite("RightSplice_icon.png");
                    _icons[4] = ResourceLoader.LoadSprite("DontDoThat_icon.png");
                }
                return _icons;
            }
        }
        public static string[] L_Slay => new string[] { "Left Sway", "Left Whirl", "Left Wave", "Left Slay" };
        public static string[] R_Slay => new string[] { "Right Sway", "Right Whirl", "Right Wave", "Right Slay" };
        public static string[] L_Splice => new string[] { "Left Thrash", "Left Rush", "Left Grind", "Left Splice" };
        public static string[] R_Splice => new string[] { "Right Thrash", "Right Rush", "Right Grind", "Right Splice" };
        static BasePassiveAbilitySO _ally;
        public static BasePassiveAbilitySO Ally
        {
            get
            {
                if (_ally == null)
                {
                    PerformEffectPassiveAbility p = ScriptableObject.CreateInstance<PerformEffectPassiveAbility>();
                    p._passiveName = "Ally";
                    p.passiveIcon = ResourceLoader.LoadSprite("Ally_icon.png");
                    p.doesPassiveTriggerInformationPanel = false;
                    p.effects = new EffectInfo[0];
                    p.conditions = new EffectorConditionSO[] { ScriptableObject.CreateInstance<AllyCondition>() };
                    p._enemyDescription = "This enemy is an ally.";
                    p._characterDescription = "This character is an ally.";
                    p.type = PassiveSystem.Type;
                    p._triggerOn = new TriggerCalls[] { TriggerCalls.OnDamaged, TriggerCalls.OnBeingDamaged, TriggerCalls.OnHealed, TriggerCalls.CanHeal, TriggerCalls.OnDeath, PassiveSystem.Post };
                    _ally = p;
                }
                return _ally;
            }
        }
        static BasePassiveAbilitySO _maso;
        public static BasePassiveAbilitySO Maso
        {
            get
            {
                if (_maso == null)
                {
                    PerformEffectPassiveAbility p = ScriptableObject.CreateInstance<PerformEffectPassiveAbility>();
                    p._passiveName = "Masochism";
                    p.passiveIcon = Passives.Masochism.passiveIcon;
                    p._enemyDescription = "Upon receiving any kind of damage, this enemy will queue the aditional ability \"Don't Do That!\"";
                    p._characterDescription = "la";
                    p.type = PassiveAbilityTypes.Masochism;
                    p.conditions = new EffectorConditionSO[] { ScriptableObject.CreateInstance<HiddenMultiattackZeroCondition>() };
                    p.doesPassiveTriggerInformationPanel = true;
                    p._triggerOn = new TriggerCalls[] { TriggerCalls.OnDamaged, TriggerCalls.OnBeingDamaged, TriggerCalls.OnRoundFinished, TriggerCalls.AttacksPerTurn, PassiveSystem.Post };
                    p.effects = ExtensionMethods.ToEffectInfoArray(new Effect[]
                    {
                        new Effect(AddCharlieEnemyAbilityFrontEffect.Create("Don't Do That!"), 1, null, Slots.Self)
                    });

                    _maso = p;
                }
                return _maso;
            }
        }
        static BaseCombatTargettingSO _allies;
        public static BaseCombatTargettingSO Allies
        {
            get
            {
                if (_allies == null)
                {
                    TargettingByPassive ret = EZEffects.TargetSide<TargettingByPassive>(false, false) as TargettingByPassive;
                    ret.type = Ally.type;
                    _allies = ret;
                }
                return _allies;
            }
        }

        public static void Add(int level)
        {
            Enemy enemy = new Enemy()
            {
                name = "Charlie",
                health = 14,
                size = 1,
                entityID = (EntityIDs)2023114,
                healthColor = Pigments.Purple,
                priority = 0,
                prefab = Prefab
            };
            if (level == 0) enemy.health = 14;
            else if (level == 1) enemy.health = 17;
            else if (level == 2) enemy.health = 19;
            else if (level == 3) enemy.health = 21;
            enemy.enemyID = IDs[level];
            enemy.combatSprite = TheCharacter.Icons[4];
            enemy.overworldAliveSprite = TheCharacter.Icons[2];
            enemy.overworldDeadSprite = TheCharacter.Icons[2];
            enemy.hurtSound = LoadedAssetsHandler.GetCharcater("Bimini_CH").damageSound;
            enemy.deathSound = LoadedAssetsHandler.GetCharcater("Bimini_CH").deathSound;
            enemy.abilitySelector = ScriptableObject.CreateInstance<AbilitySelector_ByRarity>();
            enemy.passives = new BasePassiveAbilitySO[]
            {
                Ally, Maso,
            };

            enemy.enterEffects = new Effect[]
            {
                new Effect(ScriptableObject.CreateInstance<SetCasterHealthToTargetHealthAndMaxEffect>(), 1, null, Allies)
            };

            int[] slay = new int[] { 4, 7, 9, 10 };
            int[] splice = new int[] { 6, 9, 12, 14 };
            int[] chance = new int[] { 10, 20, 30, 40 };
            int[] dont = new int[] { 3, 4, 5, 6 };
            
            Ability Lslay = new Ability();
            Lslay.name = L_Slay[level];
            Lslay.description = "Deal " + slay[level].ToString() + " damage to the Left enemy. \nMove Charlie to the Left. Move the Opposing party member to the Right.";
            Lslay.sprite = Icons[0];
            Lslay.rarity = 5;
            Lslay.cost = new ManaColorSO[] { Pigments.Red, Pigments.Yellow };
            Lslay.animationTarget = Slots.SlotTarget(new int[] { -1 }, true);
            Lslay.visuals = LoadedAssetsHandler.GetCharacterAbility("Expire_1_A").visuals;
            Lslay.effects = new Effect[3];
            Lslay.effects[0] = new Effect(ScriptableObject.CreateInstance<DamageEffect>(), slay[level], GetDamage(slay[level]), Lslay.animationTarget);
            Lslay.effects[1] = new Effect(MoveSide[0], 1, IntentType.Swap_Left, Slots.Self);
            Lslay.effects[2] = new Effect(MoveSide[1], 1, IntentType.Swap_Right, Slots.Front);
            Ability Rslay = Lslay.Duplicate();
            Rslay.name = R_Slay[level];
            Rslay.description = "Deal " + slay[level].ToString() + " damage to the Right enemy. \nMove Charlie to the Right. Move the Opposing party member to the Left.";
            Rslay.sprite = Icons[2];
            Rslay.animationTarget = Slots.SlotTarget(new int[] { 1 }, true);
            Rslay.effects[0]._target = Rslay.animationTarget;
            Rslay.effects[1]._effect = MoveSide[1];
            Rslay.effects[1]._intent = IntentType.Swap_Right;
            Rslay.effects[2]._effect = MoveSide[0];
            Rslay.effects[2]._intent = IntentType.Swap_Left;
            Ability Lspli = new Ability();
            Lspli.name = L_Splice[level];
            Lspli.description = chance[level].ToString() + "% chance to inflict 2 Ruptured on the Left enemy. Deal " + splice[level].ToString() + " damage to the Left enemy. \nDeal 2 damage to the Left and Opposing party members.";
            Lspli.sprite = Icons[1];
            Lspli.rarity = 5;
            Lspli.cost = new ManaColorSO[] { Pigments.Red, Pigments.Red, Pigments.Yellow };
            Lspli.animationTarget = Lslay.animationTarget;
            Lspli.visuals = LoadedAssetsHandler.GetEnemyAbility("Domination_A").visuals;
            Lspli.effects = new Effect[3];
            Lspli.effects[0] = new Effect(ScriptableObject.CreateInstance<ApplyRupturedEffect>(), 2, IntentType.Status_Ruptured, Lspli.animationTarget, Conditions.Chance(chance[level]));
            Lspli.effects[1] = new Effect(ScriptableObject.CreateInstance<DamageEffect>(), splice[level], GetDamage(splice[level]), Lspli.animationTarget);
            Lspli.effects[2] = new Effect(Lspli.effects[1]._effect, 2, IntentType.Damage_1_2, Slots.SlotTarget(new int[] { -1, 0 }, false));
            Ability Rspli = Lspli.Duplicate();
            Rspli.name = R_Splice[level];
            Rspli.description = chance[level].ToString() + "% chance to inflict 2 Ruptured on the Right enemy. Deal " + splice[level].ToString() + " damage to the Right enemy. \nDeal 2 damage to the Right and Opposing party members.";
            Rspli.sprite = Icons[3];
            Rspli.animationTarget = Rslay.animationTarget;
            Rspli.effects[0]._target = Rspli.animationTarget;
            Rspli.effects[1]._target = Rspli.animationTarget;
            Rspli.effects[2]._target = Slots.SlotTarget(new int[] {0, 1}, false);
            Ability that = new Ability();
            that.name = "Don't Do That!";
            that.description = "Deal " + dont[level] + " indirect damage to the Left and Right enemies. Deal 4 damage to the Opposing party member.";
            that.sprite = Icons[4];
            that.rarity = 0;
            that.cost = new ManaColorSO[] { Pigments.SplitPigment(Pigments.Purple, Pigments.Blue) };
            that.animationTarget = MultiTargetting.Create(Slots.Sides, Slots.Front);
            that.visuals = Lslay.visuals;
            that.effects = new Effect[2];
            DamageEffect indirect = ScriptableObject.CreateInstance<DamageEffect>();
            indirect._indirect = true;
            that.effects[0] = new Effect(indirect, dont[level], GetDamage(dont[level]), Slots.Sides);
            that.effects[1] = new Effect(ScriptableObject.CreateInstance<DamageEffect>(), 4, IntentType.Damage_3_6, Slots.Front);

            enemy.abilities = new Ability[] { Lspli, that, Rspli, Lslay, Rslay };

            enemy.AddEnemy();
        }
        public static IntentType GetDamage(int num)
        {
            num = Math.Abs(num);
            if (num <= 2) return IntentType.Damage_1_2;
            else if (num <= 6) return IntentType.Damage_3_6;
            else if (num <= 10) return IntentType.Damage_7_10;
            else if (num <= 15) return IntentType.Damage_11_15;
            else if (num <= 20) return IntentType.Damage_16_20;
            else return IntentType.Damage_21;
        }
        static EffectSO[] _moveSide;
        public static EffectSO[] MoveSide
        {
            get
            {
                if (_moveSide == null)
                {
                    _moveSide = new EffectSO[2];
                    _moveSide[0] = EZEffects.GoSide<SwapToOneSideEffect>(false);
                    _moveSide[1] = EZEffects.GoSide<SwapToOneSideEffect>(true);
                }
                return _moveSide;
            }
        }

        public static void L1() => Add(0);
        public static void L2() => Add(1);
        public static void L3() => Add(2);
        public static void L4() => Add(3);
    }
    public static class TheCharacter
    {
        static Sprite[] _icons;
        public static Sprite[] Icons
        {
            get
            {
                if (_icons == null || _icons.Length <= 0)
                {
                    _icons = new Sprite[7];
                    _icons[0] = ResourceLoader.LoadSprite("CharlieFront.png");
                    _icons[1] = ResourceLoader.LoadSprite("CharlieBack.png");
                    _icons[2] = ResourceLoader.LoadSprite("CharlieWorld.png", 32, new Vector2(0.5f, 0.0f));
                    _icons[3] = ResourceLoader.LoadSprite("CharlieMenu.png");
                    _icons[4] = ResourceLoader.LoadSprite("Charlie_icon.png");
                    _icons[5] = ResourceLoader.LoadSprite("DitchedFront.png");
                    _icons[6] = ResourceLoader.LoadSprite("DitchedBack.png");
                }
                return _icons;
            }
        }
        public static Sprite[] A_Icons => TheEnemy.Icons;
        static ExtraAbilityInfo[][] _control;
        public static ExtraSpriteType spriteType => (ExtraSpriteType)220018;
        public static ExtraSpriteType normType => (ExtraSpriteType)230018;
        public static ExtraAbilityInfo[][] Control
        {
            get
            {
                if (_control == null)
                {
                    _control = new ExtraAbilityInfo[4][];
                    AddCharlieEnemyAbilityFrontEffect ls = ScriptableObject.CreateInstance<AddCharlieEnemyAbilityFrontEffect>();
                    ls.ability = TheEnemy.L_Slay[0];
                    AddCharlieEnemyAbilityFrontEffect lp = ScriptableObject.CreateInstance<AddCharlieEnemyAbilityFrontEffect>();
                    lp.ability = TheEnemy.L_Splice[0];
                    AddCharlieEnemyAbilityFrontEffect rs = ScriptableObject.CreateInstance<AddCharlieEnemyAbilityFrontEffect>();
                    rs.ability = TheEnemy.R_Slay[0];
                    AddCharlieEnemyAbilityFrontEffect rp = ScriptableObject.CreateInstance<AddCharlieEnemyAbilityFrontEffect>();
                    rp.ability = TheEnemy.R_Splice[0];
                    BaseCombatTargettingSO t = TheEnemy.Allies;

                    RestoreSwapUseEffect s = ScriptableObject.CreateInstance<RestoreSwapUseEffect>();
                    RefreshAbilityUseEffect a = ScriptableObject.CreateInstance<RefreshAbilityUseEffect>();

                    Ability re = new Ability();
                    re.name = "Re-Disguise";
                    re.description = "Make Charlie slip back into the disguise on the party member side. Refresh movement. 30% chance to refresh abilities.";
                    re.sprite = ResourceLoader.LoadSprite("Re-disguise_icon.png");
                    re.cost = new ManaColorSO[] { Pigments.Yellow };
                    re.visuals = null;
                    re.animationTarget = Slots.Self;
                    new CustomIntentInfo("fleeting", (IntentType)2240112, Passives.Fleeting.passiveIcon, IntentType.Misc);
                    re.effects = new Effect[6];
                    re.effects[0] = new Effect(ScriptableObject.CreateInstance<ResetCasterAbilitiesToDefaultEffect>(), 1, IntentType.Misc, Slots.Self);
                    SetCasterExtraSpritesEffect c = ScriptableObject.CreateInstance<SetCasterExtraSpritesEffect>();
                    c._spriteType = normType;
                    re.effects[1] = new Effect(ScriptableObject.CreateInstance<ResetCasterPassivesToDefaultEffect>(), 1, null, Slots.Self);
                    re.effects[2] = new Effect(c, 1, null, Slots.Self);
                    re.effects[3] = new Effect(ScriptableObject.CreateInstance<FleeFirstInLineEffect>(), 1, CustomIntentIconSystem.GetIntent("fleeting"), t, ScriptableObject.CreateInstance<FleeExtraCharliesCondition>());
                    re.effects[4] = new Effect(s, 1, null, Slots.Self);
                    re.effects[5] = new Effect(a, 1, null, Slots.Self, Conditions.Chance(30));

                    Ability first = new Ability();
                    first.name = TheEnemy.L_Slay[0];
                    first.description = "Make Charlie perform the titular ability. 40% chance to refresh this party member's abilities.";
                    first.sprite = A_Icons[0];
                    first.cost = new ManaColorSO[] { Pigments.SplitPigment(Pigments.Red, Pigments.Purple) };
                    first.visuals = null;
                    first.animationTarget = Slots.Self;
                    new CustomIntentInfo("Lslay", (IntentType)431103, A_Icons[0], IntentType.Misc);
                    first.effects = new Effect[2];
                    first.effects[0] = new Effect(ls, 1, CustomIntentIconSystem.GetIntent("Lslay"), t);
                    first.effects[1] = new Effect(a, 1, IntentType.Misc, Slots.Self, Conditions.Chance(40));
                    Ability sec = new Ability();
                    sec.name = TheEnemy.R_Slay[0];
                    sec.description = first.description;
                    sec.sprite = A_Icons[2];
                    sec.cost = first.cost;
                    sec.visuals = null;
                    sec.animationTarget = Slots.Self;
                    new CustomIntentInfo("Rslay", (IntentType)445879, A_Icons[2], IntentType.Misc);
                    sec.effects = new Effect[2];
                    sec.effects[0] = new Effect(rs, 1, CustomIntentIconSystem.GetIntent("Rslay"), t);
                    sec.effects[1] = first.effects[1];
                    Ability third = new Ability();
                    third.name = TheEnemy.L_Splice[0];
                    third.description = "Make Charlie Perform the titular ability. 30% chance to refresh this party member's abilities.";
                    third.sprite = A_Icons[1];
                    third.cost = new ManaColorSO[] { Pigments.Red };
                    third.visuals = null;
                    third.animationTarget = Slots.Self;
                    new CustomIntentInfo("Lspli", (IntentType)7710669, A_Icons[1], IntentType.Misc);
                    third.effects = new Effect[2];
                    third.effects[0] = new Effect(lp, 1, CustomIntentIconSystem.GetIntent("Lspli"), t);
                    third.effects[1] = new Effect(a, 1, IntentType.Misc, Slots.Self, Conditions.Chance(30));
                    Ability fourth = third.Duplicate();
                    fourth.name = TheEnemy.R_Splice[0];
                    fourth.sprite = A_Icons[3];
                    new CustomIntentInfo("Rspli", (IntentType)9863251, A_Icons[3], IntentType.Misc);
                    fourth.effects[0] = new Effect(rp, 1, CustomIntentIconSystem.GetIntent("Rspli"), t);

                    _control[0] = new ExtraAbilityInfo[]
                    {
                        new ExtraAbilityInfo(third.CharacterAbility()),
                        new ExtraAbilityInfo(first.CharacterAbility()),
                        new ExtraAbilityInfo(re.CharacterAbility()),
                        new ExtraAbilityInfo(sec.CharacterAbility()),
                        new ExtraAbilityInfo(fourth.CharacterAbility())
                    };

                    string m = "Make Charlie perform the titular ability. ";
                    re.description = "Make Charlie slip back into the disguise on the party member side. Refresh movement. 35% chance to refresh abilities.";
                    re.effects[5]._condition = Conditions.Chance(35);
                    first.name = TheEnemy.L_Slay[1];
                    first.description = m + "50% chance to refresh this party member's abilities.";
                    first.effects[1]._condition = Conditions.Chance(50);
                    sec.name = TheEnemy.R_Slay[1];
                    sec.description = first.description;
                    sec.effects[1]._condition = first.effects[1]._condition;
                    third.name = TheEnemy.L_Splice[1];
                    third.description = m + "40% chance to refresh this party member's abilities.";
                    third.effects[1]._condition = Conditions.Chance(40);
                    fourth.name = TheEnemy.R_Splice[1];
                    fourth.description = third.description;
                    fourth.effects[1]._condition = third.effects[1]._condition;
                    _control[1] = new ExtraAbilityInfo[]
                    {
                        new ExtraAbilityInfo(third.CharacterAbility()),
                        new ExtraAbilityInfo(first.CharacterAbility()),
                        new ExtraAbilityInfo(re.CharacterAbility()),
                        new ExtraAbilityInfo(sec.CharacterAbility()),
                        new ExtraAbilityInfo(fourth.CharacterAbility())
                    };

                    re.description = "Make Charlie slip back into the disguise on the party member side. Refresh movement. 40% chance to refresh abilities.";
                    re.effects[5]._condition = Conditions.Chance(40);
                    first.name = TheEnemy.L_Slay[2];
                    first.description = m + "60% chance to refresh this party member's abilities.";
                    first.effects[1]._condition = Conditions.Chance(60);
                    sec.name = TheEnemy.R_Slay[2];
                    sec.description = first.description;
                    sec.effects[1]._condition = first.effects[1]._condition;
                    third.name = TheEnemy.L_Splice[2];
                    third.description = m + "50% chance to refresh this party member's abilities.";
                    third.effects[1]._condition = Conditions.Chance(50);
                    fourth.name = TheEnemy.R_Splice[2];
                    fourth.description = third.description;
                    fourth.effects[1]._condition = third.effects[1]._condition;
                    _control[2] = new ExtraAbilityInfo[]
                    {
                        new ExtraAbilityInfo(third.CharacterAbility()),
                        new ExtraAbilityInfo(first.CharacterAbility()),
                        new ExtraAbilityInfo(re.CharacterAbility()),
                        new ExtraAbilityInfo(sec.CharacterAbility()),
                        new ExtraAbilityInfo(fourth.CharacterAbility())
                    };

                    re.description = "Make Charlie slip back into the disguise on the party member side. Refresh movement. 45% chance to refresh abilities.";
                    re.effects[5]._condition = Conditions.Chance(45);
                    first.name = TheEnemy.L_Slay[3];
                    first.description = m + "66% chance to refresh this party member's abilities.";
                    first.effects[1]._condition = Conditions.Chance(66);
                    sec.name = TheEnemy.R_Slay[3];
                    sec.description = first.description;
                    sec.effects[1]._condition = first.effects[1]._condition;
                    third.name = TheEnemy.L_Splice[3];
                    third.description = m + "60% chance to refresh this party member's abilities.";
                    third.effects[1]._condition = Conditions.Chance(60);
                    fourth.name = TheEnemy.R_Splice[3];
                    fourth.description = third.description;
                    fourth.effects[1]._condition = third.effects[1]._condition;
                    _control[3] = new ExtraAbilityInfo[]
                    {
                        new ExtraAbilityInfo(third.CharacterAbility()),
                        new ExtraAbilityInfo(first.CharacterAbility()),
                        new ExtraAbilityInfo(re.CharacterAbility()),
                        new ExtraAbilityInfo(sec.CharacterAbility()),
                        new ExtraAbilityInfo(fourth.CharacterAbility())
                    };
                }
                return _control;
            }
        }
        public static void Add()
        {
            int g = Control.Length;
            Character charlie = new Character();
            charlie.name = "Charlie";
            charlie.entityID = (EntityIDs)3344683;
            charlie.healthColor = Pigments.Purple;
            charlie.usesBaseAbility = true;

            Ability slap = new Ability();
            slap.name = "Melee";
            slap.description = "This party member ditches the cloak and enters the enemy side of the field for melee combat. \nRefresh this party member's abilities.";
            slap.sprite = ResourceLoader.LoadSprite("Melee_icon.png");
            slap.visuals = null;
            slap.animationTarget = Slots.Self;
            slap.cost = new ManaColorSO[] { Pigments.Purple };
            slap.effects = new Effect[5];
            SwapCasterPassivesEffect p = ScriptableObject.CreateInstance<SwapCasterPassivesEffect>();
            p._passivesToSwap = new BasePassiveAbilitySO[] { TheEnemy.Ally, Passives.Inanimate };
            SetCasterExtraSpritesEffect c = ScriptableObject.CreateInstance<SetCasterExtraSpritesEffect>();
            c._spriteType = spriteType;
            slap.effects[0] = new Effect(ScriptableObject.CreateInstance<SpawnCharlieEffect>(), 1, IntentType.Other_Spawn, Slots.Self);
            slap.effects[1] = new Effect(p, 1, null, Slots.Self);
            slap.effects[2] = new Effect(ScriptableObject.CreateInstance<SwapCharlieAbilitiesEffect>(), 1, null, Slots.Self);
            slap.effects[4] = new Effect(ScriptableObject.CreateInstance<RefreshAbilityUseEffect>(), 1, IntentType.Misc, Slots.Self);
            slap.effects[3] = new Effect(c, 1, null, Slots.Self);

            charlie.baseAbility = slap;
            charlie.usesAllAbilities = false;
            charlie.passives = new BasePassiveAbilitySO[0];
            charlie.levels = new CharacterRankedData[4];
            charlie.frontSprite = Icons[0];
            charlie.backSprite = Icons[1];
            charlie.overworldSprite = Icons[2];
            charlie.lockedSprite = Icons[3];
            charlie.unlockedSprite = Icons[3];

            ExtraCCSprites_BasicSO e = ScriptableObject.CreateInstance<ExtraCCSprites_BasicSO>();
            e._useDefault = normType;
            e._useSpecial = spriteType;
            e._frontSprite = Icons[5];
            e._backSprite = Icons[6];

            charlie.extraSprites = e;
            charlie.walksInOverworld = true;
            charlie.isSecret = false;
            charlie.menuChar = true;
            charlie.isSupport = true;
            charlie.ignoredAbilities = new List<int>() { 0 };
            charlie.hurtSound = LoadedAssetsHandler.GetCharcater("Bimini_CH").damageSound;
            charlie.deathSound = LoadedAssetsHandler.GetCharcater("Bimini_CH").deathSound;
            charlie.dialogueSound = LoadedAssetsHandler.GetCharcater("Bimini_CH").dxSound;

            Ability stab0 = new Ability();
            stab0.name = "Hesitant Stab";
            stab0.description = "Deal 6 damage to the Opposing enemy. Move the Opposing enemy to the Left or Right.";
            stab0.sprite = ResourceLoader.LoadSprite("Stab_icon.png");
            stab0.cost = new ManaColorSO[] { Pigments.Red, Pigments.Red };
            stab0.visuals = LoadedAssetsHandler.GetCharacterAbility("Expire_1_A").visuals;
            stab0.animationTarget = Slots.Front;
            stab0.effects = new Effect[2];
            stab0.effects[0] = new Effect(ScriptableObject.CreateInstance<DamageEffect>(), 6, IntentType.Damage_3_6, Slots.Front);
            stab0.effects[1] = new Effect(ScriptableObject.CreateInstance<SwapToSidesEffect>(), 1, IntentType.Swap_Sides, Slots.Front);
            Ability prep0 = new Ability();
            prep0.name = "Fumbling Preparation";
            prep0.description = "Apply 5 Shield to this party member's position. Heal this party member 1 health.";
            prep0.sprite = ResourceLoader.LoadSprite("Preparation_icon.png");
            prep0.cost = new ManaColorSO[] { Pigments.Blue };
            prep0.visuals = LoadedAssetsHandler.GetCharacterAbility("Entrenched_1_A").visuals;
            prep0.animationTarget = Slots.Self;
            prep0.effects = new Effect[2];
            prep0.effects[0] = new Effect(ScriptableObject.CreateInstance<ApplyShieldSlotEffect>(), 5, IntentType.Field_Shield, Slots.Self);
            prep0.effects[1] = new Effect(ScriptableObject.CreateInstance<HealEffect>(), 1, IntentType.Heal_1_4, Slots.Self);
            Ability stick0 = new Ability();
            stick0.name = "Stick the Shoulder";
            stick0.description = "Inflict 2 Ruptured on the Opposing enemy. \nIf the Opposing enemy doesn't have Scars, inflict 1 Scar on them.";
            stick0.sprite = ResourceLoader.LoadSprite("Stick_icon.png");
            stick0.cost = new ManaColorSO[] { Pigments.Red };
            stick0.visuals = LoadedAssetsHandler.GetCharacterAbility("Shank_1_A").visuals;
            stick0.animationTarget = Slots.Front;
            stick0.effects = new Effect[2];
            stick0.effects[0] = new Effect(ScriptableObject.CreateInstance<ApplyRupturedEffect>(), 2, IntentType.Status_Ruptured, Slots.Front);
            stick0.effects[1] = new Effect(ScriptableObject.CreateInstance<ScarsIfNoScarsEffect>(), 1, IntentType.Status_Scars, Slots.Front);

            charlie.AddLevel(14, new Ability[] { stab0, prep0, stick0 }, 0);

            Ability stab1 = stab0.Duplicate();
            stab1.name = "Rushed Stab";
            stab1.description = "Deal 9 damage to the Opposing enemy. Move the Opposing enemy to the Left or Right.";
            stab1.effects[0]._entryVariable = 9;
            stab1.effects[0]._intent = IntentType.Damage_7_10;
            Ability prep1 = prep0.Duplicate();
            prep1.name = "Hasty Preparation";
            prep1.description = "Apply 10 Shield to this party member's position. Heal this party member 1 health.";
            prep1.effects[0]._entryVariable = 10;
            Ability stick1 = stick0.Duplicate();
            stick1.name = "Stick an Artery";
            stick1.description = "Inflict 3 Ruptured on the Opposing enemy. \nIf the Opposing enemy doesn't have Scars, inflict 1 Scar on them.";
            stick1.effects[0]._entryVariable = 3;
            charlie.AddLevel(17, new Ability[] { stab1, prep1, stick1 }, 1);

            Ability stab2 = stab1.Duplicate();
            stab2.name = "Confident Stab";
            stab2.description = "Deal 10 damage to the Opposing enemy. Move the Opposing enemy to the Left or Right.";
            stab2.effects[0]._entryVariable = 10;
            Ability prep2 = prep1.Duplicate();
            prep2.name = "Pre-Preparation";
            prep2.description = "Apply 12 Shield to this party member's position. Heal this party member 1 health.";
            prep2.effects[0]._entryVariable = 12;
            Ability stick2 = stick1.Duplicate();
            stick2.name = "Stick an Organ";
            charlie.AddLevel(19, new Ability[] { stab2, prep2, stick2 }, 2);

            Ability stab3 = stab2.Duplicate();
            stab3.name = "Deep Stab";
            stab3.description = "Deal 12 damage to the Opposing enemy. Move the Opposing enemy to the Left or Right.";
            stab3.effects[0]._entryVariable = 12;
            stab3.effects[0]._intent = IntentType.Damage_11_15;
            Ability prep3 = prep2.Duplicate();
            prep3.name = "Secured Preparation.";
            prep3.description = "Apply 13 Shield to this party member's position. Heal this party member 1 health.";
            prep3.effects[0]._entryVariable = 13;
            Ability stick3 = stick2.Duplicate();
            stick3.name = "Stick an Eyeball";
            stick3.description = "Inflict 4 Ruptured on the Opposing enemy. \nIf the Opposing enemy doesn't have Scars, inflict 2 Scars on them.";
            stick3.effects[0]._entryVariable = 4;
            stick3.effects[1]._entryVariable = 2;
            charlie.AddLevel(21, new Ability[] { stab3, prep3, stick3 }, 3);
            charlie.AddCharacter();

            BrutalAPI.BrutalAPI.selCharsSO._dpsCharacters.Add(new CharacterRefString(charlie.charData._characterName), new CharacterIgnoredAbilities
            {
                ignoredAbilities = new List<int>() { 1, 2 }
            });
        }
    }
}
