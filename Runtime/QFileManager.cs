using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

namespace QTool
{
    public static class FileManager
    {
        public static T Copy<T>(this T target)
        {
            return XmlDeserialize<T>(XmlSerialize(target));
        }
        public static string ToAssetPath(this string path)
        {
            return "Assets" + path.SplitEndString(Application.dataPath);
        }
        public static void ForeachDirectoryFiles(this string rootPath, Action<string> action)
        {
            ForeachFiles(rootPath, action);
            ForeachDirectory(rootPath, (path) =>
            {
                path.ForeachDirectoryFiles(action);
            });
        }
        public static int DirectoryFileCount(this string rootPath)
        {
            var count = rootPath.FileCount();
            rootPath.ForeachDirectory((path) =>
            {
                count += rootPath.FileCount();
            });
            return count;
        }
        public static int FileCount(this string rootPath)
        {
            return Directory.Exists(rootPath) ? Directory.GetFiles(rootPath).Length / 2 : 0;
        }
        public static void ForeachAllDirectoryWith(this string rootPath, string endsWith, Action<string> action)
        {
            rootPath.ForeachDirectory((path) =>
            {
                if (path.EndsWith(endsWith))
                {
                    action?.Invoke(path);
                }
                else
                {
                    path.ForeachAllDirectoryWith(endsWith, action);
                }
            });
        }
        public static void ForeachDirectory(this string rootPath, Action<string> action)
        {
            if (Directory.Exists(rootPath))
            {
                var paths = Directory.GetDirectories(rootPath);
                foreach (var path in paths)
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }
                    action?.Invoke(path);
                }
            }
            else
            {
                Debug.LogError("????" + " ????????????" + rootPath);
            }
        }
        public static void ForeachFiles(this string rootPath, Action<string> action)
        {
            if (Directory.Exists(rootPath))
            {
                var paths = Directory.GetFiles(rootPath);
                foreach (var path in paths)
                {
                    if (string.IsNullOrWhiteSpace(path) || path.EndsWith(".meta"))
                    {
                        continue;
                    }
                    action?.Invoke(path);
                }
            }
            else
            {
                Debug.LogError("????" + " ????????????" + rootPath);
            }
        }

        public static Dictionary<string, XmlSerializer> xmlSerDic = new Dictionary<string, XmlSerializer>();
        public static XmlSerializer GetSerializer(Type type, params Type[] extraTypes)
        {
            var key = type.FullName;
            foreach (var item in extraTypes)
            {
                key += " " + item.FullName;
            }
            if (xmlSerDic.ContainsKey(key))
            {
                return xmlSerDic[key];
            }
            else
            {
                XmlSerializer xz = new XmlSerializer(type, extraTypes);
                xmlSerDic.Add(key, xz);
                return xz;
            }
        }
        public static string XmlSerialize<T>(T t, params Type[] extraTypes)
        {
            using (StringWriter sw = new StringWriter())
            {
                if (t == null)
                {
                    Debug.LogError("??????????????" + typeof(T));
                    return null;
                }
                GetSerializer(typeof(T), extraTypes).Serialize(sw, t);
                return sw.ToString();
            }
        }
        public static T XmlDeserialize<T>(string s, params Type[] extraTypes)
        {
            using (StringReader sr = new StringReader(s))
            {
                try
                {
                    XmlSerializer xz = GetSerializer(typeof(T), extraTypes);
                    return (T)xz.Deserialize(sr);
                }
                catch (Exception e)
                {
                    Debug.LogError("Xml????????????\n" + e);
                    return default;
                }
            }
        }
        public static bool ExistsFile(string path)
        {
            return File.Exists(path);
        }
        public static string Load(string path)
        {
            string data = "";
            if (!ExistsFile(path))
            {
                Debug.LogError("????????????" + path);
                return data;
            }
            using (var file = System.IO.File.Open(path, System.IO.FileMode.Open))
            {
                using (var sw = new System.IO.StreamReader(file))
                {
                    data = sw.ReadToEnd();
                }
            }
            return data;
        }
        public static byte[] LoadBytes(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("????????????" + path);
                return null;
            }
            return File.ReadAllBytes(path);
        }
        public static void ClearData(this string path)
        {
            var directoryPath = GetFolderPath(path);
            Directory.Delete(directoryPath, true);
         
        }
        /// <summary>
        /// ??????????????
        /// </summary>
        public static string GetFolderPath(this string path)
        {
            return Path.GetDirectoryName(path);
        }
        public static void Save(string path, byte[] bytes)
        {
            CheckFolder(path);
            var directoryPath = GetFolderPath(path);
            if (!System.IO.Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllBytes(path, bytes);
        }
        public static void SavePng(Texture2D tex, string path)
        {
            var bytes = tex.EncodeToPNG();
            if (bytes != null)
            {
                File.WriteAllBytes(path, bytes);
            }
        }
        public static Texture2D LoadPng(string path, int width = 128, int height = 128)
        {
            var bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(width, height);
            tex.LoadImage(bytes);
            return tex;
        }
        public static string CheckFolder(this string path)
        {
            var directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !System.IO.Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return path;
        }
        public static void SaveXml<T>(string path, T data)
        {
            Save(path, XmlSerialize(data));
        }
        public static T LoadXml<T>(string path)
        {
            return XmlDeserialize<T>(Load(path));
        }
        public static string Save(string path, string data)
        {
            try
            {
                CheckFolder(path);
                using (var file = System.IO.File.Create(path))
                {
                    using (var sw = new System.IO.StreamWriter(file))
                    {
                        sw.Write(data);
                    }
                }
            }
            catch (Exception e)
            {

                Debug.LogError("??????????" + path + "??" + e);
            }

            return path;
        }

 

        public static string SelectOpenPath(string title = "????????", string extension = "obj", string directory = "Assets")
        {
            var dialog = new FileDialog
            {
                title = title,
                initialDir = directory,
                // defExt = extension,
                filter = "(." + extension + ")\0*." + extension + "\0",
            };
            if (FileDialog.GetOpenFileName(dialog))
            {
                return dialog.file;
            }
            return "";
        }
        public static string SelectSavePath(string title = "????????", string directory= "Assets", string defaultName="newfile", string extension = "obj" )
        {
            var dialog = new FileDialog
            {
                title = title,
                initialDir = directory,
                file=defaultName,
                //  defExt = extension,
                filter = "(." + extension + ")\0*." + extension + "\0",
            };
            if (FileDialog.GetSaveFileName(dialog))
            {
                if (dialog.file.EndsWith("." + extension))
                {
                    return dialog.file + "." + extension;
                }
                return  dialog.file;
            }
            return "";
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class FileDialog
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
        public FileDialog()
        {
            structSize = Marshal.SizeOf(this);
            file = new string(new char[256]);
            maxFile = file.Length;
            fileTitle = new string(new char[64]);
            maxFileTitle = fileTitle.Length;
            flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        }
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] FileDialog ofd);
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] FileDialog ofd);
    }
}