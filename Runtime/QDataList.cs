using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace QTool{
    
    public class QDataList: QAutoList<string, QDataRow>
    {
        public static QDataList QToolSetting => GetData(nameof(QToolSetting)+".qdata");
        public static string StreamingPathRoot => Application.streamingAssetsPath +'\\'+ nameof(QDataList)+'\\';
        static QDataList()
        {
            Application.focusChanged += (focus) =>
            {
                if (!focus)
                {
                    QToolSetting.Save();
                }
            };
        }
        public static QDataList GetData(string path)
        {
            if (!dataList.ContainsKey(path))
            {
                dataList[path] = new QDataList(FileManager.Load(path));
                dataList[path].LoadPath = path;
            }
            return dataList[path];
        }

        static QDictionary<string, QDataList> dataList = new QDictionary<string, QDataList>();
        public override void OnCreate(QDataRow obj)
        {
            obj.OwnerData = this;
            base.OnCreate(obj);
        }
        public string LoadPath { get; private set; }
        public void Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = LoadPath;
            }
            FileManager.Save(path, ToString());
        }
        public bool TryGetTitleIndex(string title,out int index)
        {
            index = -1;
            if (TitleLine != null)
            {
                index= TitleLine.IndexOf(title); 
            }
            return index >= 0;
        }
        public QDataRow TitleLine
        {
            get
            {
                if (Count > 0)
                {
                    return base[0];
                }
                return null;
            }
        }
        public new QDataRow this[int index]
        {
            get
            {
                if (index >= Count)
                {
                    var line=new QDataRow();
                    line.OwnerData = this;
                    base[index] = line;
                }
                return base[index];
            }
        }
       
        public QDataList(string dataStr)
        {
            var lineStrs = dataStr.Split('\n');
            foreach (var lineStr in lineStrs)
            {
                Add(new QDataRow(lineStr, this));
            }
        }
        public override string ToString()
        {
            return this.ToOneString("\n");
        }

    }

    public class QDataRow:QList<string>,IKey<string>
    {
        public string Key { get => base[0]; set
            {

                base[0] = value;
            }
        }
        public T GetValue<T>()
        {
            return Value.ParseQData<T>();
        }
        public void SetValue<T>(T value)
        {
            Value = value?.ToString();
        }
        public string Value
        {
            get
            {
                if (Count > 1)
                {
                    return base[1];
                }
                else
                {
                    return "";
                }
            }
            set
            {
                base[1] = value;
            }
        }
        public string this[string title]
        {
            get
            {
                if(OwnerData.TryGetTitleIndex(title,out var index))
                {
                    return this[index];
                }
                else
                {
                    throw new System.Exception("不存在的列名[" + title + "]");
                }
            }
            set
            {
                if (OwnerData.TryGetTitleIndex(title, out var index))
                {
                    this[index] = value;
                }
                else
                {
                    throw new System.Exception("不存在的列名[" + title + "]");
                }

            }
        }
        public QDataRow()
        {
        }
        public QDataList OwnerData { get; internal set; }
        public QDataRow(string lineStr,QDataList ownerData)
        {
            var valueS = lineStr.Split('\t');
            for (int i = 0; i < valueS.Length; i++)
            {
                var value = valueS[i];
                this[i] = value;
            }
            OwnerData = ownerData;
        }
        public override string ToString()
        {
            return this.ToOneString("\t");
        }
    }
}
