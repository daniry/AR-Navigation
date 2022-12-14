using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using static Core;

public class GameManager : MonoBehaviour
{
    public Dropdown[] dds;
    private string[] word;
    private string[] wordFloor;
    private int i = 0;
    private int b = 0;
   
    private QRTrackingHandler qrTrackingHandler;
    private PlayerMovement player;
    public Mapping p_Mapping;
    public UnityEvent OnDotsFound;
    public GameObject BuildingRoot;

    [HideInInspector]
    public GameObject MainBuild;
    [HideInInspector]
    public Transform[] b_Buildings;
    public Transform[] b_BS_floors;
    [HideInInspector]
    public Transform b_BS;
    public int currentBuilding;

    public Dropdown ddFloor;
    public Dropdown ddBuild;

    [HideInInspector]
	private RoomInfoLoader roomInfoLoader;
	//номера кабинетов на экране
	public TextMeshProUGUI numbersOfCabin;
 
    [Header("Canvas items")]
    public GameObject c_AR;
    public GameObject c_ToMap;
    public GameObject c_FindLocation;
    public GameObject c_Menu;
    public GameObject roomInfo;
    public GameObject c_TextSearch;
    public GameObject c_Question;
    public GameObject с_TurnOnBuild;
    public GameObject с_DifferentLoc;

    [Header("Camera items")]
    public GameObject cam_AR;
    public GameObject cam_ToMap;

    [Header("Misc cross project items")]
    public string currentLocation;
    public string destinationLocation;
    public string tempLocation; 

    public GameObject PersonaCanvas;
    public Image PersonaCanvas_Photo;
    public Text PersonaCanvas_Name;
    public Text PersonaCanvas_Work;
    public GameObject rendererOfPressedFloor;

    public void PersonalCanvas_INFO()
    {
        RoomInfo roomInfo = roomInfoLoader[currentLocation];

        if (roomInfo.Personel != "") { PersonaCanvas_Name.text = roomInfo.Personel; }
        else { PersonaCanvas_Name.text = roomInfo.ID; }
        if (roomInfo.Description != "") { PersonaCanvas_Work.text = roomInfo.Description; }
        else { PersonaCanvas_Work.text = roomInfo.Name; }
        if (Resources.Load("Faces/" + roomInfo.Personel) != null)
        { PersonaCanvas_Photo.sprite = Resources.Load<Sprite>("Faces/" + roomInfo.Personel); }
        else
        { PersonaCanvas_Photo.sprite = Resources.Load<Sprite>("Faces/mp_logo"); }
        PersonaCanvas.SetActive(true);
    }

    private void Awake()
    {       
        MainBuild = GameObject.Find("Main_Build");
        c_TextSearch.GetComponent<Text>().text = "Поиск аудитории";
        roomInfoLoader = new RoomInfoLoader("БС/BuildingA");
        b_Buildings = new Transform[MainBuild.transform.childCount];    
        GetChildRecursive(MainBuild);
        OnDotsFound.Invoke();
        for (int i = 0; i < MainBuild.transform.childCount; i++)
        {
            b_Buildings[i] = MainBuild.transform.GetChild(i);
        }                       
        currentBuilding = 0;
        qrTrackingHandler = FindObjectOfType<QRTrackingHandler>();
        cam_ToMap.SetActive(false);
        c_Question.SetActive(false);
        с_TurnOnBuild.SetActive(false);
        с_DifferentLoc.SetActive(false);
    }

    private void Start()
    {       
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        FillDropdown();
        c_ToMap.SetActive(false);
        c_AR.transform.Find("ToMap").GetComponent<Button>().interactable = true;
        c_Question.SetActive(false);
        с_TurnOnBuild.SetActive(false);
        с_DifferentLoc.SetActive(false);

        OnFloorChanged += GameManager_OnFloorChanged;  
        BuildingRoot.SetActive(false);
    }

    private void GameManager_OnFloorChanged(int floor)
    {
        ddFloor.GetComponent<ScrollDDFix>().isOver = false;
        ddFloor.value = ddFloor.options.FindIndex((i) => {
            return i.text.Contains(floor.ToString());
        });
    }

    private void GameManager_OnBuildChanged(int build)
    {
        ddBuild.GetComponent<ScrollDDFix>().isOver = false;
        ddBuild.value = ddBuild.options.FindIndex((i) => {
            return i.text.Contains(build.ToString());
        });
    }

    private void OnTrackableStatusChange(bool arg0)
    {
        if (!arg0) 
        {
            c_AR.transform.Find("Message").GetComponent<Image>().enabled = true;
            PersonaCanvas.SetActive(false);
            qrTrackingHandler.ClearQRData();
        }
    }

    private void OnReadQRSucess(QRTrackingHandler qrTrackingHandler)
    {
        c_AR.transform.Find("Message").GetComponent<Image>().enabled = false;
        currentLocation = Regex.Replace(qrTrackingHandler.QRData, @"\s+", "");
        destinationLocation = currentLocation;
        PersonalCanvas_INFO();
        if (currentLocation.Contains("BS"))
        {
            currentBuilding = 0;
            b_Buildings[0].gameObject.SetActive(true);
        }
        else if (currentLocation[0] == '1')
        {
            currentBuilding = 1;
            b_Buildings[1].gameObject.SetActive(true);
        }
        else if (currentLocation[0] == '2')
        {
            currentBuilding = 2;
            b_Buildings[2].gameObject.SetActive(true);
        }
        var dd_build = dds.Where(dd => dd.name.Contains("Building")).FirstOrDefault();        
        dd_build.onValueChanged.AddListener(delegate
        {
            dd_buildValueChanged(dd_build);
        });
        dd_build.value = currentBuilding;        
        c_AR.transform.Find("ToMap").GetComponent<Button>().interactable = true;        
    }

    public void MoveToMapView()
    {
        p_Mapping.NewMapping();
        roomInfo.SetActive(false);       
        qrTrackingHandler.StopAllCoroutines();
        qrTrackingHandler.enabled = false;

        c_AR.SetActive(false);
        c_Menu.SetActive(false);
        cam_AR.SetActive(false);
        c_ToMap.SetActive(true);
        cam_ToMap.SetActive(true);
        c_Question.SetActive(false);
        с_TurnOnBuild.SetActive(false);
        с_DifferentLoc.SetActive(false);
        b_BS_floors = new Transform[b_Buildings[currentBuilding].childCount];
        for (int i = 0; i < b_Buildings[currentBuilding].childCount; i++)
        {
            b_BS_floors[i] = b_Buildings[currentBuilding].GetChild(i);
        }
        FindObjectOfType<PlayerMovement>().FloorObj();
        
        if (currentBuilding == 0)
        {
            b_Buildings[0].gameObject.SetActive(true);
            b_Buildings[1].gameObject.SetActive(false);
            b_Buildings[2].gameObject.SetActive(false);
        }
        if (currentBuilding == 1)
        {
            b_Buildings[0].gameObject.SetActive(false);
            b_Buildings[1].gameObject.SetActive(true);
            b_Buildings[2].gameObject.SetActive(false);
        }
        if (currentBuilding == 2)
        {
            b_Buildings[0].gameObject.SetActive(false);
            b_Buildings[1].gameObject.SetActive(false);
            b_Buildings[2].gameObject.SetActive(true);
        }
        var getFloor = player.thisFloor.name;
        var getFloorNum = Convert.ToInt32((getFloor[getFloor.Length - 1]).ToString());
        floor_GameObjects[getFloorNum].SetActive(true);
        for(int i = 0; i < b_Buildings[currentBuilding].childCount; i++)
        {
            if(i!=getFloorNum)
            {
                floor_GameObjects[i].SetActive(false);
            }
            else { continue; }
        }        
        GameObject.Find("Player").gameObject.GetComponent<PlayerMovement>().SetupStartPosition();
        ChangeFloor(floor_GameObjects[0], player.thisFloor);
        StartCoroutine(FixFloorTitle(ddFloor));

        Camera.main.transform.position = new Vector3(GameObject.FindGameObjectWithTag("Player").transform.position.x, 0,GameObject.FindGameObjectWithTag("Player").transform.position.z);
    }

    [ContextMenu("ClearTrailPath")]
    public void ClearPath()
    {
       //////// player.p_pathAfterWalking.Clear();
    }

    public void ToAR()
    {
        qrTrackingHandler.onReadQRSuccess.AddListener(delegate
        {
            OnReadQRSucess(qrTrackingHandler);
        });
        qrTrackingHandler.statusChange.AddListener(OnTrackableStatusChange);
        c_AR.SetActive(true);
        cam_AR.SetActive(true);
        c_ToMap.SetActive(false);
        c_Menu.SetActive(false);
        cam_ToMap.SetActive(false);
        c_Question.SetActive(false);
        с_TurnOnBuild.SetActive(false);
        с_DifferentLoc.SetActive(false);
        qrTrackingHandler.enabled = true;
        player.GetComponent<NavMeshAgent>().enabled = false;
    }

    private void GetChildRecursive(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            if (child.name.Contains("floor"))
            {
                GameObject g = new GameObject();
                g.name = "Canvas" + b;
                g.transform.SetParent(child.transform, false);
                Canvas canvas = g.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                CanvasScaler cs = g.AddComponent<CanvasScaler>();
                cs.scaleFactor = 10.0f;
                cs.dynamicPixelsPerUnit = 800f;
                GraphicRaycaster gr = g.AddComponent<GraphicRaycaster>();
                canvas.transform.position = new Vector3(0, 0.5f, 0);
                b++;
            }

            if (child.name.Contains("dot"))
            {
                Dots dots = new Dots();
                Dots_BD.Add(dots);
                word = child.name.Split('_');
                wordFloor = child.parent.name.Split(' ');
                Dots_BD[i].dot_floor = Convert.ToInt32(wordFloor[1]);
                Dots_BD[i].dot_class = word[1];
                Dots_BD[i].dot_name = word[2];
                Dots_BD[i].dot_vector = child.transform.position;
                if (Dots_BD[i].dot_class == "elevator") { Dots_BD[i].dot_lift = true; } else { Dots_BD[i].dot_lift = false; }				            
				//Регистрация комнаты в списке комнат
				RoomInfo info = null;
				bool noInfo = false;
				switch (Dots_BD[i].dot_class.ToLower())
				{
					case "wardrobepr":
                    case "wardrobebs":
                    case "laboratory":
					case "hall":
					case "libruary":
                    case "exit1":
                    case "exit2":
                    case "exit3":
                    case "exit4":
                    case "exit5":
                    case "assemblyhall":
                    case "profkom":
                    case "buffet":
                    case "sportshall":
                        var t = Dots_BD[i];

                        info = new RoomInfo()
						{
							ID = roomInfoLoader.RoomClassConversion[Dots_BD[i].dot_class.ToLower()]
						};
					break;
					case "room":
						{
							info = roomInfoLoader[dots.dot_name];
                            if (info == null)
							{
                                info = new RoomInfo()
								{
									ID = ConvertLatinToCyrillic(word[2].Replace("BS", "A-")),
									Description = "Аудитория"
								};
							}
						}						
					break;
					default:
						noInfo = true;
					break;
				}
                if (child.parent.parent.name == "building BS")
                {
                    Dots_BD[i].dot_build = 0;
                }
                else if (child.parent.parent.name == "building PR1")
                {
                    Dots_BD[i].dot_build = 1;
                }
                else if (child.parent.parent.name == "building PR2")
                {
                    Dots_BD[i].dot_build = 2;
                }
                if (!noInfo)
				{
                    roomInfoLoader.AddRoom(dots.dot_name, info);
					rooms.Add(new Room(dots, info));
				}
				i++;
            }

            if (child.name.Contains("Highlight"))
            {               
                    TextMeshProUGUI tempTextBox = Instantiate(numbersOfCabin, child.transform.position, child.transform.rotation) as TextMeshProUGUI;
                if (child.parent.parent.parent.name == "building BS")
                {
                    tempTextBox.transform.SetParent(GameObject.Find("Canvas" + wordFloor[1]).transform, false);                    
                }
                if (child.parent.parent.parent.name == "building PR1")
                {
                    tempTextBox.transform.SetParent(GameObject.Find("Canvas" + (5+int.Parse(wordFloor[1]))).transform, false);                    
                }
                if (child.parent.parent.parent.name == "building PR2")
                {
                    tempTextBox.transform.SetParent(GameObject.Find("Canvas" + (10 + int.Parse(wordFloor[1]))).transform, false);
                }
                tempTextBox.transform.Rotate(new Vector3(180, 0, 0));
                tempTextBox.fontSize = 0.7f;               

                tempTextBox.name = "Text_" + word[2];
                tempTextBox.text = roomInfoLoader[word[2]] == null ? ConvertLatinToCyrillic(word[2].Replace("BS", "A-")) : roomInfoLoader[word[2]].ID;
            }
            if (null == child)
                continue;
            GetChildRecursive(child.gameObject);
        }
    }

    void FillDropdown()
    {
        dds = FindObjectsOfType<Dropdown>();

        #region FillBuilding
        var dd_build = dds.Where(dd => dd.name.Contains("Building")).FirstOrDefault();
        dd_build.onValueChanged.AddListener(delegate
        {
            dd_buildValueChanged(dd_build);
        });

        dd_build.options.Add(new Dropdown.OptionData("Б. Семеновская к.А"));
        dd_build.options.Add(new Dropdown.OptionData("Прянишникова к.1"));
        dd_build.options.Add(new Dropdown.OptionData("Прянишникова к.2"));
        dd_build.RefreshShownValue();      
        dd_build.value = 0;        
        ddBuild = dd_build;
        #endregion

        #region FillFloor
        var dd_floor = dds.Where(dd => dd.name.Contains("Floor")).FirstOrDefault();
        dd_floor.RefreshShownValue();
        dd_floor.onValueChanged.AddListener(delegate
            {
                dd_floorValueChanged(dd_floor);
            });

        for (int i = 0; i < b_Buildings[0].childCount; i++)
        {
            dd_floor.options.Add(new Dropdown.OptionData((i).ToString()));
        }               
        #endregion
    }

    internal void SetDestination(string result)
    {
        destinationLocation = result;
        c_FindLocation.SetActive(false);
    }

    public void ChangeFloor(GameObject prev, GameObject current)
    {
        prev.SetActive(false);
        current.SetActive(true);
    }

    IEnumerator FixFloorTitle(Dropdown dd)
    {
        yield return new WaitForSeconds(0.1f);
        if (!dd.captionText.text.Contains("этаж"))
            dd.captionText.text += " этаж";
    }

    #region Events
    public void dd_buildValueChanged(Dropdown dd_build)
    {
        roomInfo.SetActive(false);
        c_TextSearch.GetComponent<Text>().text = "Поиск аудитории";
        currentBuilding = dd_build.value;       
        if (dd_build.value == 0)
        {            
            b_Buildings[0].gameObject.SetActive(true);
            b_Buildings[1].gameObject.SetActive(false);
            b_Buildings[2].gameObject.SetActive(false);         
        }
        if (dd_build.value == 1)
        {
            b_Buildings[0].gameObject.SetActive(false);
            b_Buildings[1].gameObject.SetActive(true);
            b_Buildings[2].gameObject.SetActive(false);    
        }
        if (dd_build.value == 2)
        {
            b_Buildings[0].gameObject.SetActive(false);
            b_Buildings[1].gameObject.SetActive(false);
            b_Buildings[2].gameObject.SetActive(true);
        }
        if (c_AR.activeSelf == false) destinationLocation = "";                
        b_BS_floors = new Transform[b_Buildings[currentBuilding].childCount];
        for (int i = 0; i < b_Buildings[currentBuilding].childCount; i++)
        {
            b_BS_floors[i] = b_Buildings[currentBuilding].GetChild(i);
        }
        FindObjectOfType<PlayerMovement>().FloorObj();
        Dropdown dd_floor = dds.Where(dd => dd.name.Contains("Floor")).FirstOrDefault();
        dd_floor.ClearOptions();
        dd_floor.RefreshShownValue();
        for (int i = 0; i < b_Buildings[currentBuilding].childCount; i++)
        {
            dd_floor.options.Add(new Dropdown.OptionData((i).ToString()));
        }
        var getFloor = player.thisFloor.name;
        var getFloorNum = Convert.ToInt32((getFloor[getFloor.Length - 1]).ToString());
        dd_floor.value = getFloorNum;
        ddFloor = dd_floor;    
        StartCoroutine(FixFloorTitle(ddFloor));
        FindObjectOfType<PlayerMovement>().SetupStartPosition();        
        Camera.main.transform.position = new Vector3(GameObject.FindGameObjectWithTag("Player").transform.position.x, 0, GameObject.FindGameObjectWithTag("Player").transform.position.z);
        MainBuild.GetComponent<NavigationBaker>().Reload();
        for (int i = 0; i < floor_GameObjects.Count; i++)
        {           
            Debug.Log("floor_GameObjects " + i + " - " + floor_GameObjects[i]);
        }      
    }

    private void dd_floorValueChanged(Dropdown dd_floor)
    {
        var currentFloor = dd_floor.options[dd_floor.value].text[0];
        var captext = dd_floor.captionText.text;

        if (!captext.Contains("этаж"))
            dd_floor.captionText.text += " этаж";

        ChangeFloor(floor_GameObjects.Single(x=>x.name.Contains(FloorNow.ToString())) , floor_GameObjects.Single(x => x.name.Contains(currentFloor)));
        FloorNow = Convert.ToInt32(currentFloor.ToString());

        //визуальное скрытие игрока, если пользователь щелкает этажи
        if (player.thisFloor.name.Contains(currentFloor))
        {
            foreach (var item in player.playerMesh)
            {
                item.enabled = true;
            }
            try
            {
                player.GetComponentInChildren<TrailRenderer>().enabled = true;
            }
            catch
            {
                Debug.LogError("[Deprecated] Компонент TrailRenderer отсутсвует.");
            }
        }
        else
        {
            foreach (var item in player.playerMesh)
            {
                item.enabled = false;
            }
            try
            {
                player.GetComponentInChildren<TrailRenderer>().enabled = false;
            }
            catch
            {
                Debug.LogError("[Deprecated] Компонент TrailRenderer отсутсвует.");
            }
        }       
    }
    #endregion 
}
