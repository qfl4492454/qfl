﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QTool;
using System.Text;
using System;
using QTool.Binary;
using System.Reflection;
using QTool.Inspector;
using System.Runtime.Serialization.Formatters.Binary;
using QTool.Reflection;
using System.Threading.Tasks;
using FixMath.NET;
using BEPUutilities;

namespace QTool.Test
{
    [Flags]
    public enum TestEnum
    {
        无 = 0,
        攻击 = 1 << 1,
        防御 = 1 << 2,
        死亡 = 1 << 3,
    }
    [System.Serializable]
    public struct V2
    {
        public float x;
        public float y;
        public static bool operator ==(V2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(V2 a, Vector2 b)
        {
            return a.x != b.x || a.y != b.y;
        }
        public V2(Vector2 vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }
      //  [JsonIgnore]
        public Vector2 Vector2
        {
            get
            {
                return new Vector2(x, y);
            }
        }
    }
    [System.Serializable]
    public class NetInput : PoolObject<NetInput>
    {
        public bool NetStay = false;
        public V2 NetVector2;

        public override void OnPoolRecover()
        {
        }

        public override void OnPoolReset()
        {
        }
    }

    //[QType()]
    [System.Serializable]
    public class TestClass//:IQSerialize
    {
        public TestEnum testEnume = TestEnum.攻击 | TestEnum.死亡;

        public List<float> list;
        public string asdl;
        public float p2;
        public TestClass2 child;
        public void Read(QBinaryReader read)
        {
            list = read.ReadObject(list);
            p2 = read.ReadSingle();
            // list = read.ReadObject<List<float>>();
            asdl = read.ReadString();
        }

        public void Write(QBinaryWriter write)
        {
            write.WriteObject(list);
            write.Write(p2);
            write.Write(asdl);
        }
        public TestClass()
        {

        }
        public TestClass(int a)
        {

        }
    }

    //[QType()]
    [System.Serializable]
    public class TestClass2//:IQSerialize
    {
        public List<float> list;
        public string asdl;
        public float p1;

        public void Read(QBinaryReader read)
        {
            list = read.ReadObject(list);
            p1 = read.ReadSingle();
            // list = read.ReadObject<List<float>>();
            asdl = read.ReadString();
        }

        public void Write(QBinaryWriter write)
        {
            write.WriteObject(list);
            write.Write(p1);
            write.Write(asdl);
        }
    }
    public interface ITest
    {

    }
    public class T1 : ITest
    {
        public string a;
    }
    [ScriptToggle("scriptList")]
    public class QToolTest : MonoBehaviour
    {
        public Fix64 fiexd1;
        public FixVector2 fixed2;
        public QDictionary<string, string> qDcitionaryTest = new QDictionary<string, string>();
        public static List<string> scriptList=> new List<string> { "QId" };
        //[ViewToggle("开关")]
        public bool toggle;
        // Start is called before the first frame update
        void Start()
        {
            //qDcitionaryTest["123"] = "123";
            //qDcitionaryTest["456"] = "456";
            //var a= qDcitionaryTest["789"];
            //qDcitionaryTest["456"] = "789";
            //var writer = new BinaryWriter().Write(new Vector3(9,8,7)).Write(v3);
            //var reader = new BinaryReader().Reset(writer.ToArray());
            // Debug.LogError(reader.ReadVector3()+":"+ reader.ReadVector3());
        }
    
        // Update is called once per frame
        void Update()
        {

        }
        [ReadOnly]
        [ViewName(name ="索引"  )]
        public int index = 0;

        public void AsyncTest()
        {

        }
        public class QTestTypeInfo : QTypeInfo<QTestTypeInfo>
        {

        }
        // public TestClass a = new TestClass { };
        // public Dictionary<string, float> aDic = new Dictionary<string, float>();
        // public Dictionary<string, float> bDic = new Dictionary<string, float>();
        public int[] b;

        public Vector3 v3 = new Vector3();
        public byte[] info;

        [ViewButton("ScreenSize",control = "togle")]
        public void SetSize()
        {
            QScreen.SetResolution(920, 630, false);
        }
        [ContextMenu("Name")]
        public void FullName()
        {
            UnityEngine.Debug.LogError(typeof(TestClass).Name);
        }

        public byte[] testBytes;

      //   [HorizontalGroup("t1", "toggle")]
        public TestClass test1;
     //   [HorizontalGroup("t1", "toggle")]
        public TestClass test2;
        public byte[] xmlBytes;
        public byte[] jsonBytes;
        public TestClass creatObj;
        NetInput last;
        public string email;
        public string emailPassword;
        public string testInfo;
        public string toAddress;
        [ContextMenu("测试邮件")]
        public void EmailTest()
        {
            FileManager.Save("test.txt", testInfo);
            MailTool.Send(email, emailPassword, "测试用户", "测试邮件", testInfo, toAddress,"test.txt");
        }
        [ContextMenu("对象池测试")]
        public void PoolTest()
        {
            last = NetInput.Get();
            last = NetInput.Get();
            last.Recover();
        }
        [ContextMenu("解析类型测试")]
        public void CreateTest()
        {
            UnityEngine.Debug.LogError(QTestTypeInfo.Get(typeof(List<string>)));
            var run = Assembly.GetExecutingAssembly();
            Tool.RunTimeCheck("系统创建", () =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    run.CreateInstance("TestClass");
                }
            });
            var ar = new object[0];
            Tool.RunTimeCheck("QInstance创建", () =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    Activator.CreateInstance(QReflection.ParseType("TestClass"));
                }
            });
            creatObj = (TestClass)Activator.CreateInstance(QReflection.ParseType("TestClass"));
            UnityEngine.Debug.LogError(creatObj);
        }
        public void GetTable(string startStr, Func<double, double> sinFunc)
        {
            var str = startStr;
            for (double i = -180; i <= 180; i++)
            {
                var value = sinFunc(2 * Math.PI * i / 360);
                str +=value.ToString("f4") + " , ";
            }
            Debug.LogError(str);
        }
        public void SinTest(string startStr,Func<float, float> sinFunc,Func<float, float> asinFunc)
        {
            var str = startStr;
            for (double i = -180; i <= 180; i++)
            {
                var value = sinFunc((float)(2 * Math.PI * i / 360));
               // asinFunc(value).ToString("f4");
                str +=asinFunc(value).ToString("f4")+":"+  value.ToString("f4") + " , ";
            }
            Debug.LogError(str);
        }
        public byte[] scenebytes;
        [ContextMenu("保存场景")]
        public void SaveAll()
        {
          
            scenebytes = QId.SaveAllInstance();
        }
        [ContextMenu("读取场景")]
        public void LoadAll()
        {
            QId.LoadAllInstance(scenebytes);
        }
        [ContextMenu("输出三角函数值")]
        public void SinTabFunc()
        {
           // Task.Run(() =>
           // {
                //GetTable("SinTable:", Math.Sin);
                //GetTable("CosTable:", Math.Cos);
                //GetTable("TanTable:", Math.Tan);
                //SinTest("Sin:", Math.Sin, Math.Asin);
                //SinTest("Cos:", Math.Cos, Math.Acos);
                //SinTest("Tan:", Math.Tan, Math.Atan);
                //SinTest("FixedSin:", (a) => Fix64.Sin(a).ToFloat(), (a) => Fix64.Asin(a).ToFloat());
                //SinTest("FixedCos:", (a) => Fix64.Cos(a).ToFloat(), (a) => Fix64.Acos(a).ToFloat());
                //SinTest("FixedTan:", (a) => Fix64.Tan(a).ToFloat(), (a) => Fix64.Atan(a).ToFloat());
           // });
        }
        [ContextMenu("写入Test")]
        public void TestFunc()
        {

            // list = new List<IValueBase>();
            //  list.Add(new IntValue { value = 4654 });

            Tool.RunTimeCheck("Xml写入", () =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    xmlBytes = FileManager.XmlSerialize(test1).GetBytes();
                   // if (i == 0) FileManager.Save(Application.dataPath + "/Test/xmlTest.xml", FileManager.Serialize(test1));
                //info = QSerialize.Serialize(al);
                //bl = QSerialize.Deserialize <List<IValueBase>>(info);
            }
            }, () => xmlBytes.Length);
            //Tool.RunTimeCheck("Json写入", () =>
            //{
            //    for (int i = 0; i < 10000; i++)
            //    {
            //       jsonBytes = FileManager.JsonSerialize(test1).GetBytes();
            //    //info = QSerialize.Serialize(al);
            //    //bl = QSerialize.Deserialize <List<IValueBase>>(info);
            //}
            //}, () => jsonBytes.Length);
            Tool.RunTimeCheck("QSerialize写入", () =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    testBytes = QSerialize.Serialize(test1);
                //info = QSerialize.Serialize(al);
                //bl = QSerialize.Deserialize <List<IValueBase>>(info);
            }
            }, () => testBytes.Length);
            //   Debug.LogError((QSerialize.Deserialize<T1>(QSerialize.Serialize(new T1 { a = "1124436" })) as T1).a);
            // Debug.LogError((bl[0] as IntValue).value);
            //  Debug.LogError(al["a"].netInput.NetVector2 .x+ ":" +bl[0].netInput.NetVector2.x);

        }
        [ContextMenu("读取Test")]
        public void Test2Func()
        {
            //al = new List<IValueBase>();
            //al.Add(new IntValue { value = 153 });
            //al.Add(null);
            //   list = new List<IValueBase>();
            //  list.Add(new IntValue { value = 431 });

            Tool.RunTimeCheck("Xml读取", () =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    test2 = FileManager.XmlDeserialize<TestClass>(xmlBytes.GetString());
                //info = Encoding.UTF8.GetBytes(FileManager.Serialize(al, typeof(IntValue)));
                //bl = FileManager.Deserialize<List<IValueBase>>(Encoding.UTF8.GetString(info), typeof(IntValue));
            }
            });
            Tool.RunTimeCheck("Json读取", () =>
            {
                for (int i = 0; i < 10000; i++)
                {
                 //   test2 = JsonConvert.DeserializeObject<TestClass>(jsonBytes.GetString());
                //info = Encoding.UTF8.GetBytes(FileManager.Serialize(al, typeof(IntValue)));
                //bl = FileManager.Deserialize<List<IValueBase>>(Encoding.UTF8.GetString(info), typeof(IntValue));
            }
            });
            Tool.RunTimeCheck("QSerialize读取", () =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    test2 = QSerialize.Deserialize<TestClass>(testBytes);
                //info = Encoding.UTF8.GetBytes(FileManager.Serialize(al, typeof(IntValue)));
                //bl = FileManager.Deserialize<List<IValueBase>>(Encoding.UTF8.GetString(info), typeof(IntValue));
            }

            });
            // Debug.LogError((bl[0] as IntValue).value);
        }
        //[ContextMenu("test3")]
        //public void Test3Func()
        //{
        //    al = new KeyList<string, IValueBase>();
        //    al.Add(new IValueBase { Key = "a" });
        //    al["a"].netInput.NetVector2 = new V2(Vector2.one * 123);
        //    TimeCheck("Json序列化", () =>
        //    {
        //        for (int i = 0; i < 1000; i++)
        //        {
        //            info = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(al));
        //            bl = JsonConvert.DeserializeObject<KeyList<string, IValueBase>>(Encoding.UTF8.GetString(info));
        //        }
        //    });
        //}
    }
}