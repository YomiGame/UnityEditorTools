using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ShowObjsMesh : EditorWindow
{
    private List<GameObject> TargetGameObjectArray;
    private int verts;
    private int tris;
    [SerializeField]//必须要加
    protected List<GameObject> TargetObjectList = new List<GameObject>();
    //序列化对象
    protected SerializedObject _serializedObject;
    //序列化属性
    protected SerializedProperty _assetLstProperty;
    protected void OnEnable()
    {
        //使用当前类初始化
        _serializedObject = new SerializedObject(this);
        //获取当前类中可序列话的属性
        _assetLstProperty = _serializedObject.FindProperty("TargetObjectList");
    }
    [MenuItem("DesignTools/Model/ShowObjsMeshVT")]
    static void Init()
    {
        //弹出窗口
        EditorWindow.GetWindow(typeof(ShowObjsMesh));
    }

    void OnGUI()
    {
        
        if (GUI.Button(new Rect(20, 15, 120, 30), "ShowObjsVT"))
        {

            ShowMeshNum();
        }
    
        EditorGUILayout.Space(50);
        EditorGUILayout.LabelField("Objects to be tested(需要检测的物体)",EditorStyles.whiteLargeLabel);
        //更新
        _serializedObject.Update();
        //开始检查是否有修改
        EditorGUI.BeginChangeCheck();
        //显示属性
        //第二个参数必须为true，否则无法显示子节点即List内容
        EditorGUILayout.PropertyField(_assetLstProperty, true);
        TargetGameObjectArray = TargetObjectList;

        //结束检查是否有修改
        if (EditorGUI.EndChangeCheck())
        {
            //提交修改
            _serializedObject.ApplyModifiedProperties();
        }
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Verts(顶点数) >>> "+verts,EditorStyles.whiteLargeLabel);
        EditorGUILayout.LabelField("Tris（三角面数） >>> "+tris,EditorStyles.whiteLargeLabel);
        //Debug.Log();
    }


        
    


    
    

    private void ShowMeshNum()
    {
        tris = 0;
        verts = 0;
        foreach (GameObject obj in TargetGameObjectArray)
        {
            GetAllVertsAndTris(obj);
        }
    }
    private void GetAllVertsAndTris(GameObject obj)
    {
        Component[] filters;
        filters = obj.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter f in filters)
        {
            tris += f.sharedMesh.triangles.Length / 3;
            verts += f.sharedMesh.vertexCount;
        }
    }
    
}
