using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public static FieldManager inst;

    public bool isBlocked;

    public Transform[,] gridPoints;
    public Element[,] field;

    public Element firstSelected;

    [SerializeField] Transform[] allPoints;
    [SerializeField] Transform elementsRoot;
    [SerializeField] Element elementPrefab;

    List<Element> confirmedList;

    Element secondSelected;

    int totalScore;
    int lines;
    int boosts;

    void Awake()
    {
        inst = this;

        confirmedList = new List<Element>();

        var pointIndex = 0;

        field = new Element[6, 6];
        gridPoints = new Transform[6, 6];

        var holes = new Vector2Int[3];

        for (int i = 0; i < 3; i++)
            holes[i] = new Vector2Int(Random.Range(0, 6), Random.Range(0, 6));

        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
            {
                gridPoints[x, y] = allPoints[pointIndex];

                field[x, y] = Instantiate(elementPrefab, allPoints[pointIndex++].position, Quaternion.identity, elementsRoot);
                field[x, y].InitializeColorData(GetRandomColorData());
                field[x, y].InitializeArrayIndex(new Vector2Int(x, y));
            }

        CheckGeneratedElements();
    }

    public IEnumerator HandleElement(Element element)
    {
        if (!firstSelected)
            firstSelected = element;
        else
        {
            if (firstSelected == element)
            {
                element.ResetColor();
                firstSelected = null;

                yield break;
            }

            if ((element.arrayIndex.x == firstSelected.arrayIndex.x && (element.arrayIndex.y == firstSelected.arrayIndex.y - 1 || element.arrayIndex.y == firstSelected.arrayIndex.y + 1)) ||
               (element.arrayIndex.y == firstSelected.arrayIndex.y && (element.arrayIndex.x == firstSelected.arrayIndex.x + 1 || element.arrayIndex.x == firstSelected.arrayIndex.x - 1)))
            {
                secondSelected = element;

                yield return StartCoroutine(SwapElements(firstSelected, element));

                isBlocked = true;

                yield return StartCoroutine(UpdateField());

                firstSelected = null;
                secondSelected = null;
            }
            else
            {
                firstSelected.ResetColor();
                firstSelected = element;
            }
        }
    }

    IEnumerator SwapElements(Element first, Element second)
    {
        isBlocked = true;

        var firstIndex = first.arrayIndex;
        var secondIndex = second.arrayIndex;

        field[firstIndex.x, firstIndex.y] = second;
        field[secondIndex.x, secondIndex.y] = first;

        first.InitializeArrayIndex(secondIndex);
        second.InitializeArrayIndex(firstIndex);

        first.ResetColor();
        second.ResetColor();

        var firstElementPos = first.transform.position;
        var secondElementPos = second.transform.position;

        while (Vector3.Distance(first.transform.position, secondElementPos) > GameSettings.inst.distanceThreshold)
        {
            first.transform.position = Vector3.Lerp(first.transform.position, secondElementPos, GameSettings.inst.elementsSwappingSpeed * Time.deltaTime);
            second.transform.position = Vector3.Lerp(second.transform.position, firstElementPos, GameSettings.inst.elementsSwappingSpeed * Time.deltaTime);
            yield return null;
        }

        first.transform.position = secondElementPos;
        second.transform.position = firstElementPos;

        isBlocked = false;
    }

    IEnumerator UpdateField()
    {
        confirmedList.Clear();

        lines = 0;
        boosts = 0;

        CheckColumns();
        CheckRows();

        if (confirmedList.Count > 0)
        {
            firstSelected = null;

            yield return new WaitForSeconds(GameSettings.inst.delayBeforeDestroying);
            DestroyMatches();

            totalScore += (lines * 10) + (boosts * GameSettings.inst.additionalScoreAmount);
            ScoreManager.inst.SetScore(totalScore);

            yield return new WaitForSeconds(GameSettings.inst.delayBeforeMovingUp);
            MoveUpElements();

            yield return new WaitForSeconds(GameSettings.inst.delayBeforeFilling);
            FillEmptySpaces();

            StartCoroutine(UpdateField());

            yield break;
        }
        else if (firstSelected)
            StartCoroutine(SwapElements(firstSelected, secondSelected));
 
        isBlocked = false;
    }

    void DestroyMatches()
    {
        for (int i = 0; i < confirmedList.Count; i++)
        {
            field[confirmedList[i].arrayIndex.x, confirmedList[i].arrayIndex.y] = null;
            confirmedList[i].DestroyElement();
        }
    }

    void MoveUpElements()
    {
        for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
                if (field[x, y] == null)
                {
                    for (int i = y; i < 6; i++)
                    {
                        if (field[x, i] != null)
                        {
                            field[x, i].MoveUp(y, true);
                            break;
                        }
                    }
                    break;
                }

    }

    void FillEmptySpaces()
    {
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
                if (field[x, y] == null)
                {
                    var element = Instantiate(elementPrefab, gridPoints[x, y].position, Quaternion.identity, elementsRoot);
                    element.InitializeArrayIndex(new Vector2Int(x, y));
                    element.InitializeColorData(GetRandomColorData());

                    field[x, y] = element;
                }
    }

    void CheckRows()
    {
        ColorData colorData;

        var tempList = new List<Element>();

        var matches = 0;

        for (int i = 0; i < 6; i++)
        {
            colorData = new ColorData() { index = -1 };
            for (int j = 0; j < 6; j++)
            {
                if (colorData.index != field[i, j].colorData.index)
                {
                    matches = 0;

                    colorData = field[i, j].colorData;

                    tempList.Clear();

                    tempList.Add(field[i, j]);
                }
                else
                {
                    matches++;

                    tempList.Add(field[i, j]);

                    if (matches >= 2)
                    {
                        for (int k = 0; k < tempList.Count; k++)
                            if (!confirmedList.Contains(tempList[k]))
                                confirmedList.Add(tempList[k]);

                        if (matches == 2)
                            lines++;
                        else
                            boosts++;
                    }

                }
            }
        }
    }

    void CheckColumns()
    {
        ColorData colorData;

        var tempList = new List<Element>();

        var matches = 0;

        for (int i = 0; i < 6; i++)
        {
            colorData = new ColorData() { index = -1 };
            for (int j = 0; j < 6; j++)
            {
                if (colorData.index != field[j, i].colorData.index)
                {
                    matches = 0;

                    colorData = field[j, i].colorData;

                    tempList.Clear();

                    tempList.Add(field[j, i]);
                }
                else
                {
                    matches++;

                    tempList.Add(field[j, i]);

                    if (matches >= 2)
                    {
                        for (int k = 0; k < tempList.Count; k++)
                            if (!confirmedList.Contains(tempList[k]))
                                confirmedList.Add(tempList[k]);

                        if (matches == 2)
                            lines++;
                        else
                            boosts++;
                    }
                }
            }

        }
    }

    void CheckFieldPlayability()
    {
        //No needed yet
    }

    void CheckGeneratedElements()
    {
        var checkAgain = false;

        for (int i = 1; i < 5; i++)
            for (int j = 0; j < 6; j++)
                if ((field[i, j].colorData.index == field[i + 1, j].colorData.index && field[i, j].colorData.index == field[i - 1, j].colorData.index))
                {
                    field[i, j].InitializeColorData(GetRandomColorData());
                    checkAgain = true;
                }

        for (int i = 0; i < 6; i++)
            for (int j = 1; j < 5; j++)
                if ((field[i, j].colorData.index == field[i, j + 1].colorData.index && field[i, j].colorData.index == field[i, j - 1].colorData.index))
                {
                    field[i, j].InitializeColorData(GetRandomColorData());
                    checkAgain = true;
                }


        if (checkAgain)
            CheckGeneratedElements();
    }

    ColorData GetRandomColorData()
    {
        return GameSettings.inst.colorData[Random.Range(0, GameSettings.inst.colorData.Count)];
    }
}