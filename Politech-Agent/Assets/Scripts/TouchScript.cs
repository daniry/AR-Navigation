using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Core;

public class TouchScript : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Material[] materials;

    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private GameObject roomInfo;
    [SerializeField]
    private Text roomName_text;
    [SerializeField]
    private Text roomDescription_text;

    [SerializeField]
    private TouchCamera panCamera;

    private string roomName, roomDescription;

    private void Start()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        panCamera = Camera.main.GetComponent<TouchCamera>();
        if (panCamera.IsMoved == false)
        {
            Debug.Log("Нажал");
            if (gameManager.rendererOfPressedFloor != null)
            {
                gameManager.rendererOfPressedFloor.GetComponent<Renderer>().material = materials[0];
            }

            gameManager.rendererOfPressedFloor = eventData.pointerEnter.gameObject;
            eventData.pointerEnter.gameObject.GetComponent<Renderer>().material = materials[1];

            roomInfo.SetActive(true);
            string[] word = eventData.pointerEnter.gameObject.transform.parent.gameObject.name.Split('_');
            Dots room = Dots_BD.Find(i => i.dot_name.Contains(word[2]));

            roomName = room.dot_name;

            //Переделать при смене способа хранения комнат

            RoomInfo info = null;

            try
            {
				
				{
					Room foundRoom = rooms.Find(x => x.dot == room);
					info = foundRoom != null ? foundRoom.info : null;
				}
					
            }
            catch
            {
                Debug.LogError("Информация о данной комнате отсутствует. Необходимо внести в БД данные.");
            }

			if(info != null)
			{
				roomName_text.text = info.ID;
				roomDescription_text.text =
					info.Description != "" ?//Если есть описание
					info.Description :
					info.Name != "" ?//Если есть имя
					info.Name:
					"";//Если ничего нет
			}
			else
			{
				roomName_text.text = ConvertLatinToCyrillic(roomName.Replace("BS", "А-"));
				roomDescription_text.text = "";
			}
            
            //roomDescription_text.text = "Вы нажали на аудиторию - " + roomName_text.text;

            gameManager.SetDestination(roomName);
        }
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (panCamera.IsMoved == false)
    //    {
    //        Debug.Log("Нажал");
    //        if (gameManager.rendererOfPressedFloor != null)
    //        {
    //            gameManager.rendererOfPressedFloor.GetComponent<Renderer>().material = materials[0];
    //        }

    //        gameManager.rendererOfPressedFloor = eventData.pointerEnter.gameObject;
    //        eventData.pointerEnter.gameObject.GetComponent<Renderer>().material = materials[1];

    //        roomInfo.SetActive(true);
    //        string[] word = eventData.pointerEnter.gameObject.transform.parent.gameObject.name.Split('_');
    //        Dots room = Dots_BD.Find(i => i.dot_name.Contains(word[2]));

    //        roomName = room.dot_name;

    //        roomName_text.text = ConvertLatinToCyrillic(roomName);
    //        //roomDescription_text.text = "Вы нажали на аудиторию - " + roomName_text.text;

    //        gameManager.SetDestination(roomName);
    //    }
    //}
}
