using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace RF5.HisaCat.BetterSorting
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    internal class BepInExLoader : BepInEx.IL2CPP.BasePlugin
    {
        public const string
            MODNAME = "BetterSorting",
            AUTHOR = "HisaCat",
            GUID = "RF5." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0";

        public static BepInEx.Logging.ManualLogSource log;

        public BepInExLoader()
        {
            log = Log;
        }

        public override void Load()
        {
            try
            {
                //Register components
                //ClassInjector.RegisterTypeInIl2Cpp<...>();
            }
            catch (System.Exception e)
            {
                BepInExLog.LogError($"Harmony - FAILED to Register Il2Cpp Types! {e}");
            }
            BepInExLog.LogInfo("[Harmony] RegisterTypeInIl2Cpp succeed.");

            try
            {
                //Patches
                Harmony.CreateAndPatchAll(typeof(SVPatcher));
                Harmony.CreateAndPatchAll(typeof(UIPatcher));
                Harmony.CreateAndPatchAll(typeof(ItemStorageatcher));
            }
            catch (System.Exception e)
            {
                BepInExLog.LogError($"Harmony - FAILED to Apply Patch's! {e}");
            }
            BepInExLog.LogInfo("[Harmony] Patch succeed.");
        }

        [HarmonyPatch]
        internal class ItemStorageatcher
        {
            private static Dictionary<ItemStorage, UISortBlock> cachedUISortBlockDic = new Dictionary<ItemStorage, UISortBlock>();

            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.SortItem))]
            [HarmonyPostfix]
            private static void SortItemPostfix(ItemStorage __instance)
            {
                BepInExLog.LogError($"SortItemPostfix");

                //Manual sort
                //BepInExLog.LogError($"- ItemDatas: {string.Join(",", __instance.ItemDatas.Select(x => x == null ? "NULL" : x.ItemID.ToString()))}");
                //BepInExLog.LogError($"- SortHead: {string.Join(",", __instance.SortHead.Select(x => x.ToString()))}");
                //BepInExLog.LogError($"- SortPriorityIndex: {string.Join(",", __instance.SortPriorityIndex.Select(x => x.ToString()))}");
                //BepInExLog.LogError($"- SortPriorityReverseVal: {string.Join(",", __instance.SortPriorityReverseVal.Select(x => x.ToString()))}");
                //BepInExLog.LogError($"- ItemDataLinkers.Index: {string.Join(",", __instance.ItemDataLinkers.Select(x => x.Index.ToString()))}");
                //BepInExLog.LogError($"- ItemDataLinkers.ItemID: {string.Join(",", __instance.ItemDataLinkers.Select(x => x.ItemData == null ? "NULL" : x.ItemData.ItemID.ToString()))}");

                //Calls when sort items:
                //DoSortItem
                //SortItem (Here)
                //SetSortBlockPostFix

                //Print items
                BepInExLog.LogError($"- ItemDatas: {string.Join(",", __instance.ItemDatas.Select(x => x == null ? "NULL" : x.ItemID.ToString()))}");
                BepInExLog.LogError($"- ItemDataLinkers.ItemID: {string.Join(",", __instance.ItemDataLinkers.Select(x => x.ItemData == null ? "NULL" : x.ItemData.ItemID.ToString()))}");

                //Sort wayas A
                //{
                //    //Sort ItemDatas only.
                //    //Idk how, but in this case, ItemDataLinkers's items order is automatically follows ItemDatas

                //    var existItems = __instance.ItemDatas.Where(x => x != null).ToArray();
                //    int existItemsCount = existItems.Length;

                //    //Do custom sort with linq
                //    existItems = existItems.OrderBy(x => x.Amount).ToArray();

                //    int count = __instance.ItemDatas.Count;
                //    for (int i = 0; i < existItemsCount; i++)
                //    {
                //        __instance.ItemDatas[i] = existItems[i];
                //    }
                //    for (int i = existItemsCount; i < count; i++)
                //    {
                //        __instance.ItemDatas[i] = null;
                //    }
                //}
                //Sort ways B:
                {
                    //Sort ItemDatas and ItemDataLinkers both via pair.

                    if (__instance.ItemDatas.Count != __instance.ItemDataLinkers.Count)
                    {
                        BepInExLog.LogError("ItemDatas and ItemDataLinkers count mismatched!");
                        return;
                    }

                    var count = __instance.ItemDatas.Count;
                    var itemsPair = new List<KeyValuePair<ItemData, ItemDataLinker>>();
                    for (int i = 0; i < count; i++)
                        itemsPair.Add(new KeyValuePair<ItemData, ItemDataLinker>(__instance.ItemDatas[i], __instance.ItemDataLinkers[i]));

                    var existItemsPair = itemsPair.Where(x => x.Key != null).ToArray();

                    //Do custom sort with linq
                    existItemsPair = existItemsPair.OrderBy(x => x.Key.Amount).ToArray();

                    var existItemsCount = existItemsPair.Length;
                    for (int i = 0; i < existItemsCount; i++)
                    {
                        __instance.ItemDatas[i] = existItemsPair[i].Key;
                        __instance.ItemDataLinkers[i] = existItemsPair[i].Value;
                    }
                    for (int i = existItemsCount; i < count; i++)
                    {
                        __instance.ItemDatas[i] = null;
                        __instance.ItemDataLinkers[i] = null;
                    }
                }

                //Print items after sorting
                BepInExLog.LogError($"- ItemDatas: {string.Join(",", __instance.ItemDatas.Select(x => x == null ? "NULL" : x.ItemID.ToString()))}");
                BepInExLog.LogError($"- ItemDataLinkers.ItemID: {string.Join(",", __instance.ItemDataLinkers.Select(x => x.ItemData == null ? "NULL" : x.ItemData.ItemID.ToString()))}");

                if (cachedUISortBlockDic.ContainsKey(__instance))
                {
                    if (cachedUISortBlockDic[__instance].SortText != null)
                        cachedUISortBlockDic[__instance].SortText.text = "MOD SORT";
                }

            }
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.DoSortItem))]
            [HarmonyPostfix]
            private static void DoSortItemPostfix(ItemStorage __instance)
            {
                BepInExLog.LogError($"DoSortItemPostfix");
            }

            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.SetSortBlock))]
            [HarmonyPostfix]
            private static void SetSortBlockPostfix(ItemStorage __instance, UISortBlock UISortBlock)
            {
                BepInExLog.LogError($"SetSortBlockPostfix\r\n" +
                    $"{UISortBlock.gameObject.FullName()}");

                if (cachedUISortBlockDic.ContainsKey(__instance))
                    cachedUISortBlockDic[__instance] = UISortBlock;
                else
                    cachedUISortBlockDic.Add(__instance, UISortBlock);

                var removed = cachedUISortBlockDic.Where(x => x.Key == null || x.Value == null || x.Value.gameObject == null).ToList();
                foreach (var rm in removed)
                {
                    if (cachedUISortBlockDic.ContainsKey(rm.Key))
                        cachedUISortBlockDic.Remove(rm.Key);
                }
            }
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.SetSubSortType))]
            [HarmonyPostfix]
            private static void SetSubSortTypePostfix(ItemStorage __instance, SubSortType type)
            {
                BepInExLog.LogError($"SetSubSortTypePostfix {type}");
            }
        }


        [HarmonyPatch]
        internal class SVPatcher
        {
            [HarmonyPatch(typeof(SV), nameof(SV.CreateUIRes))]
            [HarmonyPostfix]
            private static void CreateUIResPostfix(SV __instance)
            {
                //Localization.LocalizedString.CreateTemplate();
                //if (BundleLoader.MainBundle == null)
                //    BundleLoader.LoadBundle();
            }
        }

        [HarmonyPatch]
        internal class UIPatcher
        {
            [HarmonyPatch(typeof(CampMenuMain), nameof(CampMenuMain.Update))]
            [HarmonyPostfix]
            private static void UpdatePostfix(CampMenuMain __instance)
            {
                //FOR DEBUG
                //if (BepInEx.IL2CPP.UnityEngine.Input.GetKeyInt(BepInEx.IL2CPP.UnityEngine.KeyCode.F1) && UnityEngine.Event.current.type == UnityEngine.EventType.KeyDown)
                //{
                //    //ItemStorageManager.GetStorage(Define.StorageType.Rucksack).PushItemIn(ItemData.Instantiate(ItemID.Item_Rabunomidorinku, 1));
                //    foreach(var status in FriendMonsterManager.FriendMonsterStatusDatas)
                //    {
                //        status.LovePoint.Add(1000);
                //    }
                //    //TimeManager.Instance.ChangeTimeNextDay(12, 30);
                //    //ItemStorageManager.GetStorage(Define.StorageType.Rucksack).PushItemIn(ItemData.Instantiate(ItemID.Item_Tendon, 1));
                //    //ItemStorageManager.GetStorage(Define.StorageType.Rucksack).PushItemIn(ItemData.Instantiate(ItemID.Item_Yakionigiri, 1));
                //    //ItemStorageManager.GetStorage(Define.StorageType.Rucksack).PushItemIn(ItemData.Instantiate(ItemID.Item_Yakiimo, 1));
                //}
            }
        }
    }
}
