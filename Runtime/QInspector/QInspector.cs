﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace QTool.Inspector
{
    /// <summary>
    /// 数值更改时调用changeCallBack函数
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ChangeCallAttribute : ViewNameAttribute
    {
        public string changeCallBack;
        public ChangeCallAttribute()
        {

        }
    }
    /// <summary>
    /// 使数据在inspector视窗不可更改
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public ReadOnlyAttribute()
        {
        }
    }
    /// <summary>
    /// 显示一个标题
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TitleAttribute : PropertyAttribute
    {
        public string title;
        public float height = 20;
        public TitleAttribute()
        {
            // order = 0;
        }
    }

    /// <summary>
    /// 更改显示的名字
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ViewNameAttribute : PropertyAttribute
    {
        public string name;
        public ViewNameAttribute()
        {
            //  order = 1;
        }
        public ViewNameAttribute(string name)
        {
            this.name = name;
        }
    }

    /// <summary>
    /// 将字符传显示为枚举下拉款通过GetKeyListFunc获取所有可选择的字符串
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ViewEnumAttribute : ViewNameAttribute
    {
        public string GetKeyListFunc;
        public bool CanWriteString = true;
        public ViewEnumAttribute()
        {
        }

    }

    /// <summary>
    /// 将数组显示为toolbar工具栏通过indexName来设置int值；
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ToolbarListAttribute : QEditorAttribute
    {
        public string GetListFunc;
        public float height = 40f;
        //  public bool ResourceImage = false;
        public ToolbarListAttribute(string GetListFunc)
        {
            this.GetListFunc = GetListFunc;
        }
    }

    /// <summary>
    /// 将数组显示为toolbar工具栏通过indexName来设置int值；
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ToggleListAttribute : QEditorAttribute
    {
        public string valueSetFunc;
        public string valueGetFunc;
        public float height = 40f;
    }
    /// <summary>
    /// 当在scene视窗鼠标事件调用 传入3个参数 (Vector3 点击位置,Collider 点击碰撞物体,bool 是否按下shift键)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SceneMouseEventAttribute : Attribute
    {
        public EventType eventType = EventType.MouseDown;
        public SceneMouseEventAttribute(EventType eventType)
        {
            this.eventType = eventType;
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class EditorChangeAttribute : Attribute
    {
        public EditorChangeAttribute()
        {
        }
    }
    /// <summary>
    /// inspector初始化时调用
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EidtorInitInvokeAttribute : Attribute
    {
        public EidtorInitInvokeAttribute()
        {
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ViewButtonAttribute : QEditorAttribute
    {
        public string name;
        public float height = 20;
        public ViewButtonAttribute()
        {
            // order = 0;
        }
    }
    /// <summary>
    /// 显示按钮 调用函数CallFunc 无参数
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ViewToggleAttribute : QEditorAttribute
    {
        public string name;
        public float height = 30;
        public ViewToggleAttribute()
        {
            // order = 0;
        }

    }


    public abstract class QEditorAttribute : PropertyAttribute
    {
        public string showControl = "";
    }
    /// <summary>
    /// 选取对象按钮显示 会调用函数CallFunc传入GameObject
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SelectObjectButtonAttribute : ViewButtonAttribute
    {

        public SelectObjectButtonAttribute()
        {
            // order = 0;
        }
    }
}