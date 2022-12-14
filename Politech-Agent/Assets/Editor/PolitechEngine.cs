using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class PolitechEngine : EditorWindow
{
    private GameObject HelperForBuild, Build;
    public string nameOfPrefab;

    [MenuItem("Politech 3D Engine/Окно редактора моделей")]
    static void EngineWindow()
    {
        EditorWindow.GetWindow(typeof(PolitechEngine));
    }

    void OnGUI()
    {
        GUILayout.Label("Редактор моделей", EditorStyles.boldLabel);

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        nameOfPrefab = EditorGUILayout.TextField("Имя префаба", nameOfPrefab);

        if (GUI.Button(new Rect(5, 70, 135, 20), "Изменить модель"))
        {
            if (GameObject.Find("Main_Build") != null)
            {
                GameObject.Find("Main_Build").GetComponent<NavigationBaker>().surfaces[0] = null;
                Destroy(GameObject.Find("Main_Build"));
            }

            HelperForBuild = GameObject.Instantiate(Resources.Load("Main_Build"), new Vector3(0,0,0), Quaternion.identity) as GameObject;
            HelperForBuild.name = "Main_Build";
            Build = GameObject.Instantiate(Resources.Load(nameOfPrefab), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            Build.transform.SetParent(HelperForBuild.transform);
            //////////GameObject.Find("Player").GetComponent<PlayerMovement>().p_pathAfterWalking.Clear();

        }
    }
}

