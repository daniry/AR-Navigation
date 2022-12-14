using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Core;
using RoomSearch;
using RoomSearch.SearchProviders;

public class SearchLocationEngine : MonoBehaviour
{
	public Transform Content;

	private GameObject itemExample;

	private RoomSearch<Room> roomSearchEngine;
	private StringRoomSearchProvider<Room> roomSearchProvider;

	private void Start()
	{
		itemExample = Content.GetChild(0).gameObject;

		//seek room objects

		roomSearchProvider = new StringRoomSearchProvider<Room>(rooms);
		roomSearchEngine = new RoomSearch<Room>(roomSearchProvider);
		roomSearchEngine.SearchResultQuantity = 15;
		roomSearchEngine.SearchFinished += SearchHandler;
	}
	List<Room> lastResult;
	string lastQuery;
	bool updated = false;
	private void SearchHandler(List<Room> result, string query)
	{
		lastResult = result;
		lastQuery = query;
		//Await for the main thread to perform update
		UnityMainThreadDispatcher.Instance().Enqueue(UpdateList);
	//	updated = true;
	}
	void UpdateList()
	{
		ClearList();
		foreach (Room item in lastResult)
		{
			GameObject obj = Instantiate(itemExample);
			obj.transform.SetParent(Content, false);
			obj.SetActive(true);
			Text[] textElements = obj.GetComponentsInChildren<Text>();

			textElements[0].text = item.info.ID;
			textElements[1].text = item.info.Name;
			//obj.GetComponentInChildren<Text>().text = item.info.ID;
			(obj.GetComponent<DestinationSelect>()).room = item;
		}
	}

	public void OnInputQuery(string query)
	{
		roomSearchEngine.SearchQuery = query;
		//Deprecated
		//ClearList();
		//foreach (var item in Search(query))
		//{
		//    var obj = Instantiate(itemExample);
		//    obj.transform.SetParent(Content);
		//    obj.SetActive(true);
		//    obj.GetComponentInChildren<Text>().text = ConvertLatinToCyrillic(item.dot_name);
		//}
	}

	private void ClearList()
	{
		if (Content.childCount > 1)
		{
			for (int i = 1; i < Content.childCount; i++)
			{
				Destroy(Content.GetChild(i).gameObject);
			}
		}
	}
	/// <summary>
	/// Deprecated
	/// </summary>
	/// <param name="query"></param>
	/// <returns></returns>
	IEnumerable<Dots> Search(string query)
	{
		return Dots_BD.Where(x => x.dot_name.Contains(ConvertCyrillicToLatin(query)));
	}
}
