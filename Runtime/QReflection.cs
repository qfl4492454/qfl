using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace QTool.Reflection
{
    #region 类型反射

    public class QMemeberInfo : IKey<string>
    {
        public string Key { get => Name; set => Name=value; }
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public Action<object, object> Set { get; private set; }
        public Func<object, object> Get { get; private set; }
        public Attribute Attribute { get; set; }
        public MemberInfo MemeberInfo { get; private set; }
        public QMemeberInfo(FieldInfo info)
        {
            MemeberInfo = info;
            Name = info.Name;
            Type = info.FieldType;
            Set = info.SetValue;
            Get = info.GetValue;
        }
        public QMemeberInfo(PropertyInfo info)
        {
            MemeberInfo = info;
            Name = info.Name;
            Type = info.PropertyType;
            if (info.SetMethod != null)
            {
                Set = info.SetValue;
            }
            if (info.GetMethod != null)
            {
                Get = info.GetValue;
            }
        }
        public override string ToString()
        {
            return   "var " + Name+" \t\t("+ Type+")"  ;
        }
    }
    public class QFunctionInfo : IKey<string>
    {
        public string Key { get => Name; set => Name = value; }
        public string Name { get; private set; }
        public ParameterInfo[] ParamType { get; private set; }
        public Type ReturnType {
            get
            {
                return MethodInfo.ReturnType;
            }
        }
        public MethodInfo MethodInfo { get; private set; }
        public Func<object, object[], object> Function { get; private set; }
        public Attribute Attribute { get;  set; }
        public object Invoke(object target,params object[] param)
        {
            return Function?.Invoke(target,param);
        }
        public QFunctionInfo(MethodInfo info)
        {
            this.MethodInfo = info;
            Key = info.Name;
            ParamType = info.GetParameters();
            Function = info.Invoke;
        }
        public override string ToString()
        {
            return  "function " + Name + "(" + ParamType.ToOneString(",") + ") \t\t("+ ReturnType+")" ;
        }
    }
    public class QTypeInfo<T>:IKey<string> where T:QTypeInfo<T> ,new()
    {
        public string Key { get;  set; }
        public QList<string, QMemeberInfo> Members = new QList<string, QMemeberInfo>();
        public QList<string, QFunctionInfo> Functions = new QList<string, QFunctionInfo>();
        public bool IsList;
        public Type ElementType { get; private set; }
        public Type Type { get; private set; }
        public TypeCode Code { get; private set; }
        public BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public BindingFlags FunctionFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public bool IsArray {
            get
            {
                return Type.IsArray;
            }
        }
        public object Create(params object[] param)
        {
            return Activator.CreateInstance(Type, param);
        }
        public bool IsValueType
        {
            get
            {
                return Type.IsValueType;
            }
        }
        public int[] IndexArray { get; private set; }
        public int ArrayRank
        {
            get
            {
                if (IndexArray == null)
                {
                    return 0;
                }
                else
                {
                    return IndexArray.Length;
                }
            }
        }
        protected void CheckInit(Type type,Func<QMemeberInfo,bool> memeberCheck,Func<QFunctionInfo,bool> functionCheck)
        {
            Key = type.FullName;
            Type = type;
            Code = Type.GetTypeCode(type);
            if (TypeCode.Object.Equals(Code))
            {
                if (type.IsArray)
                {
                    ElementType = type.GetElementType();
                    IndexArray = new int[type.GetArrayRank()];
                }
                else if (type.GetInterface(typeof(IList<>).FullName, true) != null)
                {
                    ElementType = type.GetInterface(typeof(IList<>).FullName, true).GenericTypeArguments[0];
                    IsList = true;
                }
                if (Members != null)
                {
                    QMemeberInfo memeber=null;
                    type.ForeachMemeber((info) =>
                    {
                        memeber = new QMemeberInfo(info);
                        if (memeberCheck == null || memeberCheck(memeber))
                        {
                            Members.Add(memeber);
                        }
                    },
                    (info) =>
                    {
                        memeber = new QMemeberInfo(info);
                        if (memeberCheck==null||memeberCheck(memeber))
                        {
                            Members.Add(memeber);
                        }
                    }, MemberFlags);
                }

                if (Functions != null)
                {
                    type.ForeachFunction((info) =>
                    {
                        var function = new QFunctionInfo(info);
                        if (functionCheck==null|| functionCheck(function))
                        {
                            Functions.Add(function);
                        }
                    }, FunctionFlags);
                }
            }
        }
        protected virtual void Init(Type type)
        {
            CheckInit(type,null,null);
        }
        static Type[] defaultCreatePrams = new Type[0];
        public static Dictionary<Type, T> table = new Dictionary<Type, T>();
        public static T Get(Type type)
        {
            if (!table.ContainsKey(type))
            {
                var info = new T();
                info.Init(type);
                table.Add(type, info);
            }
            return table[type];
        }
        public override string ToString()
        {
            return "Type " + Key + " \n{\n\t" + Members.ToOneString("\n\t") + "\n\t" + Functions.ToOneString("\n\t") + "}";
        }
    }

    #endregion
    public static class QReflection
    {
        public static string ViewName(this MemberInfo type)
        {
            var att = type.GetCustomAttribute<ViewNameAttribute>();
            if (att != null && att.name != "")
            {
                return att.name;
            }
            else
            {
                return type.Name;
            }
        }
        public static string ViewName(this ParameterInfo info)
        {
            var att = info.GetCustomAttribute<ViewNameAttribute>();
            if (att != null && att.name != "")
            {
                return att.name;
            }
            else
            {
                return info.Name;
            }
        }
        public static Assembly[] GetAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
        public static MethodInfo GetStaticMethod(this Type type, string name)
        {
            while (type.BaseType != null)
            {
                var funcInfo = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (funcInfo != null)
                {
                    return funcInfo;
                }
                else
                {
                    type = type.BaseType;
                }
            }
            return null;
        }
        public static void InvokeStaticFunction(this Type type,string name,params object[] param)
        {
            GetStaticMethod(type, name)?.Invoke(null, param);
        }
        public static List<Type> GetAllTypes(this Type rootType)
        {
            List<Type> typeList = new List<Type>();
            foreach (var ass in GetAllAssemblies())
            {
                typeList.AddRange(ass.GetTypes());
            }
            typeList.RemoveAll((type) =>
            {
                var baseType = type.BaseType;
                while (baseType != null && !type.IsAbstract)
                {
                    if (baseType.Name == rootType.Name)
                    {
                        return false;
                    }
                    else
                    {
                        baseType = baseType.BaseType;
                    }
                }
                return true;
            });
            return typeList;
        }
        static Dictionary<string, Type> typeDic = new Dictionary<string, Type>();
        public static Type ParseType(string typeString)
        {
            if (typeDic.ContainsKey(typeString))
            {
                return typeDic[typeString];
            }
            else
            {
                Type type = null;
                Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
                int assemblyArrayLength = assemblyArray.Length;
                for (int i = 0; i < assemblyArrayLength; ++i)
                {
                    type = assemblyArray[i].GetType(typeString);
                    if (type != null)
                    {
                        typeDic.Add(typeString, type);
                        return type;
                    }
                    
                }
                for (int i = 0; i < assemblyArrayLength; ++i)
                {
                    foreach (var eType in assemblyArray[i].GetTypes())
                    {
                        if (eType.Name.Equals(typeString))
                        {
                            type = eType;
                            if (type != null)
                            {
                                typeDic.Add(typeString, type);
                                return type;
                            }
                        }
                    }
                }
            }
            typeDic.Add(typeString, null);
            Debug.LogError("类型[" + typeString + "]未解析成功");
            return null;

        }
        public static void ForeachMemeber(this Type type, Action<FieldInfo> fieldInfo, Action<PropertyInfo> propertyInfo = null, BindingFlags bindingFlags= BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo[] fields = type.GetFields(bindingFlags);
            foreach (var item in fields)
            {
                fieldInfo?.Invoke(item);
            }
            var infos = type.GetProperties(bindingFlags);
            foreach (var item in infos)
            {
                propertyInfo?.Invoke(item);
            }
        }
        public static void ForeachFunction(this Type type, Action<MethodInfo> methodeInfo, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var methods= type.GetMethods(bindingFlags);
            foreach (var method in methods)
            {
                methodeInfo?.Invoke(method);
            }
        }
    }
}
