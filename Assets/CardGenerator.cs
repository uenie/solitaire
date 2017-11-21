using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CardGenerator : MonoBehaviour
{
    // Префаб ячейки для 7 ячеек
    public GameObject cell7Prefab;
    public float cell7XOffset = 3f;
    public float cell7YOffsetForClosedCards = 0.2f;
    public float cell7YOffsetForOpenCards = 0.5f;
    public GameObject[] allCell7 = new GameObject[7];

    // Префаб ячейки для 4 ячеек
    public GameObject cell4Prefab;
    public float cell4XOffset = 3f;
    public GameObject[] allCell4 = new GameObject[4];

    // Колода
    public CellKoloda cellKoloda;

    // Все 52 карты
    public GameObject[] allCards = new GameObject[52];

    // Коэффициенты расстояния между картами
    public float offsetX = 3;
    public float offsetY = 4;

    // Префаб карты
    public GameObject cardPrefab;

    // Масть: Червы (♥), Бубны (♦), Трефы (♣), Пики (♠)
    public Sprite[] mastSprites = new Sprite[4];

    // Значение - величина
    // 13 величин по возрастанию: 1(Туз), 2...10, 11(Валет), 12(Дама), 13(Король)
    // 52 вида - 13 величин * 4 масти = 52 варианта карт
    public Sprite[] cardValueTexturesBlack = new Sprite[13];
    public Sprite[] cardValueTexturesRed = new Sprite[13];

    // Use this for initialization
    void Start()
    {
        // Индекс для массива карт
        int allCardsIndex = 0;

        // Создать все карты
        for (int iMast = 0; iMast <= 3; iMast++)
        {
            for (int iValue = 0; iValue <= 12; iValue++)
            {
                // Колода
                // Создать карту
                Vector3 newPosition = new Vector3(cellKoloda.transform.position.x, cellKoloda.transform.position.y, -cellKoloda.cardCount);
                GameObject cardGO = Instantiate(cardPrefab, newPosition, Quaternion.identity);
                Card card = cardGO.GetComponent<Card>();
                card.isOpen = false;

                // Применить текстуру масти
                card.mast = iMast + 1;
                card.mastSR.sprite = mastSprites[iMast];

                card.ApplySettings();

                // Применить текстуру значения
                card.value = iValue + 1;
                if (card.isCardColorRed)
                {
                    card.valueSR.sprite = cardValueTexturesRed[iValue];
                }
                else
                {
                    card.valueSR.sprite = cardValueTexturesBlack[iValue];
                }

                // Переименовать карту
                card.name = "Card (" + card.value + ", " + (card.isCardColorRed ? "Red" : "Black") + " (" + card.mastSR.sprite.name + "))";

                // Добавить карту в массив карт колоды
                allCards[allCardsIndex] = card.gameObject;
                allCardsIndex++;
            }
        }

        // Перемешать карты
        int[] allCardsIndexes = new int[52];
        for (int i = 0; i < allCardsIndexes.Length; i++)
        {
            allCardsIndexes[i] = i;
            //print(allCardsIndexes[i]);
        }
        for (int i = 0; i < allCardsIndexes.Length; i++)
        {
            int index = allCardsIndexes[i];
            int randomIndex = UnityEngine.Random.Range(0, allCardsIndexes.Length - 1);
            allCardsIndexes[i] = allCardsIndexes[randomIndex];
            allCardsIndexes[randomIndex] = index;
            // print(allCardsIndexes[i]);
        }

        // Колода
        for (int i = allCards.Length - 1; i >= 0; i--)
        {
            GameObject cardGO = allCards[allCardsIndexes[i]];
            Card card = cardGO.GetComponent<Card>();
            cellKoloda.allCardsInKoloda[i] = cardGO;

            // Поместить карту поверх другой
            Vector3 newPosition = new Vector3(cellKoloda.transform.position.x, cellKoloda.transform.position.y, -cellKoloda.cardCount - 1);
            cardGO.transform.position = newPosition;
            card.backgroundSR.sortingOrder = cellKoloda.cardCount;
            card.mastSR.sortingOrder = cellKoloda.cardCount + 1;
            card.valueSR.sortingOrder = cellKoloda.cardCount + 1;

            cellKoloda.cardCount++;
        }

        allCardsIndex = 0;

        // Создание 7 ячеек
        for (int iCell7 = 0; iCell7 < 7; iCell7++)
        {
            Vector3 newPositionForCell7 = new Vector3(cell7Prefab.transform.position.x + iCell7 * cell7XOffset, cell7Prefab.transform.position.y, cell7Prefab.transform.position.z);
            GameObject cell7GO = Instantiate(cell7Prefab, newPositionForCell7, Quaternion.identity);
            allCell7[iCell7] = cell7GO;

            // Заполнение 7 ячеек картами
            for (int iCell7Cards = 0; iCell7Cards < iCell7 + 1; iCell7Cards++)
            {
                // Переместить карту из колоды в ячейку
                Card card = cellKoloda.allCardsInKoloda[allCardsIndex].GetComponent<Card>();

                // Ячейка
                Cell7 cell7 = cell7GO.GetComponent<Cell7>();

                // Поместить карту поверх другой
                card.backgroundSR.sortingOrder = cell7.cardCount;
                card.mastSR.sortingOrder = cell7.cardCount + 1;
                card.valueSR.sortingOrder = cell7.cardCount + 1;
                cell7.cardCount++;
                card.inCell7 = true;

                // Уже не в колоде
                card.vKolode = false;

                // Раскрыть карту 
                Vector3 newPositionForCard;
                if (iCell7Cards == iCell7)
                {
                    card.isOpen = true;
                    card.isFirst = true;
                    card.ApplySettings();

                    newPositionForCard = new Vector3(cell7.transform.position.x, cell7.transform.position.y - iCell7Cards * cell7YOffsetForClosedCards, -cell7.cardCount);
                }
                else
                {
                    newPositionForCard = new Vector3(cell7.transform.position.x, cell7.transform.position.y - iCell7Cards * cell7YOffsetForClosedCards, -cell7.cardCount);
                }

                card.transform.position = newPositionForCard;
                allCardsIndex++;
            }
        }

        // Создание 4 ячеек
        for (int iCell4 = 0; iCell4 < 4; iCell4++)
        {
            Vector3 newPositionForCell4 = new Vector3(cell4Prefab.transform.position.x + iCell4 * cell4XOffset, cell4Prefab.transform.position.y, cell4Prefab.transform.position.z);
            GameObject cell4GO = Instantiate(cell4Prefab, newPositionForCell4, Quaternion.identity);
            allCell4[iCell4] = cell4GO;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Перезапустить игру
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneManager.LoadScene(0);
        }
    }

    void OnGUI()
    {
        // Menu
        if (GUI.Button(new Rect(10, 10, 100, 30), "Restart"))
        {
            SceneManager.LoadScene(0);
        }
        if (GUI.Button(new Rect(10, 10 + 35, 100, 30), "Exit"))
        {
            Application.Quit();
        }

        // Проверка на победу
        bool win = true;
        foreach (GameObject cardGO in allCards)
        {
            if (!cardGO.GetComponent<Card>().isOpen)
            {
                win = false;
            }
        }
        if (win)
        {
            int winW = 250, winH = 50;
            int x = Screen.width / 2 - winW / 2, y = Screen.height / 2 - winH / 2;
            GUI.Box(new Rect(x, y, winW, winH), "Win!");
            // print("win - " + Screen.width + " - " + Screen.height);
            // print("win - " + x + " - " + y);
        }
    }
}
