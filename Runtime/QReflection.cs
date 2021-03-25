using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace QTool.Reflection
{
    #region ���ͷ���

    public class QMemeberInfo : IKey<string>
    {
        public string Key { get => Name; set => value = Name; }
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public Action<object, object> Set { get; private set; }
        public Func<object, object> Get { get; private set; }
        public List<Attribute> Attributes { get; private set; } = new List<Attribute>();
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
            Set = info.SetValue;
            Get = info.GetValue;
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
        public List<Attribute> Attributes { get; private set; } = new List<Attribute>();
        public List<Type> ParamType { get; private set; } = new List<Type>();
        public Type ReturnType {
            get
            {
                return MethodInfo.ReturnType;
            }
        }
        public MethodInfo MethodInfo { get; private set; }
        public Func<object, object[], object> Function { get; private set; }
        public object Invoke(object target,params object[] param)
        {
            return Function?.Invoke(target,param);
        }
        public QFunctionInfo(MethodInfo info)
        {
            this.MethodInfo = info;
            Key = info.Name;
            var paramArray = info.GetParameters();
            Function = info.Invoke;
            foreach (var param in paramArray)
            {
                ParamType.Add(param.ParameterType);
            }
        }
        public override string ToString()
        {
            return  "function " + Name + "(" + ParamType.ToOneString(",") + ") \t\t("+ ReturnType+")" ;
        }
    }
    public class QTypeInfo<T>where T:QTypeInfo<T> ,new()
    {
        public string Name { get; private set; }
        public DicList<string, QMemeberInfo> Members = new DicList<string, QMemeberInfo>();
        public DicList<string, QFunctionInfo> Functions = new DicList<string, QFunctionInfo>();
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
        protected virtual void Init(Type type)
        {
            Name = type.Name;
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
                    type.ForeachMemeber((info) =>
                    {
                        Members.Add(new QMemeberInfo(info));
                    },
                    (info) =>
                     {
                         Members.Add(new QMemeberInfo(info));
                     }, MemberFlags);
                }

                if (Functions != null)
                {
                    type.ForeachFunction((info) =>
                    {
                        Functions.Add(new QFunctionInfo(info));
                    }, FunctionFlags);
                }
            }
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
            return "Type " + Name + " \n{\n\t" + Members.ToOneString("\n\t") + "\n\t" + Functions.ToOneString("\n\t") + "}";
        }
    }

    #endregion
    public static class QReflection
    {
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
            }
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