using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellKoloda : MonoBehaviour
{
    // Все карты в колоде
    public GameObject[] allCardsInKoloda = new GameObject[52];

    // Спрайт для закончившихся возможностей перебора колоды
    public Sprite endSprite;

    // Количество карт в ячейке
    public int cardCount = 0;

    // Количество попыток
    public int trys = 2;
    public CellKolodaHelper cellKolodaHelper;

    void OnMouseDown()
    {
        // print("CellKoloda - OnMouseDown()");

        if (trys > 0)
        {
            // Пересобрать колоду
            Transform cellKolodaHelperTransform = cellKolodaHelper.transform;
            cardCount = 0;

            while (true)
            {
                RaycastHit2D hit;
                hit = Physics2D.Raycast(cellKolodaHelperTransform.position, Vector3.back);

                if (hit)
                {
                    GameObject cardGO = hit.collider.gameObject;
                    Card card = cardGO.GetComponent<Card>();

                    // Поместить карту поверх другой
                    Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, -cardCount - 1);
                    cardGO.transform.position = newPosition;
                    card.backgroundSR.sortingOrder = cardCount;
                    card.mastSR.sortingOrder = cardCount + 1;
                    card.valueSR.sortingOrder = cardCount + 1;
                    card.isOpen = false;
                    card.ApplySettings();
                    card.vKolode = true;
                    card.isFirst = false;
                    card.inCellKolodaHelper = false;

                    cardCount++;
                    cellKolodaHelper.cardCount--;
                }
                else
                {
                    break;
                }
            }
            trys--;
            if (trys == 0)
            {
                GetComponent<SpriteRenderer>().sprite = endSprite;
            }
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
