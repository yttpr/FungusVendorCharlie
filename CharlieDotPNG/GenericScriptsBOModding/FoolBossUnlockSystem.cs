using BrutalAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace CharlieDotPNG
{
    public static class FoolBossUnlockSystem
    {
        //when adding this script to your mod, make sure to 
        //a) call "Setup()" someone in your Awake function
        //b) change the ModID to something unique to your mod.
        //check out Example() at the bottom for info on how to use this script.
        //It'd be appreciated if you credit "Salt" should you decide to publish a mod utilizing this script.

        public const string ModID = "TempID";//CHANGE THIS

        public static Dictionary<EntityIDs, FoolItemPairs> FoolsList;

        public static void MassUpdateAchievements()
        {
            foreach (FoolItemPairs pair in FoolsList.Values)
            {
                pair.Update();
            }
        }

        public static class AchievementSystem
        {
            public static Dictionary<Achievement, AchieveInfo> AchievementList;
            public static void Initialize()
            {
                if (AchievementList == null)
                {
                    AchievementList = new Dictionary<Achievement, AchieveInfo>();
                }
            }

            public static bool TryGetAchievement(EntityIDs ID, BossType Boss, out AchieveInfo Info)
            {
                if (AchievementList != null)
                {
                    foreach (AchieveInfo value in AchievementList.Values)
                    {
                        if (value.Character == ID && value.Boss == Boss)
                        {
                            Info = value;
                            return true;
                        }
                    }
                }
                Info = null;
                return false;
            }

            public class AchieveInfo
            {
                public Achievement ID;
                public AchievementUnlockType List;
                public string Name;
                public string Description;
                public Sprite Icon;
                public Achievement_t Info;
                public bool IsSecret;
                public string SecretDesc;

                public EntityIDs Character;
                public BossType Boss;

                public bool Unlocked;

                public AchieveInfo(Achievement id, AchievementUnlockType type, string name, string description, Sprite icon, bool secret = false, string secretDesc = "")
                {
                    Unlocked = false;
                    ID = id;
                    List = type;
                    Name = name;
                    Description = description;
                    Icon = icon;
                    IsSecret = secret;
                    SecretDesc = secretDesc;
                    Info = new Achievement_t(ID, Name, Description) { m_unlockedSprite = Icon };
                    if (IsSecret)
                    {
                        Info.m_isSecret = IsSecret;
                        Info.m_strSecretDesctription = SecretDesc;
                    }
                }

                public void Prepare(EntityIDs character, BossType boss)
                {
                    Character = character;
                    Boss = boss;
                    Initialize();
                    if (!AchievementList.Keys.Contains(ID))
                    {
                        if (!TryGetAchievement(character, boss, out var val))
                        {
                            AchievementList.Add(ID, this);
                            Debug.Log("Prepared achievement for " + Name);
                        }
                        else
                        {
                            Debug.Log("Achievement for character " + character.ToString() + " and boss type " + boss.ToString() + " already exists!");
                        }
                    }
                    else
                    {
                        Debug.Log("Achievement value for " + ID.ToString() + " already added, dont add it twice!");
                    }
                }

                public void SetValue(bool entry)
                {
                    Info.m_bAchieved = entry;
                    Info.m_offlinebAchieved = entry;
                    Unlocked = entry;
                }
            }

            public static Achievement[] GetAchievementList(Func<UnlockInformationDatabase, AchievementUnlockType, Achievement[]> orig, UnlockInformationDatabase self, AchievementUnlockType type)
            {
                Achievement[] old = orig(self, type);
                List<Achievement> ret = new List<Achievement>(old);
                foreach (Achievement key in AchievementList.Keys)
                {
                    if (AchievementList[key].List == type)
                    {
                        ret.Add(key);
                    }
                }
                return ret.ToArray();
            }

            public static Achievement_t GetAchievementInfo(Func<UnlockInformationDatabase, Achievement, Achievement_t> orig, UnlockInformationDatabase self, Achievement achID)
            {
                if (AchievementList.TryGetValue(achID, out AchieveInfo value))
                {
                    return value.Info;
                }
                else return orig(self, achID);
            }

            public static int LowerBy = 0;

            public static void PopulateInformation(Action<UnlockedAchievementsUIHandler, IGameCheckData> orig, UnlockedAchievementsUIHandler self, IGameCheckData game)
            {
                LowerBy = 0;
                orig(self, game);
            }

            public static void TryInitializeUnlockableAchievements(Action<UnlockListUIPanel, int, IUnlockCalls, Sprite[]> orig, UnlockListUIPanel self, int listID, IUnlockCalls calls, Sprite[] achInfo)
            {
                Transform listZone = self.transform.GetChild(1);
                Transform text = self.transform.GetChild(0);
                Transform baseDupe = listZone.GetChild(0);

                int inc = 0;

                while (self._icons.Length < achInfo.Length)
                {
                    Transform newer = GameObject.Instantiate(baseDupe, listZone);

                    List<UnlockIconUILayout> addThese = new List<UnlockIconUILayout>(self._icons);
                    for (int i = 0; i < newer.childCount; i++)
                    {
                        Transform nextIcon = newer.GetChild(i);
                        addThese.Add(nextIcon.GetComponent<UnlockIconUILayout>());
                    }
                    inc += 150;
                    self._icons = addThese.ToArray();
                }
                RectTransform zone = listZone.GetComponent<RectTransform>();

                RectTransform panel = self.transform.GetComponent<RectTransform>();

                RectTransform tbox = text.GetComponent<RectTransform>();

                LayoutRebuilder.ForceRebuildLayoutImmediate(zone);

                Vector2 size = panel.sizeDelta;
                size.y += inc;
                panel.sizeDelta = size;

                Transform parent = self.transform.parent;
                ContentSizeFitter fitter = parent.GetComponent<ContentSizeFitter>();
                VerticalLayoutGroup layout = parent.GetComponent<VerticalLayoutGroup>();

                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());

                orig(self, listID, calls, achInfo);
            }


            public static void Setup()
            {
                IDetour list = new Hook(typeof(UnlockInformationDatabase).GetMethod(nameof(UnlockInformationDatabase.GetAchievementList), ~BindingFlags.Default), typeof(AchievementSystem).GetMethod(nameof(GetAchievementList), ~BindingFlags.Default));
                IDetour info = new Hook(typeof(UnlockInformationDatabase).GetMethod(nameof(UnlockInformationDatabase.GetAchievementInfo), ~BindingFlags.Default), typeof(AchievementSystem).GetMethod(nameof(GetAchievementInfo), ~BindingFlags.Default));
                IDetour game = new Hook(typeof(UnlockedAchievementsUIHandler).GetMethod(nameof(UnlockedAchievementsUIHandler.PopulateInformation), ~BindingFlags.Default), typeof(AchievementSystem).GetMethod(nameof(PopulateInformation), ~BindingFlags.Default));
                IDetour array = new Hook(typeof(UnlockListUIPanel).GetMethod(nameof(UnlockListUIPanel.TryInitializeUnlockableAchievements), ~BindingFlags.Default), typeof(AchievementSystem).GetMethod(nameof(TryInitializeUnlockableAchievements), ~BindingFlags.Default));
            }
        }

        public static void AddToFishPool(string ItemName, int probability)
        {
            LootItemProbability newFish = new LootItemProbability();
            if (LoadedAssetsHandler.LoadedWearables.Keys.Contains(ItemName))
            {
                newFish.itemName = ItemName;
                newFish.probability = probability;
            }
            else
            {
                Debug.Log("YOUR ITEM DOESNT EXIST MOTHERFUCKER!!!");
                return;
            }
            try
            {
                if (!fishSet) SetupFish();
                List<LootItemProbability> rod = new List<LootItemProbability>(rodFish._lootableItems);
                rod.Add(newFish);
                rodFish._lootableItems = rod.ToArray();
                if (rodFish != canFish)
                {
                    List<LootItemProbability> can = new List<LootItemProbability>(canFish._lootableItems);
                    can.Add(newFish);
                    canFish._lootableItems = can.ToArray();
                }
                if (catFish != rodFish && catFish != canFish)
                {
                    List<LootItemProbability> cat = new List<LootItemProbability>(catFish._lootableItems);
                    cat.Add(newFish);
                    catFish._lootableItems = cat.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
            }
        }

        public static PerformEffectWearable fishingRod;
        public static PerformEffectWearable wormsCan;
        public static PerformEffectWithConsumeEffectWearable catfish;
        public static ExtraLootListEffect rodFish;
        public static ExtraLootListEffect canFish;
        public static ExtraLootListEffect catFish;

        public static void SetupFish()
        {
            fishingRod = LoadedAssetsHandler.GetWearable("FishingRod_TW") as PerformEffectWearable;
            wormsCan = LoadedAssetsHandler.GetWearable("CanOfWorms_SW") as PerformEffectWearable;
            catfish = LoadedAssetsHandler.GetWearable("WelsCatfish_ExtraW") as PerformEffectWithConsumeEffectWearable;
            rodFish = fishingRod.effects[0].effect as ExtraLootListEffect;
            canFish = wormsCan.effects[0].effect as ExtraLootListEffect;
            catFish = catfish._consumptionEffects[0].effect as ExtraLootListEffect;
            fishSet = true;
        }
        static bool fishSet = false;
        public static void TrySetFish()
        {
            if (fishSet) return;
            SetupFish();
        }
        public class FoolItemPairs
        {
            public EntityIDs entity;
            public string fool;
            public Dictionary<BossType, BrutalAPI.Item> unlocks;
            public Dictionary<BossType, int> autoFishPool;
            public BrutalAPI.Character ApiChara
            {
                get
                {
                    if (_apiChara != null) return _apiChara;
                    else { Debug.Log("NULL FoolItemPairs API character: " + fool + " " + entity); return new BrutalAPI.Character(); }
                }
            }
            public CharacterSO CharaSO
            {
                get
                {
                    if (_charaSO != null) return _charaSO;
                    else
                    {
                        foreach (CharacterSO chara in LoadedAssetsHandler.LoadedCharacters.Values)
                        {
                            if (chara.characterEntityID == entity)
                            {
                                _charaSO = chara; return chara;
                            }
                        }
                        Debug.Log("NULL Fool ItemPairs CharacterSO: " + fool + " " + entity);
                        return ScriptableObject.CreateInstance<CharacterSO>();
                    }
                }
            }

            public BrutalAPI.Character _apiChara;
            public CharacterSO _charaSO;

            public FoolItemPairs(BrutalAPI.Character chara, BrutalAPI.Item heavenUnlock, BrutalAPI.Item osmanUnlock, int heavenFishPoolWeight = 0, int osmanFishPoolWeight = 0) : this(chara)
            {
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
                unlocks.Add(BossType.Heaven, heavenUnlock);
                if (heavenFishPoolWeight > 0) autoFishPool.Add(BossType.Heaven, heavenFishPoolWeight);
                unlocks.Add(BossType.OsmanSinnoks, osmanUnlock);
                if (osmanFishPoolWeight > 0) autoFishPool.Add(BossType.OsmanSinnoks, osmanFishPoolWeight);
            }

            public FoolItemPairs(CharacterSO chara, BrutalAPI.Item heavenUnlock, BrutalAPI.Item osmanUnlock, int heavenFishPoolWeight = 0, int osmanFishPoolWeight = 0) : this(chara)
            {
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
                unlocks.Add(BossType.Heaven, heavenUnlock);
                if (heavenFishPoolWeight > 0) autoFishPool.Add(BossType.Heaven, heavenFishPoolWeight);
                unlocks.Add(BossType.OsmanSinnoks, osmanUnlock);
                if (osmanFishPoolWeight > 0) autoFishPool.Add(BossType.OsmanSinnoks, osmanFishPoolWeight);
            }

            public FoolItemPairs(EntityIDs id, string foolName, BrutalAPI.Item heavenUnlock, BrutalAPI.Item osmanUnlock, int heavenFishPoolWeight = 0, int osmanFishPoolWeight = 0) : this(id, foolName)
            {
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
                unlocks.Add(BossType.Heaven, heavenUnlock);
                if (heavenFishPoolWeight > 0) autoFishPool.Add(BossType.Heaven, heavenFishPoolWeight);
                unlocks.Add(BossType.OsmanSinnoks, osmanUnlock);
                if (osmanFishPoolWeight > 0) autoFishPool.Add(BossType.OsmanSinnoks, osmanFishPoolWeight);
            }//it is better to use this method if you are adding unlocks for characters in mods other than this one. for if the CharacterSO ends up being null, it could fuck up the save file.

            public FoolItemPairs(EntityIDs id, string foolName, BossType boss, BrutalAPI.Item unlock, int fishPoolWeight = 0) : this(id, foolName)
            {
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
                unlocks.Add(boss, unlock);
                if (fishPoolWeight > 0) autoFishPool.Add(boss, fishPoolWeight);
            }//alternatively, use this one for things like unlocks for modded bosses, or if you only want to add unlocks for one boss.

            public FoolItemPairs(BrutalAPI.Character chara, BossType boss, BrutalAPI.Item unlock, int fishPoolWeight = 0) : this(chara)
            {
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
                unlocks.Add(boss, unlock);
                if (fishPoolWeight > 0) autoFishPool.Add(boss, fishPoolWeight);
            }

            public FoolItemPairs(CharacterSO chara, BossType boss, BrutalAPI.Item unlock, int fishPoolWeight = 0) : this(chara)
            {
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
                unlocks.Add(boss, unlock);
                if (fishPoolWeight > 0) autoFishPool.Add(boss, fishPoolWeight);
            }

            public FoolItemPairs(BrutalAPI.Character chara)
            {
                _apiChara = chara;
                entity = chara.entityID;
                fool = chara.name;
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
            }

            public FoolItemPairs(CharacterSO chara)
            {
                _charaSO = chara;
                entity = chara.characterEntityID;
                fool = chara._characterName;
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
            }

            public FoolItemPairs(EntityIDs id, string foolName)
            {
                entity = id;
                fool = foolName;
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
            }

            public void AddUnlock(BossType boss, BrutalAPI.Item unlock, int fishPoolWeight = 0)
            {
                if (unlocks == null) unlocks = new Dictionary<BossType, BrutalAPI.Item>();
                if (autoFishPool == null) autoFishPool = new Dictionary<BossType, int>();
                unlocks.Add(boss, unlock);
                if (fishPoolWeight > 0) autoFishPool.Add(boss, fishPoolWeight);
            }//use this to add extra boss types to the unlock list, like modded bosses, for example.

            public void Add()
            {
                if (fool != null)
                {
                    fool = Regex.Replace(fool, "\\s+", "");
                    fool = RemoveSpecialChars(fool);
                }
                if (unlocks == null)
                {
                    Debug.Log("not set up yet?");
                    return;
                }
                if (FoolsList == null)
                {
                    FoolsList = new Dictionary<EntityIDs, FoolItemPairs>();
                }
                FoolsList.Add(entity, this);
                if (SaveConfigNames == null)
                {
                    SaveConfigNames = new Dictionary<string, bool>();
                }
                string l = SaveName;
                FileStream inStream = File.Open(SaveName, FileMode.Open);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load((Stream)inStream);
                foreach (BossType value in unlocks.Keys)
                {
                    string kug = fool + value.ToString() + GetItemSystemName(unlocks[value]);
                    bool add = false;
                    if (xmlDocument.GetElementsByTagName("config").Count > 0)
                    {
                        if (xmlDocument.GetElementsByTagName("config")[0].Attributes[kug] != null)
                        {
                            add = bool.Parse(xmlDocument.GetElementsByTagName("config")[0].Attributes[kug].Value);
                            if (add)
                            {
                                unlocks[value].AddItem();
                                if (autoFishPool.ContainsKey(value)) AddToFishPool(GetItemName(unlocks[value]), autoFishPool[value]);
                                if (AchievementSystem.TryGetAchievement(entity, value, out AchievementSystem.AchieveInfo info))
                                {
                                    info.SetValue(true);
                                }
                                
                            }
                        }
                        if (SaveConfigNames.Keys.Contains(kug)) SaveConfigNames[kug] = add; else SaveConfigNames.Add(kug, add);
                    }
                }
                inStream.Close();
            }

            public void Update()
            {
                FileStream inStream = File.Open(SaveName, FileMode.Open);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load((Stream)inStream);
                foreach (BossType value in unlocks.Keys)
                {
                    string kug = fool + value.ToString() + GetItemSystemName(unlocks[value]);
                    bool add = false;
                    if (xmlDocument.GetElementsByTagName("config").Count > 0)
                    {
                        if (xmlDocument.GetElementsByTagName("config")[0].Attributes[kug] != null)
                        {
                            add = bool.Parse(xmlDocument.GetElementsByTagName("config")[0].Attributes[kug].Value);
                            if (add)
                            {
                                if (AchievementSystem.TryGetAchievement(entity, value, out AchievementSystem.AchieveInfo info))
                                {
                                    info.SetValue(true);
                                }

                            }
                        }
                    }
                }
                inStream.Close();
            }
        }

        public static Dictionary<string, bool> SaveConfigNames;

        
        static string baseSave
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + "\\ItsTheMaceo\\BrutalOrchestra\\";
            }
        }
        static string pathPlus
        {
            get
            {
                if (!Directory.Exists(baseSave + "Mods\\"))
                {
                    Directory.CreateDirectory(baseSave + "Mods\\");
                }
                return baseSave + "Mods\\";
            }
        }
        public static string SavePath
        {
            get
            {
                if (!Directory.Exists(pathPlus + ModID + "\\"))
                {
                    Directory.CreateDirectory(pathPlus + ModID + "\\");
                }
                return pathPlus + ModID + "\\";
            }
        }
        public static string SaveName
        {
            get
            {
                if (!File.Exists(SavePath + ModID + ".config"))
                {
                    WriteConfig(SavePath + ModID + ".config");
                }
                return SavePath + ModID + ".config";
            }
        }
        
        public static void Setup()
        {
            try 
            {
                if (FoolsList == null)
                {
                    FoolsList = new Dictionary<EntityIDs, FoolItemPairs>();
                }
            }
            catch
            {
                Debug.LogError("failed to set up foolslist, ModID: " + ModID);
            }
            try
            {
                if (SaveConfigNames == null)
                {
                    SaveConfigNames = new Dictionary<string, bool>();
                }
            }
            catch
            {
                Debug.LogError("failed to set up main dict, ModID: " + ModID);
            }
            try { SetupFish(); }
            catch { Debug.LogError("failed to set up fish, ModID: " + ModID); }
            try { AchievementSystem.Initialize(); }
            catch { Debug.LogError("failed to initialize achievment system, ModID: " + ModID); }
            try { AchievementSystem.Setup(); }
            catch { Debug.LogError("failed to set p achievement system, ModID: " + ModID); }
            CleanedData = false;
            try { IDetour TryBeatBossIDetour = (IDetour)new Hook((MethodBase)typeof(UnlockablesManager).GetMethod(nameof(UnlockablesManager.TryBeatBossWith), ~BindingFlags.Default), typeof(FoolBossUnlockSystem).GetMethod(nameof(FoolBossUnlockSystem.TryBeatBossWith), ~BindingFlags.Default)); }
            catch { Debug.LogError("failed to set up try beat boss hook, ModID: " + ModID); }
            try { IDetour MainStartIDetour = (IDetour)new Hook((MethodBase)typeof(MainMenuController).GetMethod(nameof(MainMenuController.Start), ~BindingFlags.Default), typeof(FoolBossUnlockSystem).GetMethod(nameof(FoolBossUnlockSystem.Start), ~BindingFlags.Default)); }
            catch { Debug.LogError("failed to set up main menu hook, ModID: " + ModID); }
            try { IDetour CharaStarIDetour = (IDetour)new Hook((MethodBase)typeof(SelectableCharactersSO).GetMethod(nameof(SelectableCharactersSO.PrepareCharacters), ~BindingFlags.Default), typeof(FoolBossUnlockSystem).GetMethod(nameof(FoolBossUnlockSystem.PrepareCharacters), ~BindingFlags.Default)); }
            catch { Debug.LogError("failed to set up character selection hook, ModID: " + ModID); }
            try { LoadAllValues(); }
            catch { Debug.LogError("failed to load all save values, ModID: " + ModID); }
        }//call this somewhere in your awake

        public static void TryBeatBossWith(Action<UnlockablesManager, BossType, EntityIDs> orig, UnlockablesManager self, BossType boss, EntityIDs entity)
        {
            orig(self, boss, entity);
            Unlocks = self;
            if (!FoolsList.TryGetValue(entity, out var value))
            {
                return;
            }
            if (value.unlocks.TryGetValue(boss, out var item))
            {
                string str = value.fool + boss.ToString() + GetItemSystemName(item);
                if (SaveConfigNames.TryGetValue(str, out var unlocked) && unlocked) 
                {
                    return;
                }
                item.AddItem();
                BaseWearableSO ITE = LoadedAssetsHandler.GetWearable(GetItemName(item));
                ITE.usesTheOnUnlockText = false;
                if (value.autoFishPool.ContainsKey(boss))
                {
                    AddToFishPool(GetItemName(value.unlocks[boss]), value.autoFishPool[boss]);
                    BaseWearableSO fish = LoadedAssetsHandler.GetWearable(rodFish._lockedLootableItems[0].itemName);
                    ITE.hasSpecialUnlock = fish.hasSpecialUnlock;
                    ITE.unlockTextID = fish.unlockTextID;
                }
                Unlocks._freshlyAcquiredItems.Add(GetItemName(item));
                SaveManager.SaveFreshItemsSaveData(Unlocks._freshlyAcquiredItems);
                SaveConfigNames[str] = true;
                WriteConfig(SaveName);
            }
            value.Update();
        }

        public static AchievementGetterUIHandler Achievements;
        public static GameInformationHolder Info;
        public static UnlockablesManager Unlocks;

        public static void Start(Action<MainMenuController> orig, MainMenuController self)
        {
            MassUpdateAchievements();
            orig(self);
            Achievements = self._achievementGetterHandler;
            Info = self._informationHolder;
            Unlocks = Info.UnlockableManager;
            ClearUnlockPanels(Unlocks);
        }

        public static bool CleanedData;
        public static void ClearUnlockPanels(UnlockablesManager funlocks)
        {
            if (CleanedData)
                return;
            CleanedData = true;
            funlocks._freshlyAcquiredItems.Clear();
            SaveManager.SaveFreshItemsSaveData(funlocks._freshlyAcquiredItems);
        }

        public static string GetItemName(BrutalAPI.Item w)
        {
            string wName = "";
            if (w.itemPools.HasFlag(ItemPools.Treasure))
            {
                wName = Regex.Replace(w.name + "_TW", "\\s+", "");
            }
            else if (w.itemPools.HasFlag(ItemPools.Shop))
            {
                wName = Regex.Replace(w.name + "_SW", "\\s+", "");
            }
            else if (w.itemPools.HasFlag(ItemPools.Fish))
            {
                wName = Regex.Replace(w.name + "_FW", "\\s+", "");
            }
            else if (w.itemPools.HasFlag(ItemPools.Extra))
            {
                wName = Regex.Replace(w.name + "_EW", "\\s+", "");
            }
            else if (w.isShopItem)
            {
                wName = Regex.Replace(w.name + "_SW", "\\s+", "");
            }
            else if (!w.isShopItem)
            {
                wName = Regex.Replace(w.name + "_TW", "\\s+", "");
            }
            return wName;
        }

        public static char[] SpecialCharacters = new char[] { '.', '\'', '!', '?', '@', '#', '$', '%', '^', '&', '*', '(', ')', '=', '+', '<', '>', '/', '\\' };//update this for any special characters you wanna use in item names. the xml save system kinda breaks if you have characters other than letters and numbers.
        public static string GetItemSystemName(BrutalAPI.Item w)
        {
            string wName = GetItemName(w);
            return RemoveSpecialChars(wName);
        }
        public static string RemoveSpecialChars(string source)
        {
            StringBuilder str = new StringBuilder();
            foreach (char c in source)
            {
                if (!SpecialCharacters.Contains(c))
                {
                    str.Append(c);
                }
            }
            return str.ToString();
        }

        public static void WriteConfig(string location)
        {
            StreamWriter text = File.CreateText(location);
            XmlDocument xmlDocument = new XmlDocument();
            string xml = "<config";
            foreach(string key in SaveConfigNames.Keys)
            {
                xml += " ";
                xml += key;
                xml += "='";
                xml += SaveConfigNames[key].ToString().ToLower();
                xml += "'";
            }
            xml += "> </config>";
            xmlDocument.LoadXml(xml);
            xmlDocument.Save((TextWriter)text);
            text.Close();
        }

        public static void PrepareCharacters(Action<SelectableCharactersSO, HashSet<string>> orig, SelectableCharactersSO self, HashSet<string> unlockedCharacters)
        {
            orig(self, unlockedCharacters);
            foreach (SelectableCharacterData character in self._characters)
            {
                if (character.HasCharacter)
                {
                    if (FoolsList.TryGetValue(character.LoadedCharacter.characterEntityID, out FoolItemPairs box))
                    {
                        bool heaven = character.HasTheDivine;
                        bool osman = character.HasTheWitness;
                        if (box.unlocks.Keys.Contains(BossType.Heaven))
                        {
                            string kug = box.fool + BossType.Heaven.ToString() + GetItemSystemName(box.unlocks[BossType.Heaven]);
                            SaveConfigNames.TryGetValue(kug, out bool state);
                            heaven = state;
                        }
                        if (box.unlocks.Keys.Contains(BossType.OsmanSinnoks))
                        {
                            string kug = box.fool + BossType.OsmanSinnoks.ToString() + GetItemSystemName(box.unlocks[BossType.OsmanSinnoks]);
                            SaveConfigNames.TryGetValue(kug, out bool state);
                            osman = state;
                        }
                        character.SetAchievementState(osman, heaven);
                    }
                }
            }
            WriteConfig(SaveName);
        }

        public static void LoadAllValues()
        {
            FileStream inStream = File.Open(SaveName, FileMode.Open);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load((Stream)inStream);
            if (xmlDocument.GetElementsByTagName("config").Count > 0)
            {
                foreach (XmlAttribute node in xmlDocument.GetElementsByTagName("config")[0].Attributes)
                {
                    bool val = bool.Parse(node.Value);
                    string name = node.Name;
                    if (val != null & name.Length > 0)
                    {
                        if (SaveConfigNames.Keys.Contains(name))
                        {
                            SaveConfigNames[name] = val;
                        }
                        else
                        {
                            SaveConfigNames.Add(name, val);
                        }
                    }
                }
            }
            inStream.Close();
        }

        public static void Example()
        {
            //this is meant to be an example. do not call this void.
            Character chara = new Character();
            chara.entityID = (EntityIDs)865753;
            chara.levels = new CharacterRankedData[1];
            chara.AddLevel(10, new Ability[] { }, 0);
            //etc
            chara.AddCharacter();
            EffectItem heaven = new EffectItem();
            heaven.name = "weebol";
            //etc
            //DO NOT ADD THE ITEM
            EffectItem osman = new EffectItem();
            osman.name = "wibil";
            //etc
            //DO NOT ADD THE ITEM
            FoolItemPairs guhPair = new FoolItemPairs(chara, heaven, osman);//character, heaven unlock, osman unlock
            guhPair.Add();

            EffectItem fish = new EffectItem();
            fish.name = "waggerwol";
            fish.itemPools = ItemPools.Fish;
            //etc etc
            //do not add blah blah blah
            FoolItemPairs wawPair = new FoolItemPairs((EntityIDs)69999, "Greeble", BossType.Heaven, fish, 3);//entity id, character name, boss, item, fish-pool weight.
            //leaving the fish weight blank will make it not auto-add the item to the fish pool.
            wawPair.Add();
            //if you are adding unlocks for a character in a different mod, use the EntityIDs, string name method of adding unlocks for them.
            //if the LoadedAssetsHandler.GetCharcater for that fool ends up null, it may bug out the save file. so erm, dont do that
            //P.S. try to avoid naming items weirdly. i have no clue what will and wont fuck up the Xml writter.

            //EXAMPLE FOR ACHIEVEMENTS
            AchievementSystem.AchieveInfo info = new AchievementSystem.AchieveInfo((Achievement)77777, AchievementUnlockType.TheDivine, "Achievement Name", "Unlocked a new item.", ResourceLoader.LoadSprite("weebol", 32));
            info.Prepare((EntityIDs)69999, BossType.Heaven);
            //that's it. that's all of it.
            //note that this is only one achievement, you'd have to make a separate one for the osman panel.
        }
    }
}
