using UnityEngine;

public class HUD : MonoBehaviour
{
    public RectTransform cursorRectTransform;
    public RectTransform lineRectTransform;
    public float radius;

    private Vector3 screenCenter;
    private Vector3 relativeScreenCenter;

    private void Start()
    {
        Cursor.visible = false;
        screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);
        relativeScreenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);
    }

    void Update()
    {
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        float distanceFromCenter = Vector2.Distance(relativeScreenCenter, Input.mousePosition);
        Vector3 direction = Input.mousePosition - relativeScreenCenter;

        if (distanceFromCenter < radius)
        {
            cursorRectTransform.position = screenCenter + direction;
        }
        else if (distanceFromCenter >= radius)
        {
            cursorRectTransform.position = screenCenter + (direction.normalized * radius);
            relativeScreenCenter += (distanceFromCenter - radius) * (direction.normalized);
        }

        lineRectTransform.position = 0.5f * screenCenter + 0.5f * cursorRectTransform.position;
        lineRectTransform.LookAt(cursorRectTransform, Vector3.Cross(cursorRectTransform.up, (cursorRectTransform.position - lineRectTransform.position).normalized));

        if (lineRectTransform.anchoredPosition.x < 0f)
        {
            lineRectTransform.rotation = Quaternion.Euler(0f, 0f, lineRectTransform.rotation.eulerAngles.x);
        }
        else
        {
            lineRectTransform.rotation = Quaternion.Euler(0f, 180f, lineRectTransform.rotation.eulerAngles.x);
        }

        lineRectTransform.sizeDelta = new Vector2((cursorRectTransform.localPosition - lineRectTransform.localPosition).magnitude * 2 * (1 / lineRectTransform.localScale.x), lineRectTransform.sizeDelta.y);
    }
}