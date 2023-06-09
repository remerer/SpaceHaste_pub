using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetBoxGenerator : MonoBehaviour
{
    private Camera mainCam;
    public CheckPoints checkPoints;
    public List<GameObject> checkpointList = new List<GameObject>();
    public List<GameObject> onScreenCheckpoint = new List<GameObject>();
    public List<GameObject> enemyList = new List<GameObject>();
    public List<GameObject> onScreenEnemy = new List<GameObject>();
    public GameObject checkpointPrefab;
    public GameObject warpGatePrefab;
    public GameObject enemyPrefab;

    [SerializeField] private Vector2 minSize = new Vector2(100, 100);
    [SerializeField] private Vector2 sizeMargin = new Vector2(-50, -50);

    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        if (checkPoints != null)
        {
            AddCheckpointTargetBox();
        }
    }

    void FixedUpdate()
    {
        if (checkPoints != null) 
        { 
            UpdateCheckpointBox();
        }
        if (enemyList != null)
        {
            UpdatePlayerBox();
        }
    }

    private void UpdateCheckpointBox()
    {
        int i = checkPoints.idx;
        if (!checkPoints.goalActive)
        {
            bool isInView = IsInScreen(checkpointList[i].transform.position);
            onScreenCheckpoint[i].GetComponent<CanvasGroup>().alpha = isInView ? 1 : 0;
            if (isInView)
            {
                Rect targetRect = GetBoundsInScreenSpace(checkpointList[i], mainCam);
                RectTransform targetRectTransform = onScreenCheckpoint[i].gameObject.GetComponent<RectTransform>();

                targetRectTransform.position = new Vector2(targetRect.center.x, targetRect.center.y);
                targetRectTransform.sizeDelta = new Vector2(Mathf.Max(targetRect.width, minSize.x), Mathf.Max(targetRect.height, minSize.y)) + sizeMargin;
                onScreenCheckpoint[i].SendMessage("UpdateDistance", checkpointList[i].transform.position);
            }
        }
    }

    private void UpdatePlayerBox()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            bool isInView = IsInScreen(enemyList[i].transform.position);
            onScreenEnemy[i].GetComponent<CanvasGroup>().alpha = isInView ? 1 : 0;
            if (isInView)
            {
                Rect targetRect = GetBoundsInScreenSpace(enemyList[i], mainCam);
                RectTransform targetRectTransform = onScreenEnemy[i].gameObject.GetComponent<RectTransform>();

                targetRectTransform.position = new Vector2(targetRect.center.x, targetRect.center.y);
                targetRectTransform.sizeDelta = new Vector2(Mathf.Max(targetRect.width, minSize.x), Mathf.Max(targetRect.height, minSize.y)) + sizeMargin;
                onScreenEnemy[i].SendMessage("UpdateDistance", enemyList[i].transform.position);
            }
        }
    }

    public void ResetTargetBox()
    {
        foreach (GameObject obj in onScreenCheckpoint)
        {
            obj.SetActive(false);
        }
    }

    public void SetNextTargetBox(int i)
    {
        ResetTargetBox();
        onScreenCheckpoint[i].SetActive(true);
    }

    public static Rect GetBoundsInScreenSpace(GameObject targetObj, Camera camera)
    {
        Bounds targetBounds = targetObj.GetComponent<Renderer>().bounds;
        Vector3[] screenSpaceCorners = new Vector3[8];

        screenSpaceCorners[0] = camera.WorldToScreenPoint(new Vector3(targetBounds.center.x + targetBounds.extents.x, targetBounds.center.y + targetBounds.extents.y, targetBounds.center.z + targetBounds.extents.z));
        screenSpaceCorners[1] = camera.WorldToScreenPoint(new Vector3(targetBounds.center.x + targetBounds.extents.x, targetBounds.center.y + targetBounds.extents.y, targetBounds.center.z - targetBounds.extents.z));
        screenSpaceCorners[2] = camera.WorldToScreenPoint(new Vector3(targetBounds.center.x + targetBounds.extents.x, targetBounds.center.y - targetBounds.extents.y, targetBounds.center.z + targetBounds.extents.z));
        screenSpaceCorners[3] = camera.WorldToScreenPoint(new Vector3(targetBounds.center.x + targetBounds.extents.x, targetBounds.center.y - targetBounds.extents.y, targetBounds.center.z - targetBounds.extents.z));
        screenSpaceCorners[4] = camera.WorldToScreenPoint(new Vector3(targetBounds.center.x - targetBounds.extents.x, targetBounds.center.y + targetBounds.extents.y, targetBounds.center.z + targetBounds.extents.z));
        screenSpaceCorners[5] = camera.WorldToScreenPoint(new Vector3(targetBounds.center.x - targetBounds.extents.x, targetBounds.center.y + targetBounds.extents.y, targetBounds.center.z - targetBounds.extents.z));
        screenSpaceCorners[6] = camera.WorldToScreenPoint(new Vector3(targetBounds.center.x - targetBounds.extents.x, targetBounds.center.y - targetBounds.extents.y, targetBounds.center.z + targetBounds.extents.z));
        screenSpaceCorners[7] = camera.WorldToScreenPoint(new Vector3(targetBounds.center.x - targetBounds.extents.x, targetBounds.center.y - targetBounds.extents.y, targetBounds.center.z - targetBounds.extents.z));

        float min_x = screenSpaceCorners[0].x;
        float min_y = screenSpaceCorners[0].y;
        float max_x = screenSpaceCorners[0].x;
        float max_y = screenSpaceCorners[0].y;

        for (int i = 1; i < 8; i++)
        {
            if (screenSpaceCorners[i].x < min_x)
            {
                min_x = screenSpaceCorners[i].x;
            }
            if (screenSpaceCorners[i].y < min_y)
            {
                min_y = screenSpaceCorners[i].y;
            }
            if (screenSpaceCorners[i].x > max_x)
            {
                max_x = screenSpaceCorners[i].x;
            }
            if (screenSpaceCorners[i].y > max_y)
            {
                max_y = screenSpaceCorners[i].y;
            }
        }
        return Rect.MinMaxRect(min_x, min_y, max_x, max_y);
    }

    public bool IsInScreen(Vector3 targetPos)
    {
        Vector3 viewportPos = mainCam.WorldToViewportPoint(targetPos);
        if ((0 < viewportPos.x && viewportPos.x < 1) && (0 < viewportPos.y && viewportPos.y < 1) && 0 < viewportPos.z)
        {
            return true;
        }
        return false;
    }

    public void AddCheckpointTargetBox()
    {
        foreach (GameObject checkPoint in checkPoints.checkPointArr)
        {
            checkpointList.Add(checkPoint);
        }
        for (int i = 0; i < checkpointList.Count; i++)
        {
            if (checkpointList[i].name == "HyPeRdRiVe")
            {
                onScreenCheckpoint.Add(Instantiate(warpGatePrefab));
            }
            else
            {
                onScreenCheckpoint.Add(Instantiate(checkpointPrefab));
            }
            onScreenCheckpoint[i].transform.parent = transform;
            onScreenCheckpoint[i].SendMessage("SetName", checkpointList[i].gameObject.name);
        }
        SetNextTargetBox(0);
    }

    public void AddPlayerTargetBox(GameObject targetObj)
    {
        enemyList.Add(targetObj);
        onScreenEnemy.Add(Instantiate(enemyPrefab));
        for (int i = 0; i < enemyList.Count; i++)
        {
            onScreenEnemy[i].transform.parent = transform;
            onScreenEnemy[i].SendMessage("SetName", enemyList[i].GetComponent<NetworkPlayer>().UserId);
        }
    }
}
