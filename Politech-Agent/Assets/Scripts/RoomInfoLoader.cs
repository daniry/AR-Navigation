using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class RoomInfoLoader
{
	//Supported sources
	public enum SourceType { LocalCSV }
	public GameManager gm;
	private Dictionary<string, RoomInfo> roomInfos;

	enum Field { SceneID, ID, Name, Personel, Description, PersonelTitle, ImageResource };
	Dictionary<string, Field> namingScheme = new Dictionary<string, Field>()
	{
		{ "Сцена",Field.SceneID },
		{ "Номер аудитории",Field.ID },
		{ "Название подразделения",Field.Name },
		{ "Название подразделения ",Field.Name }, //почему-то есть пробел в excel файле
		{ "ФИО",Field.Personel },
		{ "Краткое описание",Field.Description },
		{ "Должность",Field.PersonelTitle },
		{ "Изображение", Field.ImageResource }
	};
	public RoomInfoLoader(string source, SourceType type = SourceType.LocalCSV)
	{

		roomInfos = new Dictionary<string, RoomInfo>();
		switch (type)
		{
			case SourceType.LocalCSV:
			LoadCSV(source);
			break;
			default:
			break;
		}


	}
	public RoomInfo this[string latinRoomID]
	{
		get
		{
			RoomInfo info;
			if (!roomInfos.TryGetValue(latinRoomID.ToLower(), out info))
				return null;
			return info;
		}
	}

	private void LoadCSV(string file, string rowSeparator = "\r\n", char elementSeparator = ';')//, string encoding = "Windows-1251")
	{
		string csvContents = Encoding.UTF8.GetString(Resources.Load<TextAsset>(file).bytes);//Encoding.GetEncoding(encoding)

#if UNITY_IOS
        rowSeparator = "\n";
#endif

        List<string> csvLines = csvContents.Split(new string[] { rowSeparator }, StringSplitOptions.None).ToList();


		Dictionary<Field, int> csvScheme = new Dictionary<Field, int>();
		{
			string[] columns = csvLines.ElementAt(0).Split(elementSeparator);
			for (int i = 0; i < columns.Length; i++)
			{
				string column = columns[i];
				if (namingScheme.ContainsKey(column))
				{
					csvScheme.Add(namingScheme[column], i);
				}
			}
		}
		csvLines.RemoveAt(0);

		foreach (string csvLine in csvLines)
		{
			string[] columns = csvLine.Split(elementSeparator);
			if (columns.Length < csvScheme.Count) break;

			RoomInfo info = new RoomInfo();


			info.ID = columns[csvScheme[Field.ID]]; 
			
			info.Name = columns[csvScheme[Field.Name]];
			info.Personel = columns[csvScheme[Field.Personel]];
			info.Description = columns[csvScheme[Field.Description]];
			info.PersonelTitle = columns[csvScheme[Field.PersonelTitle]];
			info.ImageResource = 
				columns[csvScheme[Field.ImageResource]] != "" ?
					columns[csvScheme[Field.ImageResource]] : 
					columns[csvScheme[Field.SceneID]];//fallback to resource name as default(scene name)


			if (!roomInfos.ContainsKey(columns[csvScheme[Field.SceneID]]))
				roomInfos.Add(columns[csvScheme[Field.SceneID]].ToLower(), info);
			else
				Debug.Log(string.Format("Conflicting room {0}:{1}:{2}.", columns[csvScheme[Field.SceneID]], info.ID, info.Name));

		}
	}

	//case "wardrobe":
	//				case "laboratory":
	//				case "hall":
	//					info = new RoomInfo()
	//{
	//	ID = Dots_BD[i].dot_name,
	//					};
	//				break;
	//				case "libruary":
	//				info = new RoomInfo()
	//{
	//	ID = "Библиотека"
	//				};
	//				break;
	//				case "exit":
	//					info = new RoomInfo()
	//{
	//	ID = "Выход"
	//					};
	//				break;
	//				case "sportshall":
 //                       var t = Dots_BD[i];

	//info = new RoomInfo()
	//{
	//	ID = "Спортивный зал"
	//					};
	//				break;
	public readonly Dictionary<string, string> RoomClassConversion = new Dictionary<string, string>()
	{
		{ "wardrobepr","Гардероб Прянишникова к.2" },
        { "wardrobebs","Гардероб Б.Семёновская" },
        { "hall","Холл" },
		{ "laboratory","Лаборатория" },
		{ "libruary","Библиотека" },
		{ "sportshall","Спортивный зал" },
        { "buffet","Буфет" },
        //{ "exit","Выход" },
        { "exit1","Выход из Б.Семёновская" },
        { "exit2","Выход из Прянишникова к.2" },
        { "exit3","Переход в Прянишникова к.1" },
        { "exit4","Переход в Прянишникова к.2" },
        { "exit5","Выход ПР2" },
        { "assemblyhall","Актовый зал" },
        { "profkom","Профком" }
    };
	public bool AddRoom(string latinRoomID, RoomInfo info)
	{
		if (!roomInfos.ContainsKey(latinRoomID.ToLower()))
			roomInfos.Add(latinRoomID.ToLower(), info);
		else
			return false;

		return true;
	}
}

