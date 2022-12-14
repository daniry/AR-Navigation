using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Core;

public class DestinationSelect : MonoBehaviour
{
    Button button;
	public Room room;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate
        {
            OnButtonClick(button);
        });
    }

    private void OnButtonClick(Button button)
    {
		//deprecated
		//var result = ConvertCyrillicToLatin(button.GetComponentInChildren<Text>().text);
		//FindObjectOfType<GameManager>().SetDestination(result);
		FindObjectOfType<GameManager>().SetDestination(room.LatinID);
		GameObject.FindWithTag("FindLocationInput").GetComponentInChildren<Text>().text = room.info.ID;//button.GetComponentInChildren<Text>().text;
    }
}
 