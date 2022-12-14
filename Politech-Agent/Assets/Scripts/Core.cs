using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Core
{
    public delegate void FloorChanged(int floor);
    public static event FloorChanged OnFloorChanged = delegate { };

    public delegate void BuildChanged(int build);
    public static event BuildChanged OnBuildChanged = delegate { };

    public static Dots temp_dot;
    public static bool isDragging;

    /// <summary>
    /// Акцентный цвет
    /// </summary>
    public static Color akcentColor = new Color(0.9019608f, 0.1960784f, 0.2745098f);

    /// <summary>
    /// Список всех найденных пунктов назначения
    /// </summary>
    public static List<Dots> Dots_BD = new List<Dots>();

	/// <summary>
	/// Список зарегистрированных комнат
	/// </summary>
	public static List<Room> rooms = new List<Room>();

	/// <summary>
	/// Список всех этажей здания
	/// </summary>
	public static List<GameObject> floor_GameObjects = new List<GameObject>();

    /// <summary>
    /// Список всех локаций
    /// </summary>
    public static List<GameObject> build_GameObjects = new List<GameObject>();

    /// <summary>
    /// Текущий этаж
    /// </summary>
    public static int FloorNow
    {
        get
        {
            return floorNow;
        }
        set
        {
            floorNow = value;
            OnFloorChanged(floorNow);
        }
    }

    public static int BuildNow
    {
        get
        {
            return buildNow;
        }
        set
        {
            BuildNow = value;
            OnBuildChanged(buildNow);
        }
    }


    private static int floorNow;
    private static int buildNow;

    /// <summary>
    /// Текущая стартовая точка маршрута
    /// </summary>
    public static Dots start_dot;

    /// <summary>
    /// Текущая финишная точка маршрута
    /// </summary>
    public static Dots finish_dot;

    /// <summary>
    /// Конвертирует название аудитории с кириллицы на латиницу
    /// </summary>
    /// <param name="original">Исходное название</param>
    /// <returns></returns>
    public static string ConvertCyrillicToLatin(string original)
    {
        original = original.ToUpper();
        foreach (var item in original)
        {
            switch (item)
            {
                case 'П':
                    original = original.Replace(item, 'P');
                    break;
                case 'Р':
                    original = original.Replace(item, 'R');
                    break;
                case 'Б':
                    original = original.Replace(item, 'B');
                    break;
                case 'С':
                    original = original.Replace(item, 'S');
                    break;
            }
        }

        return original;
    }

    /// <summary>
    /// Конвертирует название аудитории с латиницы на кириллицу
    /// </summary>
    /// <param name="original">Исходное название</param>
    /// <returns></returns>
    public static string ConvertLatinToCyrillic(string original)
    {
        original = original.ToUpper();
        foreach (var item in original)
        {
            switch (item)
            {
                case 'P':
                    original = original.Replace(item, 'П');
                    break;
                case 'R':
                    original = original.Replace(item, 'Р');
                    break;
                case 'B':
                    original = original.Replace(item, 'Б');
                    break;
                case 'S':
                    original = original.Replace(item, 'С');
                    break;
            }
        }

        return original;
    }
}
