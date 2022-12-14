using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Core;

public class Mapping : MonoBehaviour
{
    [SerializeField]
    private GameObject[] visibleStep;
    [SerializeField]
    private GameObject Parent;
    public bool WalkOrNot;
    [SerializeField]
    private int floor;

    private int i = 0;

    public float waitTime;

    private void Update()
    {

        {
            RefreshStepInMap();
            floor = FloorNow;
        }
    }

    public void RefreshStepInMap()
    {
        foreach (Transform child in Parent.transform)
        {
            if (child.name.Contains("step_" + floor.ToString()))
            {
                child.gameObject.SetActive(false);
            }
        }

        foreach (Transform child in Parent.transform)
        {
            if (child.name.Contains("step_" + FloorNow.ToString()))
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator MappingProcess()
    {
        if (WalkOrNot == false)
        {
            //NeWDot();
            FinishMapping();
        }
        yield return new WaitForSeconds(waitTime);
        GameObject go = Instantiate(visibleStep[1], new Vector3(transform.position.x, transform.position.y - 0.19F, transform.position.z), transform.rotation) as GameObject;
        go.transform.Rotate(new Vector3(90, 0, 90));
        go.name = "step_" + floor.ToString();
        go.transform.SetParent(Parent.transform);

        if (WalkOrNot == true)
        {
            RestartMapping();
        }
    }

    public void StartMapping()
    {
        foreach (Transform child in Parent.transform)
        {
            if (child.name.Contains("step_"))
            {
                Destroy(child.gameObject);
            }
        }
        floor = FloorNow;
        WalkOrNot = true;
        //NeWDot();
        StartCoroutine(MappingProcess());
    }

    public void RestartMapping()
    {
        StartCoroutine(MappingProcess());
    }

    public void FinishMapping()
    {
        StopCoroutine(MappingProcess());
    }

    void NeWDot()
    {
        GameObject go = Instantiate(visibleStep[0], new Vector3(transform.position.x, transform.position.y - 0.19F, transform.position.z), transform.rotation) as GameObject;
        go.transform.Rotate(new Vector3(90, 0, 90));
        go.name = "step_" + floor.ToString();
        go.transform.SetParent(Parent.transform);
    }
    public void NewMapping()
    {
        foreach (Transform child in Parent.transform)
        {
            if (child.name.Contains("step_"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}

