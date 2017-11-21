using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    // mouseDragging
    public bool mouseDragging = false;

    // Самая верхняя
    public bool isFirst = false;

    // В колоде
    public bool vKolode = true;

    // inCellKolodaHelper
    public bool inCellKolodaHelper = false;
    public int sortingOrderAdd = 100;
    public int sortingOrderAddZ = 100;
    public Vector3 oldPositionInCell;
    public int backgroundSRSortingOrder;
    public int mastSRSortingOrder;
    public int valueSRSortingOrder;

    // В одной из 7 ячеек
    public bool inCell7 = false;

    // В одной из 4 ячеек
    public bool inCell4 = false;

    // Раскрыта ли карта (изначально не раскрыта)
    public bool isOpen = false;

    // Текстуры фона карты
    public SpriteRenderer backgroundSR;
    public Sprite openedCardTexture;
    public Sprite closedCardTexture;

    /*
	Цвет - красный
		Червы (♥)
		Бубны (♦)
	Цвет - черный
		Трефы (♣)
		Пики (♠)
    */
    // Масть
    public int mast = 0;
    public SpriteRenderer mastSR;

    // Цвет
    public bool isCardColorRed = false; // true - красный, false - черный

    // Значение - величина
    // 13 величин по возрастанию: 1(Туз), 2...10, 11(Валет), 12(Дама), 13(Король)
    // 52 вида - 13 величин * 4 масти = 52 варианта карт
    public int value = 0;
    public SpriteRenderer valueSR;

    // Старое расположение карты
    Vector3 oldCardPosition;
    Transform oldParent;

    public void ApplySettings()
    {
        // Применить текстуру фона карты
        if (isOpen)
        {
            backgroundSR.sprite = openedCardTexture;
            mastSR.gameObject.SetActive(true);
            valueSR.gameObject.SetActive(true);
        }
        else
        {
            backgroundSR.sprite = closedCardTexture;
            mastSR.gameObject.SetActive(false);
            valueSR.gameObject.SetActive(false);
        }

        // Установить цвет
        if (mast < 3)
        {
            isCardColorRed = true;
        }
        else
        {
            isCardColorRed = false;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Отрисовка карты
        if (!mouseDragging && transform.parent != null && transform.parent.GetComponent<Card>() != null)
        {
            Card parentCard = transform.parent.GetComponent<Card>();
            SpriteRenderer parentCardSR = parentCard.GetComponent<SpriteRenderer>();
            backgroundSR.sortingOrder = parentCardSR.sortingOrder + 1;
            mastSR.sortingOrder = backgroundSR.sortingOrder + 1;
            valueSR.sortingOrder = backgroundSR.sortingOrder + 1;
            parentCard.isFirst = false;
            // print("+1");
        }
    }


    void OnMouseDrag()
    {
        mouseDragging = true;

        if (isOpen && (inCellKolodaHelper || inCell7 || inCell4))
        {
            // Перемещение карты с зажатой ЛКМ
            Vector3 newPosition = Camera.main.ScreenPointToRay(Input.mousePosition).origin + oldCardPosition;
            // print(newPosition.z);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
    }


    void OnMouseDown()
    {
        // Сохранить положение карты
        oldCardPosition = transform.position - Camera.main.ScreenPointToRay(Input.mousePosition).origin;
        // print("mouse down");

        // Раскрыть карту ЛКМ,
        if (!isOpen)
        {
            // если карта в колоде
            if (vKolode)
            {
                // Раскрыть карту
                isOpen = true;
                ApplySettings();

                // Переместить карту из колоды в ячейку рядом с ней
                CellKolodaHelper cellKolodaHelper = GameObject.Find("CellKolodaHelper").GetComponent<CellKolodaHelper>();
                Vector3 newPosition = new Vector3(cellKolodaHelper.transform.position.x, cellKolodaHelper.transform.position.y, -cellKolodaHelper.cardCount);
                transform.position = newPosition;
                backgroundSR.sortingOrder = cellKolodaHelper.cardCount;
                mastSR.sortingOrder = cellKolodaHelper.cardCount + 1;
                valueSR.sortingOrder = cellKolodaHelper.cardCount + 1;
                cellKolodaHelper.cardCount++;

                isFirst = true;
            }
            // если карта в cell7
            if (inCell7 && isFirst)// 
            {
                // Раскрыть карту
                isOpen = true;
                ApplySettings();
            }
        }

        // Сохранить положение карты в ячейке
        if (isOpen && (inCellKolodaHelper || inCell7 || inCell4))
        {
            oldPositionInCell = transform.position;
            oldParent = transform.parent;
            transform.SetParent(null);

            backgroundSRSortingOrder = backgroundSR.sortingOrder;
            mastSRSortingOrder = mastSR.sortingOrder;
            valueSRSortingOrder = valueSR.sortingOrder;

            // Сделать карту видимой поверх всех карт
            backgroundSR.sortingOrder += sortingOrderAdd;
            mastSR.sortingOrder += sortingOrderAdd;
            valueSR.sortingOrder += sortingOrderAdd;
            transform.position = new Vector3(transform.position.x, transform.position.y, -sortingOrderAddZ);
        }
    }
    void OnMouseUp()
    {
        mouseDragging = false;

        // Положить на карту
        if (isOpen && !vKolode)// && (inCellKolodaHelper || inCell7 || inCell4)
        {
            // Временно отключить коллайдер карты
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.enabled = false;

            // После опускания ЛКМ над картой, прикрепить карту к карте под ней
            RaycastHit2D hit;
            hit = Physics2D.Raycast(transform.position + (-oldCardPosition), Vector3.forward);

            // Проверить на допустимость хода
            Card otherCard = null;
            Cell7 cell7 = null;
            Cell4 cell4 = null;

            if (hit.collider != null)
            {
                otherCard = hit.collider.GetComponent<Card>();
                cell7 = hit.collider.GetComponent<Cell7>();
                cell4 = hit.collider.GetComponent<Cell4>();
            }

            // Можно ли перевести карту
            // В саму ячейку inCell7 - Король
            if (cell7 != null // Есть ли cell7
                && cell7.isFirst // Над ячейкой нет других карт?
                && value == 13 // Только король может приземлиться в пустую ячейку cell7
                )
            {
                // print("В саму ячейку inCell7 - Король");
                // Прикрепить карту к этой ячейке
                transform.position = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y, hit.collider.transform.position.z - 1);
                transform.SetParent(null);

                SpriteRenderer cell7SR = cell7.GetComponent<SpriteRenderer>();
                backgroundSR.sortingOrder = cell7SR.sortingOrder + 1;
                mastSR.sortingOrder = backgroundSR.sortingOrder + 1;
                valueSR.sortingOrder = backgroundSR.sortingOrder + 1;
                cell7.isFirst = false;
                isFirst = true;

                inCell4 = false;
                inCellKolodaHelper = false;
                inCell7 = true;
            }
            else
            // В саму ячейку inCell4 - Туз
            if (cell4 != null // Есть ли cell4
                              // && cell4.isFirst // Над ячейкой нет других карт?
                && value == 1 // Только туз может приземлиться в пустую ячейку cell4
                && isFirst
                )
            {
                // print("В саму ячейку inCell4 - Туз");
                // Прикрепить карту к этой ячейке
                transform.position = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y, hit.collider.transform.position.z - 1);
                transform.SetParent(null);

                SpriteRenderer cell4SR = cell4.GetComponent<SpriteRenderer>();
                backgroundSR.sortingOrder = cell4SR.sortingOrder + 1;
                mastSR.sortingOrder = backgroundSR.sortingOrder + 1;
                valueSR.sortingOrder = backgroundSR.sortingOrder + 1;

                cell4.isFirst = false;
                isFirst = true;

                // TODO: Остальные отключить ... сделать enum
                inCell7 = false;
                inCellKolodaHelper = false;
                inCell4 = true;
            }
            else
            // На открытую карту в ячейке inCell7
            if (otherCard != null // Есть ли другая карта
                && otherCard.inCell7 // Другая карта должна находиться в одной из 7 ячеек
                && otherCard.isFirst
                && otherCard.isOpen // Раскрыта ли другая карта
                && otherCard.isCardColorRed != isCardColorRed // Противоположен ли цвет другой карты
                && otherCard.value == value + 1 // Значение другой карты должно быть больше на единицу
                )
            {
                // print("На открытую карту в ячейке inCell7");
                // Прикрепить карту к этой карте
                transform.position = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y - FindObjectOfType<CardGenerator>().cell7YOffsetForOpenCards, hit.collider.transform.position.z - 1);
                transform.SetParent(hit.collider.transform);

                backgroundSR.sortingOrder = otherCard.backgroundSR.sortingOrder + 1;
                mastSR.sortingOrder = otherCard.mastSR.sortingOrder + 1;
                valueSR.sortingOrder = otherCard.valueSR.sortingOrder + 1;

                otherCard.isFirst = false;
                if (transform.childCount < 3)
                {
                    isFirst = true;
                }
        
                inCell4 = false;
                inCellKolodaHelper = false;
                inCell7 = true;
            }
            else
            // На открытую карту в ячейке inCell4
            if (otherCard != null // Есть ли другая карта
                && isFirst
                && otherCard.inCell4 // Другая карта должна находиться в одной из 4 ячеек
                && otherCard.isFirst
                && otherCard.isOpen // Раскрыта ли другая карта
                && otherCard.mast == mast // Масти должны совпадать
                && otherCard.value == value - 1 // Значение другой карты должно быть меньше на единицу
                )
            {
                // print("На открытую карту в ячейке inCell4");
                // Прикрепить карту к этой карте
                transform.position = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y, hit.collider.transform.position.z - 1);
                transform.SetParent(null);

                backgroundSR.sortingOrder = otherCard.backgroundSR.sortingOrder + 1;
                mastSR.sortingOrder = backgroundSR.sortingOrder + 1;
                valueSR.sortingOrder = backgroundSR.sortingOrder + 1;

                otherCard.isFirst = false;
                isFirst = true;

                inCell7 = false;
                inCellKolodaHelper = false;
                inCell4 = true;

            }
            // Вернуть карту на прежнее место 
            else if (!vKolode)
            {
                // print("transform.childCount: "+transform.childCount);
                transform.position = oldPositionInCell;
                transform.SetParent(oldParent);

                backgroundSR.sortingOrder = backgroundSRSortingOrder;
                mastSR.sortingOrder = mastSRSortingOrder;
                valueSR.sortingOrder = valueSRSortingOrder;

                // Обнулить луч попадания
                hit = new RaycastHit2D();
            }

            // Автоматически раскрыть карту под старым местом этой карты
            if (hit.collider != null && (cell7 != null || cell4 != null || otherCard != null))
            {
                // print("Автоматически раскрыть карту под старым местом этой карты");
                RaycastHit2D hitDownCard;
                hitDownCard = Physics2D.Raycast(oldPositionInCell, Vector3.forward);
                Card oldDownCard = null;
                Cell7 cell7Down = null;
                Cell4 cell4Down = null;

                if (hitDownCard.collider != null)
                {
                    oldDownCard = hitDownCard.collider.GetComponent<Card>();
                    cell7Down = hitDownCard.collider.GetComponent<Cell7>();
                    cell4Down = hitDownCard.collider.GetComponent<Cell4>();
                }

                if (oldDownCard != null)
                {
                    // print("oldDownCard: " + oldDownCard.name);
                    oldDownCard.isFirst = true;
                    oldDownCard.isOpen = true;
                    oldDownCard.ApplySettings();
                }
                else
                if (cell7Down != null)
                {
                    cell7Down.isFirst = true;
                }
                else
                if (cell4Down != null)
                {
                    cell4Down.isFirst = true;
                }
            }

            // Включить коллайдер карты
            boxCollider.enabled = true;
        }

        if (vKolode)
        {
            // Уже не в колоде
            vKolode = false;
            inCellKolodaHelper = true;
        }
    }
}
