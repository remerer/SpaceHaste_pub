using Jane.Unity;
using Jane.Unity.ServerShared.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankManager : MonoBehaviour
{
    public static RankManager instance;
    public StandingsGenerator standingsGenerator;
    public StandingsGenerator resultsGenerator;
    public TargetBoxGenerator targetBoxGenerator;
    public CheckPoints checkPoints;

    [HideInInspector] public int finishCount = 0;
    [SerializeField] private HUDManager hudManager;
    private Dictionary<string, NetworkPlayer> players = new();
    private NetworkPlayer playerId;
    private List<KeyValuePair<string, NetworkPlayer>> sortedList = new();
    private bool isUpdating = false;
    private float switchTime = 0.15f;

    private void Awake() => instance = this;
    void Update()
    {
        if (playerId != null)
        {
            if (!isUpdating && !playerId.IsFinished && GameInfo.GameState == GameState.Playing)
            {
                SetStandings(standingsGenerator);
            }
            else if (!isUpdating && playerId.IsFinished && GameInfo.GameState == GameState.Playing)
            {
                SetResultStandings(resultsGenerator);
            }
        }
        SetRank();
    }

    public void GetLocalPlayer(NetworkPlayer currentLocalPlayer)
    {
        playerId = currentLocalPlayer;
    }

    public void GetPlayers(NetworkPlayer id)
    {
        // Get players through playerID and add them to List
        players.Add(id.UserId, id);
        standingsGenerator.AddPlayerStanding();
        resultsGenerator.AddPlayerStanding();
        if (id.UniqueId != playerId.UniqueId)
        {
            targetBoxGenerator.AddPlayerTargetBox(id.gameObject);
        }
    }

    public void SetStandings(StandingsGenerator standings)
    {
        IOrderedEnumerable<KeyValuePair<string, NetworkPlayer>> sortedPlayer = players.Where(x => !x.Value.IsFinished)
                                                                                      .OrderByDescending(x => x.Value.activeCheckpointIndex)
                                                                                      .ThenBy(x => x.Value.distanceToCheckpoint);
        List<KeyValuePair<string, NetworkPlayer>> tempList = sortedPlayer.ToList();

        foreach (KeyValuePair<string, NetworkPlayer> item in sortedPlayer)
        {
            Debug.Log(item.Key);
        }
        if (sortedList.Any() && !sortedList.SequenceEqual(tempList) && tempList.Count == sortedList.Count)
        {
            List<int?> differentPositions = sortedList.Zip(tempList, (x, y) => x.Equals(y) ? (int?)null : Array.IndexOf(tempList.ToArray(), x)).ToList();
            differentPositions = differentPositions.Where(x => x != null).ToList();
            int change1 = differentPositions.First() ?? 0;
            int change2 = differentPositions.Last() ?? 0;
            change1 += finishCount;
            change2 += finishCount;
            int i = finishCount;

            foreach (KeyValuePair<string, NetworkPlayer> item in sortedPlayer)
            {
                if (i == change1)
                {
                    isUpdating = true;
                    GameObject tempBox = standings.standingsBox[change1];
                    standings.standingsBox[change1] = standings.standingsBox[change2];
                    standings.standingsBox[change2] = tempBox;

                    RectTransform first = standings.standingsBox[change1].GetComponent<RectTransform>();
                    RectTransform second = tempBox.GetComponent<RectTransform>();
                    StartCoroutine(MoveStandings(first, second, sortedPlayer, switchTime, standings));
                }
                i++;
            }
        }
        else if (!sortedList.Any())
        {
            int i = finishCount;
            foreach (KeyValuePair<string, NetworkPlayer> item in sortedPlayer)
            {
                if (item.Value.UniqueId == playerId.UniqueId)
                {
                    standings.standingsBox[i].GetComponent<Image>().color = new Color(1.0f, 0.5f, 0f, 1.0f);
                }
                else
                {
                    standings.standingsBox[i].GetComponent<Image>().color = new Color(0f, 0f, 0f, 1.0f);
                }
                standings.standingsBox[i].GetComponentInChildren<TMP_Text>().text = "   " + (i + 1) + "   " + item.Key;
                i++;
            }
        }
        sortedList = tempList;
    }

    private void SetRank()
    {
        int currentRank = finishCount + 1;
        int totalRank = players.Count;
        hudManager.totalRankText.text = "/" + totalRank.ToString();

        foreach (KeyValuePair<string, NetworkPlayer> player in sortedList)
        {
            if (player.Value.UniqueId == playerId.UniqueId)
            {
                hudManager.currentRankText.text = currentRank.ToString();
            }
            currentRank++;
        }
    }

    public void SetResult(long tick)
    {
        hudManager.hudCanvas.enabled = false;
        hudManager.standingsCanvas.enabled = false;
        hudManager.targetCanvas.enabled = false;
        hudManager.resultCanvas.enabled = true;
        int i = 0;
        TimeSpan time = TimeSpan.FromTicks(tick);
        foreach (KeyValuePair<string, NetworkPlayer> item in sortedList)
        {
            if (item.Value == playerId)
            {
                hudManager.resultRankText.text = "" + (i + 1);
                hudManager.resultTimeText.text = $"{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
            }
            i++;
        }
    }

    public void SetResultStandings(StandingsGenerator standings)
    {
        IOrderedEnumerable<KeyValuePair<string, NetworkPlayer>> sortedPlayer = players.Where(x => !x.Value.IsFinished)
                                                                              .OrderByDescending(x => x.Value.activeCheckpointIndex)
                                                                              .ThenBy(x => x.Value.distanceToCheckpoint);
        List<KeyValuePair<string, NetworkPlayer>> tempList = sortedPlayer.ToList();

        if (tempList.Any() && tempList.Count != sortedList.Count)
        {
            int i = finishCount - 1;
            foreach (KeyValuePair<string, NetworkPlayer> item in sortedList)
            {
                if (item.Value.UniqueId == playerId.UniqueId)
                {
                    standings.standingsBox[i].GetComponent<ResultBox>().ChangeColor(new Color(1.0f, 0.5f, 0f, 1.0f));
                }
                else
                {
                    standings.standingsBox[i].GetComponent<ResultBox>().ChangeColor(new Color(0f, 0f, 0f, 1.0f));
                }
                standings.standingsBox[i].GetComponent<ResultBox>().ChangeRank((i + 1).ToString());
                standings.standingsBox[i].GetComponent<ResultBox>().ChangeStandings(" " + item.Key);
                standings.standingsBox[i].GetComponent<ResultBox>().RecordTime(GameInfo.GameTime);
                i++;
            }
        }
        else if (tempList.Any() && !sortedList.SequenceEqual(tempList) && tempList.Count == sortedList.Count)
        {
            List<int?> differentPositions = sortedList.Zip(tempList, (x, y) => x.Equals(y) ? (int?)null : Array.IndexOf(tempList.ToArray(), x)).ToList();
            differentPositions = differentPositions.Where(x => x != null).ToList();
            int change1 = differentPositions.First() ?? 0;
            int change2 = differentPositions.Last() ?? 0;
            change1 += finishCount;
            change2 += finishCount;
            int i = finishCount;

            foreach (KeyValuePair<string, NetworkPlayer> item in sortedPlayer)
            {
                if (i == change1)
                {
                    isUpdating = true;
                    GameObject tempBox = standings.standingsBox[change1];
                    standings.standingsBox[change1] = standings.standingsBox[change2];
                    standings.standingsBox[change2] = tempBox;

                    RectTransform first = standings.standingsBox[change1].GetComponent<RectTransform>();
                    RectTransform second = tempBox.GetComponent<RectTransform>();
                    StartCoroutine(MoveStandings(first, second, sortedPlayer, switchTime, standings));
                }
                i++;
            }
        }
        else if (tempList.Any() && sortedList.SequenceEqual(tempList) && tempList.Count == sortedList.Count)
        {
            int i = finishCount;
            foreach (KeyValuePair<string, NetworkPlayer> item in sortedList)
            {
                standings.standingsBox[i].GetComponent<ResultBox>().RecordTime(GameInfo.GameTime);
                i++;
            };
        }
        else if (!tempList.Any())
        {
            ;
        }
        sortedList = tempList;
    }

    public float GetDistance(GameObject playerObj, GameObject checkpointObj)
    {
        Vector3 playerLocation = playerObj.transform.position;
        Vector3 checkpointLocation = checkpointObj.transform.position;
        float distance = Vector3.Distance(playerLocation, checkpointLocation);
        return distance;
    }

    IEnumerator MoveStandings(RectTransform rectTransformA, RectTransform rectTransformB, IOrderedEnumerable<KeyValuePair<string, NetworkPlayer>> sorted, float duration, StandingsGenerator standings)
    {
        Vector2 startPosA = rectTransformA.anchoredPosition;
        Vector2 endPosA = rectTransformB.anchoredPosition;
        Vector2 startPosB = rectTransformB.anchoredPosition;
        Vector2 endPosB = rectTransformA.anchoredPosition;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = Mathf.Clamp01(elapsedTime / duration);
            rectTransformA.anchoredPosition = Vector2.Lerp(startPosA, endPosA, t);
            rectTransformB.anchoredPosition = Vector2.Lerp(startPosB, endPosB, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        int i = finishCount;
        if (standings == standingsGenerator)
        {
            foreach (KeyValuePair<string, NetworkPlayer> item in sorted)
            {
                if (item.Value.UniqueId == playerId.UniqueId)
                {
                    standings.standingsBox[i].GetComponent<Image>().color = new Color(1.0f, 0.5f, 0f, 1.0f);
                }
                else
                {
                    standings.standingsBox[i].GetComponent<Image>().color = new Color(0f, 0f, 0f, 1.0f);
                }
                standings.standingsBox[i].GetComponentInChildren<TMP_Text>().text = "   " + (i + 1) + "   " + item.Key;
                i++;
            }
        }
        else if (standings == resultsGenerator)
        {
            foreach (KeyValuePair<string, NetworkPlayer> item in sortedList)
            {
                if (item.Value.UniqueId == playerId.UniqueId)
                {
                    standings.standingsBox[i].GetComponent<ResultBox>().ChangeColor(new Color(1.0f, 0.5f, 0f, 1.0f));
                }
                else
                {
                    standings.standingsBox[i].GetComponent<ResultBox>().ChangeColor(new Color(0f, 0f, 0f, 1.0f));
                }
                standings.standingsBox[i].GetComponent<ResultBox>().ChangeRank((i + 1).ToString());
                standings.standingsBox[i].GetComponent<ResultBox>().ChangeStandings(" " + item.Key);
                standings.standingsBox[i].GetComponent<ResultBox>().RecordTime(GameInfo.GameTime);
                i++;
            }
        }
        isUpdating = false;
        rectTransformA.anchoredPosition = endPosA;
        rectTransformB.anchoredPosition = endPosB;
    }
}