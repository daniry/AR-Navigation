using RoomSearch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс комнаты
/// </summary>
public class Room : IRoom
{
	private List<TagGroup> tags = new List<TagGroup>();

	public Dots dot;
	public RoomInfo info;
	public string LatinID { get { return dot.dot_name; } }

	public void UpdateTags()
	{
		tags.Clear();
		//Сделать подстройку индексов удобнее
		tags.AddRange(
			new TagGroup[]{
			new TagGroup(info.ID, 6),
			new TagGroup(info.Name, 4),
			new TagGroup(info.Personel, 1)
			}

		);
	}
	/// <summary>
	/// Создаёт экземпляр комнаты с поисковой информацией
	/// </summary>
	/// <param name="fromDot">Путевая точка</param>
	/// <param name="fromInfo">Инфо о комнате</param>
	public Room(Dots fromDot, RoomInfo fromInfo)
	{
		dot = fromDot;
		info = fromInfo == null ? new RoomInfo() : fromInfo;
		info.ID = info.ID == ""? Core.ConvertLatinToCyrillic(LatinID.Replace("BS", "А-")) : info.ID;

		UpdateTags();
	}
	/// <summary>
	/// Реализация IRoom
	/// </summary>
	/// <returns></returns>
	public List<TagGroup> GetTags()
	{
		return tags;
	}
}
