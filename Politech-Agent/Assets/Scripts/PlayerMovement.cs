using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Core;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private GameObject EventSystem;
    [SerializeField]
    private NavMeshAgent[] p_navAgents;
    [SerializeField]
    private Vector3 p_targetMarker;
    [SerializeField]
    private InputField p_startField, p_finishField;
    private RoomInfoLoader roomInfoLoader;
    Dropdown ddFloor;
    Dropdown ddBuild;
    public string start_door, finish_door;
    private Vector3 end;
    public string temp_door = null;
    public Mapping p_Mapping;

    private bool walking;
    public bool Walking
    {
        get
        {
            return walking;
        }

        set
        {
            if (walking != value)
            {
                if (value)
                    animator.SetTrigger("Walk");
                else
                    animator.SetTrigger("Stop");
            }

            walking = value;
        }
    }
    float Distance = 1000000f;

    Dots lift_dot;

    public GameObject thisFloor;
    public GameObject thisBuild;
    bool otherFloor;

    public GameObject c_ButtonStartSearch;
    public GameObject c_TextSearch;

    public GameObject c_Question;
    public GameObject с_TurnOnBuild;
    public GameObject с_DifferentLoc;
    public GameObject c_ToMap;
   
    GameManager gm;
    Animator animator;

    [HideInInspector]
    public Renderer[] playerMesh;

    public void FloorObj()
    {
        p_navAgents[0].enabled = false;
        floor_GameObjects.Clear();    
        for (int i = 0; i < gm.b_BS_floors.Length; i++)
        {
            floor_GameObjects.Add(gm.b_BS_floors[i].gameObject);            
        }
        OutFloor(gm.MainBuild);        
        thisFloor = floor_GameObjects[0];
        FloorNow = Convert.ToInt32(thisFloor);
        for (int i = 1; i < floor_GameObjects.Count; i++)
        {
            floor_GameObjects[i].SetActive(false);
        }
        ddFloor = gm.ddFloor;
        p_Mapping.NewMapping();       
    }


    void Start()
    {
        var rabbit = transform.Find("Metka");
        playerMesh = rabbit.GetComponentsInChildren<Renderer>();
        animator = rabbit.GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        thisFloor = null;
    }

    private void OutFloor(GameObject obj)
    {
        int i=0;       
        foreach (Transform child in obj.transform)
        {
            if (name.Contains("floor"))
            {
                floor_GameObjects[i] = gameObject;
                i++;
            }
            if (null == child)
                continue;
            OutFloor(child.gameObject);
        }       
    }

    public void SetupStartPosition()
    {
        c_Question.SetActive(false);
        if (gm.destinationLocation == "" && temp_door=="")
        {
            Debug.LogError("START POINT IS NULL! Setting default: Exit (1)");           
                if (gm.currentBuilding == 0)
                {
                    gm.currentLocation = "exit1";
                }
                if (gm.currentBuilding == 1)
                {
                gm.currentLocation = "exit4";
                }
                if (gm.currentBuilding == 2)
                {
                    gm.currentLocation = "exit2";
                }           
        }        
        start_dot = Dots_BD.Find(item => item.dot_name == gm.currentLocation); // ищет в базе данных информацию о точке
        var t = start_dot;    
        //перемещает игрока на стартовую точку
        transform.position = new Vector3(start_dot.dot_vector.x,
                                         transform.position.y,
                                         start_dot.dot_vector.z);
        thisFloor.SetActive(false); //выключает нынешний этаж
        thisFloor = floor_GameObjects[start_dot.dot_floor]; //устанваливает нынешний этаж
        FloorNow = start_dot.dot_floor; //ищет нынешний этаж по координатам точки
        floor_GameObjects[FloorNow].SetActive(true); //включает нынешний этаж

        //обнуляет агент, графичекую полоску пути, обновляет агента
        p_navAgents[0].enabled = true;
        gm.MainBuild.GetComponent<NavigationBaker>().Reload();
    }

    public void StepFirstToStart()
    {
        if (c_TextSearch.GetComponent<Text>().text == "" || c_TextSearch.GetComponent<Text>().text == "Поиск аудитории")
        {
            c_ButtonStartSearch.GetComponent<Button>().interactable = false;
            c_ButtonStartSearch.GetComponent<Button>().interactable = true;
            Debug.LogError("Не введен номер аудитории");
        }
        else
        { c_ButtonStartSearch.GetComponent<Button>().interactable = true; }

            if (gm.currentLocation != gm.destinationLocation)
            {
                foreach (Transform child in gm.b_Buildings[gm.currentBuilding].gameObject.transform)
                {
                    if (child.name.Contains("floor"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                foreach (var item in playerMesh)
                {
                    item.enabled = true;
                }
                EventSystem.SetActive(false);
                SetupStartPosition();
                Debug.Log("Начало 1 фазы");
                start_door = gm.currentLocation; //добавление точки старта
                finish_door = gm.destinationLocation; //добавление точки финиша
                Debug.Log(start_door + " : " + finish_door);               
                finish_dot = Dots_BD.Find(item => item.dot_name == finish_door); // ищет в базе данных информацию о точке
            if (start_dot.dot_build != finish_dot.dot_build)
            {
                DifferentBuildings();
            }
            if (start_dot.dot_build == finish_dot.dot_build)
            {
                Walking = true; //разрешает идти
                p_Mapping.StartMapping();
                Debug.Log("Конец 1 фазы");
            }
            //обнуляет агент, графичекую полоску пути, обновляет агента
            p_navAgents[0].enabled = true;

                gm.MainBuild.GetComponent<NavigationBaker>().Reload();

                end = finish_dot.dot_vector; //выбирает конечную точку
                p_targetMarker = end;
                Debug.Log(start_dot.dot_floor + " " + finish_dot.dot_floor);
            if (c_TextSearch.GetComponent<Text>().text.Contains("BS"))
            {
                c_TextSearch.GetComponent<Text>().text = ConvertLatinToCyrillic(roomInfoLoader.RoomClassConversion[finish_dot.dot_class.ToLower()].Replace("BS", "А-"));
            }
            }       
    }

    public void DifferentBuildings()
    {
        EventSystem.SetActive(true);
        if ((start_dot.dot_build == 1 && start_dot.dot_name == "exit4" && finish_dot.dot_build == 2)
            || (start_dot.dot_build == 2 && start_dot.dot_name == "exit3" && finish_dot.dot_build == 1))//если стоит в ПР1 и хочет в ПР2
        {
            с_TurnOnBuild.SetActive(true);
        }
        else  if ((start_dot.dot_build == 0 && finish_dot.dot_build == 1)
               ||  (start_dot.dot_build == 1 && finish_dot.dot_build == 0)
               ||  (start_dot.dot_build == 0  && finish_dot.dot_build == 2) //если стоит в БС на выходе и хочет в ПР2
            || (start_dot.dot_build == 2  && finish_dot.dot_build == 0)) //если из БС в ПР1 и наоборот
            {
                c_Question.SetActive(false);
                с_TurnOnBuild.SetActive(false);
                с_DifferentLoc.SetActive(true);
                c_TextSearch.GetComponent<Text>().text = "Поиск аудитории";
            }       
         else
            {
            c_Question.SetActive(true);
       }
    }

    public void YesButtonClicked()
    {
        EventSystem.SetActive(false);
        Debug.Log("Зашли в YesButton.");
        temp_dot = finish_dot;
        temp_door = finish_door;
        gm.tempLocation = gm.destinationLocation;
        if (start_dot.dot_build == 0)
        {
            finish_door = "exit1";
        }
        if (start_dot.dot_build == 1)
        {
            finish_door = "exit4";
        }
        if (start_dot.dot_build == 2)
        {
            finish_door = "exit3";
        }
        //переопределяем для finish_dot и _door пункт назначения в exit
        finish_dot = Dots_BD.Find(item => item.dot_name == finish_door);
        p_Mapping.StartMapping();
        p_navAgents[0].enabled = true;
        gm.MainBuild.GetComponent<NavigationBaker>().Reload();
        end = finish_dot.dot_vector;
        gm.tempLocation = "";     
        gm.destinationLocation = finish_door;
        gm.currentLocation = gm.destinationLocation;
        FloorMoving(finish_dot);
        Debug.Log("Зашли в FloorMoving.");
        c_ToMap.SetActive(true);
        c_Question.SetActive(false);
        Walking = true;
        if (Vector3.Distance(transform.position, finish_dot.dot_vector) < 0.5f)
        {
            с_TurnOnBuild.SetActive(true);
        }     
    }

    public void QueToOtherBuild()
    {
        EventSystem.SetActive(false);
        if (temp_door == "")
        {
            temp_dot = finish_dot;
            temp_door = finish_door;
            gm.tempLocation = gm.destinationLocation;
            //в зависимости от корпуса, определяем finish_dot как exit
            if (start_dot.dot_build == 0)
            {
                finish_door = "exit1";
            }
            if (start_dot.dot_build == 1)
            {
                finish_door = "exit4";
            }
            if (start_dot.dot_build == 2)
            {
                finish_door = "exit3";
            }
            //переопределяем для finish_dot и _door пункт назначения в exit
            finish_dot = Dots_BD.Find(item => item.dot_name == finish_door);
        }
        c_ToMap.SetActive(true);
        с_TurnOnBuild.SetActive(false);
        finish_door = temp_door;
        finish_dot = Dots_BD.Find(item => item.dot_name == finish_door);
        if (finish_dot.dot_build == 0 && start_dot.dot_build == 2) //из ПР2 в БС
        {
            start_door = "exit1";
        }
        if (finish_dot.dot_build == 2 && start_dot.dot_build == 0)//из БС в ПР
        {
            start_door = "exit2";
        }
        if (finish_dot.dot_build == 1 && start_dot.dot_build == 2) //из ПР2 в ПР1
        {
            start_door = "exit4";
        }
        if (finish_dot.dot_build == 2 && start_dot.dot_build == 1)// из ПР1 в ПР2
        {
            start_door = "exit3";
        }
        start_dot = Dots_BD.Find(item => item.dot_name == start_door);
        gm.currentLocation = start_door;
        gm.currentBuilding = finish_dot.dot_build;
        Walking = true;
        gm.ddBuild.value = finish_dot.dot_build;
        gm.dd_buildValueChanged(gm.ddBuild);
        temp_door = "";
        temp_dot = null;
        Walking = true;
        end = finish_dot.dot_vector; //выбирает конечную точку
        p_Mapping.StartMapping();
        p_navAgents[0].enabled = true;
        gm.MainBuild.GetComponent<NavigationBaker>().Reload();
        end = finish_dot.dot_vector;
        Walking = true;
        this.enabled = true;
        gm.tempLocation = "";
        gm.destinationLocation = finish_door;
        gm.currentLocation = gm.destinationLocation;
        FloorMoving(finish_dot);
        Debug.Log("Зашли в QueToOtherBuild.");
        c_ToMap.SetActive(true);
        с_TurnOnBuild.SetActive(false);
        Walking = true;
    }

    public void NoButtonClicked()
    {
        Distance = 1000000f;
        otherFloor = false;
        p_navAgents[0].enabled = false;
        Walking = false;
        EventSystem.SetActive(true);
        p_Mapping.WalkOrNot = false;
        this.enabled = false;
        c_ToMap.SetActive(true);
        c_Question.SetActive(false);
        с_TurnOnBuild.SetActive(false);
        с_DifferentLoc.SetActive(false);
        Debug.Log("Зашли в NoButtonClicked.");
        gm.destinationLocation = "";
        finish_door = "";
        start_door = "";
        start_dot = null;
        finish_dot = null;
        temp_door = "";
        temp_dot = null;
        c_TextSearch.GetComponent<Text>().text = "Поиск аудитории";
    }

    private void StepSecondToFinish(Vector3 targetPosition)
    {
        foreach (NavMeshAgent agent in p_navAgents) //идет от точки к точке
        {
            agent.destination = targetPosition;
        }
    }

    void FloorMoving(Dots finish_dot)
    {
                if (start_dot.dot_floor == finish_dot.dot_floor && Walking == true) // Если этаж одинаковый
                {
                    if (((start_dot.dot_name == "1011" || start_dot.dot_name == "1012" || start_dot.dot_name == "1013" || start_dot.dot_name == "1014")
                        && (finish_dot.dot_name == "Assemblyhall" || finish_dot.dot_name == "1010") && (start_dot.dot_floor == 0 && finish_dot.dot_floor == 0))
                        || ((start_dot.dot_name == "Assemblyhall" || start_dot.dot_name == "1010")
                        && (finish_dot.dot_name == "1011" || finish_dot.dot_name == "1012" || finish_dot.dot_name == "1013" || finish_dot.dot_name == "1014") && (start_dot.dot_floor == 0 && finish_dot.dot_floor == 0))
                        )
                    {
                        List<Dots> lift = Dots_BD.FindAll(item => item.dot_lift == true && item.dot_build == start_dot.dot_build);
                        List<Dots> liftNeed = lift.FindAll(item => item.dot_floor == FloorNow);
                        foreach (Dots dots in liftNeed)
                        {
                            Debug.Log("Ищем лифт хех");
                            if (Vector3.Distance(dots.dot_vector, finish_dot.dot_vector) < Distance)
                            {
                                if (CalculateNewPath(p_navAgents[0], dots.dot_vector))
                                {
                                    Debug.Log("Если дистанция маленькая, то выбрать лифт хех");
                                    Distance = Vector3.Distance(dots.dot_vector, finish_dot.dot_vector);
                                    lift_dot = dots;
                                }
                            }
                        }
                        if (lift_dot != null)
                        {
                            //Идем до лифта
                            Debug.Log("Идем до лифта хех");
                            foreach (NavMeshAgent agent in p_navAgents)
                            {
                                agent.destination = lift_dot.dot_vector;
                            }
                            if (Vector3.Distance(transform.position, lift_dot.dot_vector) < 0.5f)
                            {
                                Debug.Log("Если дошел до лифта, то... хех");
                                gm.ChangeFloor(floor_GameObjects[FloorNow], floor_GameObjects[1]);
                                thisFloor = floor_GameObjects[1];
                                FloorNow = 1;

                                ddFloor.value = ddFloor.options.FindIndex((i) =>
                                {
                                    return i.text.Contains(FloorNow.ToString());
                                });
                                var getFloor = thisFloor.name;
                                var getFloorNum = Convert.ToInt32((getFloor[getFloor.Length - 1]).ToString());
                                gm.ddFloor.value = getFloorNum;
                                gm.MainBuild.GetComponent<NavigationBaker>().Reload();
                                gm.currentLocation = "1101";
                                start_dot.dot_name = "1101";
                                start_dot.dot_floor = 1;
                                start_door = "1101";
                                lift_dot = null;
                                lift_dot = null;
                                lift = null;
                                liftNeed = null;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Начало 2 фазы 1");
                        if (transform.position.x != end.x || transform.position.z != end.z)
                        {
                            StepSecondToFinish(new Vector3(finish_dot.dot_vector.x,
                                                            finish_dot.dot_vector.y,
                                                            finish_dot.dot_vector.z));

                            if (Vector3.Distance(transform.position, finish_dot.dot_vector) < 0.5f)
                            {
                                p_navAgents[0].enabled = false;
                                Walking = false;
                                gm.currentLocation = gm.destinationLocation;
                                EventSystem.SetActive(true);
                                p_Mapping.WalkOrNot = false;
                                this.enabled = false;

                                Debug.Log("Пришли к финишу");
                                Distance = 1000000f;
                                if (temp_door != "")
                                {
                                    с_TurnOnBuild.SetActive(true);
                                }
                            }
                        }
                    }

                }
                if ((start_dot.dot_floor != finish_dot.dot_floor && Walking == true))    // Если этажи разные
                {
                    Debug.Log("Начало 2 фазы 2");
                    if (transform.position.x != end.x || transform.position.z != end.z)
                    {
                        //Создаем лист с лифтами на нашем этаже
                        List<Dots> lift = Dots_BD.FindAll(item => item.dot_lift == true && item.dot_build == start_dot.dot_build);
                        List<Dots> liftNeed = lift.FindAll(item => item.dot_floor == FloorNow);
                        //Ищем ближайший лифт к нашей точке
                        foreach (Dots dots in liftNeed)
                        {
                            Debug.Log("Ищем лифт");
                            if (Vector3.Distance(dots.dot_vector, finish_dot.dot_vector) < Distance)
                            {
                                if (CalculateNewPath(p_navAgents[0], dots.dot_vector))
                                {
                                    Debug.Log("Если дистанция маленькая, то выбрать лифт");
                                    Distance = Vector3.Distance(dots.dot_vector, finish_dot.dot_vector);
                                    lift_dot = dots;
                                }
                            }
                        }
                        //Когда лифт найден...
                        if (lift_dot != null)
                        {
                            //Идем до лифта
                            Debug.Log("Идем до лифта");
                            foreach (NavMeshAgent agent in p_navAgents)
                            {
                                agent.destination = lift_dot.dot_vector;
                            }
                            //Доходим до нужного этажа, меняем этаж, обнуляем путь, перегенерируем этаж для навигации, обнуляем лифт
                            if (Vector3.Distance(transform.position, lift_dot.dot_vector) < 0.5f)
                            {
                                Debug.Log("Если дошел до лифта, то...");
                                gm.ChangeFloor(floor_GameObjects[FloorNow], floor_GameObjects[finish_dot.dot_floor]);
                                thisFloor = floor_GameObjects[finish_dot.dot_floor];
                                FloorNow = finish_dot.dot_floor;
                                ddFloor.value = ddFloor.options.FindIndex((i) =>
                                {
                                    return i.text.Contains(FloorNow.ToString());
                                });

                                var getFloor = thisFloor.name;
                                var getFloorNum = Convert.ToInt32((getFloor[getFloor.Length - 1]).ToString());
                                gm.ddFloor.value = getFloorNum;
                                gm.MainBuild.GetComponent<NavigationBaker>().Reload();
                                otherFloor = true;

                                lift_dot = null;
                            }
                        }
                        //Если мы на другом этаже, то идем до финишной точки
                        if ((transform.position.x != end.x && otherFloor == true) || (transform.position.z != end.z && otherFloor == true))
                        {
                            Debug.Log("Если на другом этаже, то идем до финиша");
                            foreach (NavMeshAgent agent in p_navAgents)
                            {
                                agent.destination = finish_dot.dot_vector;
                            }
                            //Если мы на финишной точке, то обнуляем навигацию, убираем флаг хотьбы, выключаем скрипт                          
                            var d = Vector3.Distance(transform.position, finish_dot.dot_vector);
                            if (Vector3.Distance(transform.position, finish_dot.dot_vector) < 0.5f)
                            {
                                Debug.Log("Пришли к финишу");
                                Distance = 1000000f;
                                lift_dot = null;
                                lift = null;
                                liftNeed = null;
                                otherFloor = false;
                                p_navAgents[0].enabled = false;
                                Walking = false;                          
                        gm.currentLocation = gm.destinationLocation;
                                EventSystem.SetActive(true);
                                p_Mapping.WalkOrNot = false;
                                this.enabled = false;
                                if (temp_door != "")
                                {
                                    с_TurnOnBuild.SetActive(true);
                                }                           
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Пришли к финишу");

                        Distance = 1000000f;
                        lift_dot = null;
                        otherFloor = false;
                        p_navAgents[0].enabled = false;
                        Walking = false;
                
                gm.currentLocation = gm.destinationLocation;
                        EventSystem.SetActive(true);
                        p_Mapping.WalkOrNot = false;
                        this.enabled = false;                        
                        if (temp_door != "")
                        {
                            с_TurnOnBuild.SetActive(true);
                        }
                    }              
        }
    }

    private void Update()
    {      
            if (start_dot != null && finish_dot != null)
            {
                if (start_dot.dot_build == finish_dot.dot_build)
                {
                    FloorMoving(finish_dot);
                }
                else
                {
                    DifferentBuildings();
                }
            }
            if (Walking)
            {
                Camera.main.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
    }

    bool CalculateNewPath(NavMeshAgent spawnPosition, Vector3 targetPosition)
    {
        var navMeshPath = new NavMeshPath();
        spawnPosition.CalculatePath(targetPosition, navMeshPath);
        print("New path calculated");
        if (navMeshPath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}