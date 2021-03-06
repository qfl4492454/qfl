using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace QTool.Asset
{
#if Addressables
    using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.AddressableAssets;
    using UnityEditor;
    using System.IO;

    public static  class AddressableTool
    {
        public static QDictionary<string, List<AddressableAssetEntry>> labelDic = new QDictionary<string, List<AddressableAssetEntry>>();
        public static QDictionary<string, AddressableAssetGroup> groupDic = new QDictionary<string, AddressableAssetGroup>();
        public static QDictionary<string, AddressableAssetEntry> entryDic = new QDictionary<string, AddressableAssetEntry>();
        public static void SetAddresableGroup(string assetPath, string groupName, string key = "")
        {
            var group = GetGroup(groupName);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = entryDic.ContainsKey(guid) ? entryDic[guid] : AssetSetting.FindAssetEntry(guid);
          
            if (entry == null)
            {
                entry = AssetSetting.CreateOrMoveEntry(guid, group);
                if (entry == null)
                {
                    Debug.LogError("生成资源【" + key + "】出错：" + assetPath);
                    return;
                }
            }
            else if (entry.parentGroup != group)
            {
                AssetSetting.MoveEntry(entry, group);
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                entry.address = Path.GetFileNameWithoutExtension(assetPath);
            }
            else if (entry.address != key)
            {
                entry.address = key;
            }
            if (!entry.labels.Contains(groupName))
            {
             //   entry.labels.Clear();
                entry.SetLabel(groupName, true, true);
            }
            EditorUtility.SetDirty(AssetSetting);
            EditorUtility.SetDirty(group);
        }
       

        public static AddressableAssetSettings AssetSetting
        {
            get
            {
                return AddressableAssetSettingsDefaultObject.Settings;
            }
        }
        public static List<AddressableAssetEntry> GetLabelList(string label)
        {
            if (labelDic[label] == null)
            {
                labelDic[label] = new List<AddressableAssetEntry>();
            }
            if (AssetSetting != null)
            {

                labelDic[label].Clear();
                foreach (var group in AssetSetting.groups)
                {
                    if (group == null) continue;
                    foreach (var item in group.entries)
                    {
                        if (item.labels.Contains(label))
                        {
                            labelDic[label].Add(item);
                        }
                    }
                }
            }
            return labelDic[label];
      
        }
        public static AddressableAssetGroup GetGroup(string groupName)
        {
            var group = groupDic[groupName];
            if (group == null)
            {
                group = AssetSetting.FindGroup(groupName);
                if (group == null)
                {
                    group = AssetSetting.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema>
                    {AssetSetting.DefaultGroup.Schemas[0],AssetSetting.DefaultGroup.Schemas[1] }, typeof(System.Data.SchemaType));
                }
                else
                {
                    foreach (var e in group.entries)
                    {
                        entryDic[e.guid] = e;
                    }
                }
            }
            return group;
        }
    }
#endif

#endif
    public abstract class AssetList<TObj> : AssetList<TObj, TObj> where TObj: UnityEngine.Object  { 
        
    }

    public abstract class AssetList<TLabel,TObj> where TObj:UnityEngine.Object 
    {
        public static QDictionary<string, TObj> objDic = new QDictionary<string, TObj>();
        public static string Label
        {
            get
            {
                return typeof(TLabel).Name;
            }
        }
        public static void Clear()
        {
            _loadOver = false;
#if Addressables
            loaderTask = null;
#endif
            objDic.Clear();
            QToolDebug.Log(()=>"清空ResourceList<" + Label+">");
        }
       
        public static bool ContainsKey(string key)
        {
            return objDic.ContainsKey(key);
        }
        public static void Set(TObj obj, string key = "", bool checkQid = true)
        {
            if (obj == null) return;
            var setKey = key;
            if (string.IsNullOrEmpty(setKey))
            {
                setKey = obj.name;
            }
            objDic[setKey] = obj;
            QToolDebug.Log(()=>"资源缓存[" + setKey + "]:" + obj);
            if (checkQid)
            {
                if (obj is GameObject)
                {
                    var qid = (obj as GameObject).GetComponentInChildren<QId>();
                    if (qid != null && !string.IsNullOrWhiteSpace(qid.PrefabId) && qid.PrefabId != key)
                    {
                        Set(obj, qid.PrefabId, false);
                    }
                }
            }
        }
        public static TObj Get(string key)
        {
            if (!_loadOver)
            {
                _ = LoadAllAsync();
            }
            if (objDic.ContainsKey(key))
            {
                return objDic[key];
            }
            else if(_loadOver)
            {
                Debug.LogError("不存在资源" + Label + '\\' + key);
            }
            else
            {
                Debug.LogError("未初始化" + Label);
            }
            return null;
        }
        public static async Task<TObj> GetAsync(string key)
        {
            if (objDic.ContainsKey(key))
            {
                return objDic[key];
            }
            else
            {
#if Addressables
                return await AddressableGetAsync(key);
#else
                return ResourceGet(key);
#endif
            }
        }
        public static bool LoadOver => _loadOver;
        static bool _loadOver = false;
#if Addressables
        static bool _loading = false;
#endif
        static DateTime startTime;
        public static async Task LoadAllAsync()
        {
          
            if (_loadOver) return;
            startTime = DateTime.Now;
            ResourceLoadAll();
            await Task.Yield();
#if Addressables
           if (_loading)
            {
                await QTool.Tool.Wait(() => !_loading);
            }
            else
            {
                _loading = true;
                await AddressableLoadAll();
                _loading = false;
            }
#endif
            _loadOver = true;
            Debug.Log("[" + typeof(TLabel).Name + "]资源加载完成：" + (DateTime.Now - startTime).TotalMilliseconds+"ms\n"+objDic.ToOneString());
        }



#if Addressables
#region Addressable加载

        static async Task<TObj> AddressableGetAsync(string key)
        {
            LoadAllAsync();
            if (objDic.ContainsKey(key))
            {
                return objDic[key];
            }
            else
            {
                if (Application.isPlaying)
                {
                    var loader = Addressables.LoadAssetAsync<TObj>(key);
                    var obj = await loader.Task;
                    if(loader.Status== UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {

                        Set(obj, key);
                    }
                    else
                    {
                        if (loader.OperationException != null)
                        {
                            Debug.LogError("异步加载" + Label + "资源[" + key + "]出错" + loader.OperationException);
                        }
                    }
                    return obj;

                }
                else
                {
                    await LoadAllAsync();
                    if (objDic.ContainsKey(key))
                    {
                        return objDic[key];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        static Task loaderTask;
        static async Task AddressableLoadAll()
        {
            if (loaderTask != null)
            {
                await loaderTask;
                return;
            }

            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                if (AddressableTool.AssetSetting == null)
                {
                    Debug.LogError("未创建Addressable文件");
                    return;
                }
#endif
                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<TObj>> loader = default;
                try
                {
                    loader = Addressables.LoadAssetsAsync<TObj>(Label, null);
                }
                catch (Exception e)
                {
                    Debug.LogWarning( "Addressables不存在Lable[" +Label+"]:"+e);
                    return;
                }
                loaderTask = loader.Task;
                var obj = await loader.Task;
                loaderTask = null;
                if (loader.OperationException != null)
                {
                    Debug.LogError( "加载资源表[" + Label + "]出错" + loader.OperationException);
                }
                else if(loader.Task.Exception!=null)
                {
                    Debug.LogError("加载资源表[" + Label + "]出错" + loader.Task.Exception);
                }
                else
                {
                    foreach (var result in loader.Result)
                    {
                        Set(result);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                try
                {


                    var list = AddressableTool.GetLabelList(Label);
                    foreach (var entry in list)
                    {

                        if (entry.TargetAsset is TObj)
                        {
                            Set(entry.TargetAsset as TObj, entry.address);
                        }
                        else if (typeof(TObj) == typeof(Sprite))
                        {
                            if (entry.TargetAsset is Texture2D)
                            {
                                Set(AssetDatabase.LoadAssetAtPath<Sprite>(entry.AssetPath) as TObj, entry.address);
                            }
                        }
                    }
                    Debug.Log("[" + Label + "]加载完成\n" + objDic.ToOneString());
                    _loadOver = true;
                }
                catch (Exception e)
                {
                    Debug.LogError("[" + Label + "]加载出错"+e);
                }
#endif
            }
        }


#endregion
#endif
#region Resource加载

        static TObj ResourceGet(string key)
        {
            _ = LoadAllAsync();
            if (objDic.ContainsKey(key)) {
                return objDic[key];
            }
            else
            {
                Debug.LogError("不存在资源 Resources\\" + Label + '\\' + key);
                return null;
            }
        }
        static void ResourceLoadAll()
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                Application.dataPath.ForeachAllDirectoryWith("\\Resources", (rootPath) =>
                 {
                    
                     if (System.IO.Directory.Exists(rootPath + '\\' + Label))
                     {
                       
                        (rootPath + '\\' + Label).ForeachDirectoryFiles((loadPath) =>
                        {
                            Set(UnityEditor.AssetDatabase.LoadAssetAtPath<TObj>(loadPath.Substring(loadPath.IndexOf("Assets"))));
                        });
                     }
                 });
#endif
            }
            else
            {
                foreach (var obj in Resources.LoadAll<TObj>(Label))
                {
                    Set(obj);
                }
            }
            _loadOver = true;
        }
#endregion
    }
    public abstract class PrefabAssetList<TLabel>: AssetList<TLabel,GameObject> where TLabel:PrefabAssetList<TLabel>
    {
        static Dictionary<string, ObjectPool<GameObject>> PoolDic = new Dictionary<string, ObjectPool<GameObject>>();
        static async Task<ObjectPool<GameObject>> GetPool(string key)
        {
            var poolkey = key + "_AssetList";
            if (!PoolDic.ContainsKey(poolkey))
            {
                var prefab =await GetAsync(key) as GameObject;
                if (!PoolDic.ContainsKey(poolkey))
                {
                    if (prefab == null)
                    {
                        Debug.LogError(Label + "找不到预制体资源" + key);
                        PoolDic.Add(poolkey, null);
                    }
                    else
                    {
                        var pool = QPoolManager.GetPool(poolkey, prefab);
                        if (!PoolDic.ContainsKey(poolkey))
                        {
                            PoolDic.Add(poolkey, pool);
                        }
                    }
                }
            }
            return PoolDic[poolkey];
        }
      
        public static async Task<GameObject> GetInstance(string key, Vector3 position,Quaternion rotation,Transform parent = null)
        {
            var obj =await GetInstance(key, parent);
            obj.transform.position = position;
            obj.transform.localRotation = rotation;
            return obj;
        }
        public static async void Push(string key,GameObject obj)
        {
            if (key.Contains(" "))
            {
                key = key.Substring(0, key.IndexOf(" "));
            }
            (await GetPool(key))?.Push(obj);
        }
        public static void Push(GameObject obj)
        {
            Push(obj.name, obj);
        }
        public static void Push(List<GameObject> objList)
        {
            foreach (var obj in objList)
            {
                Push(obj);
            }
            objList.Clear();
        }
        public static async Task<GameObject> GetInstance(string key, Transform parent = null)
        {
            var pool = await GetPool(key);
            if (pool == null)
            {
                Debug.LogError("无法实例化预制体[" + key + "]");
                return null;
            }
            var obj = pool.Get();
            if (obj == null)
            {
                return null;
            }
            if (parent != null)
            {
                obj.transform.SetParent(parent,false);
            }
            if (obj.transform is RectTransform)
            {
                var prefab = await GetAsync(key);
                (obj.transform as RectTransform).anchoredPosition = (prefab.transform as RectTransform).anchoredPosition;
            }
            obj.name = key;
            return obj;
        }
        public async static Task<CT> GetInstance<CT>(string key, Transform parent = null) where CT : Component
        {
            var obj =await GetInstance(key, parent);
            if (obj == null)
            {
                return null;
            }
            return obj.GetComponent<CT>();
        }
        public async static Task<CT> GetInstance<CT>(string key, Vector3 pos, Quaternion rotation, Transform parent = null) where CT : Component
        {
            var obj =await GetInstance(key, pos, rotation, parent);
            if (obj == null)
            {
                return null;
            }
            return obj.GetComponent<CT>();
        }
    }
}


