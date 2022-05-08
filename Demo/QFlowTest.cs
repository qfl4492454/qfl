using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QTool;
using QTool.Command;
using QTool.FlowGraph;
using QTool.Reflection;
using System.Threading.Tasks;
using QTool.Test;

public class QFlowTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    [ContextMenu("Test")]
    public void Test()
    {
        var c = QCommand.GetCommand(nameof(QFlowNodeTest.OutTest));
        QCommand.FreshCommands(typeof(QFlowNodeTest));
        var graph = new QFlowGraph();
        var a= graph.Add(nameof(QFlowNodeTest.LogErrorTest));
        var wait = graph.Add(nameof(QFlowNodeTest.CoroutineWaitTest));
        a["value"]="QState����";
        wait["time"]=3;
        a.Connect(wait);
        wait.Connect(a);
        StartCoroutine(graph.RunCoroutine());
     
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
[ViewName("QFlowNode����")]
public static class QFlowNodeTest
{
    static QFlowNodeTest()
    {
        QCommand.FreshCommands(typeof(QFlowNodeTest));
    }
    
    public static IEnumerator CoroutineWaitTest(float time)
    {
        yield return new WaitForSeconds(time);
    }
    public static async Task<string> TaskWaitReturnTest(int time,string strValue="wkejw")
    {
        await Task.Delay(time*1000);
        return strValue;
    }
    public static void LogErrorTest(string value="test1")
    {
        Debug.LogError(value);
    }
    public enum T1
    {
        E1,
        E2,
    }
  
    public static void EnumTest(TestEnum testEnum, T1 testEnum2,  out string value, string defaultTest1 = "1239180")
    {
        value = testEnum2.ToString();
        Debug.LogError(value + "  " + defaultTest1);
    }
    public static void OutTest([ViewName("����Bool")] bool inBool, [ViewName("���Bool")] out bool outBool, int inInt, out int outInt, float inFloat, out float outFloat)
    {
        outBool = inBool;
        outInt = inInt;
        outFloat = inFloat;
    }
    public static void ObjectTest(QObjectReference QObjectReference,Object _object,GameObject gameObject,Sprite sprite, Vector3 vector3)
    {

    }
    public static void ListTest(List<string> list, List<Vector3> v3List)
    {

    }
    public static void BoolTest(QFlowNode This,bool boolValue,[QFlowPort,QNodeOutput]bool True, [QFlowPort] out object False)
    {
        False = true;
        if (boolValue)
        {
            This.SetNetFlowPort(nameof(True));
        }
        else
        {
            This.SetNetFlowPort(nameof(False));
        }
    }
    public static void GetTime_AutoUseTest([QNodeOutput(true)]out float time)
    {
        time = Time.time;
    }
    public static float AddTest1(float a,float b)
    {
        return a + b;
    }
    public static void AddTest2(QFlowNode This, float a,float b,[QNodeOutput(true)] float result)
    {
        This[nameof(result)] = a + b;
    }
}
