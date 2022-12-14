using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Расширенный компонент Dropdown, который позволяет подменивать визуальный вид вершины списка в зависимости от того, раскрыт этот список или нет
/// </summary>
public class TwoSpriteDropdown : Dropdown
{
    public Sprite normal;
    public Sprite opened;

    private int prevChildren;
    private int curChildren;

    //Очень костыльный метод, чтобы понять раскрыт список или нет, 
    //однако у данного компонента почему-то нет event-ов типа OnShow() или OnHide()
    //а события типа OnSelect/Cancel/Click работают некорректно
    private void FixedUpdate()
    {
        curChildren = transform.childCount;

        if (curChildren != prevChildren)
        {
            curChildren = transform.childCount;
            if (curChildren == 3)
                IsOpened(false);
            else
                IsOpened(true);

            prevChildren = curChildren;
        }
    }

    private void IsOpened(bool o)
    {
        if (interactable)
        {
            if (o)
            {
                GetComponent<Image>().sprite = opened;
            }
            else
            {
                GetComponent<Image>().sprite = normal;
            }
        }
    }
}
