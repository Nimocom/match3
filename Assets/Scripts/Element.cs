using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Element : MonoBehaviour
{
    public ColorData colorData;

    public Vector2Int arrayIndex;

    public Image Image;

    Coroutine lerpColor;
    Coroutine lerpPosition;

    void Awake()
    {
        Image = GetComponent<Image>();
    }

    public void HandleClick()
    {
        if (FieldManager.inst.isBlocked)
            return;

        if (lerpColor != null)
            StopCoroutine(lerpColor);

        Image.color = Color.red;

        StartCoroutine(FieldManager.inst.HandleElement(this));
    }

    public void InitializeArrayIndex(Vector2Int arrayIndex)
    {
        this.arrayIndex = arrayIndex;
    }

    public void InitializeColorData(ColorData colorData)
    {
        this.colorData = colorData;
        Image.color = colorData.color;
    }

    public void ResetColor()
    {
        if (lerpColor != null)
            StopCoroutine(lerpColor);

        lerpColor = StartCoroutine(LerpColor());
    }

    public void DestroyElement()
    {
        StopAllCoroutines();
        StartCoroutine(LerpScale());
    }

    public void MoveUp(int startIndex, bool calculateSteps = false)
    {
        var steps = calculateSteps ? arrayIndex.y - startIndex : startIndex;

        var targetPosition = transform.localPosition;
        targetPosition.y += (160f * steps);

        FieldManager.inst.field[arrayIndex.x, arrayIndex.y] = null;

        if (arrayIndex.y + 1 < 6 && FieldManager.inst.field[arrayIndex.x, arrayIndex.y + 1] != null)
            FieldManager.inst.field[arrayIndex.x, arrayIndex.y + 1].MoveUp(steps);

        arrayIndex.y -= steps;

        FieldManager.inst.field[arrayIndex.x, arrayIndex.y] = this;

        if (lerpPosition != null)
            StopCoroutine(lerpPosition);

        lerpPosition = StartCoroutine(LerpPosition(targetPosition));
    }

    IEnumerator LerpPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.localPosition, targetPosition) > GameSettings.inst.distanceThreshold)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, GameSettings.inst.elementMovementSpeed * Time.deltaTime);
            yield return null;
        }

        transform.localPosition = targetPosition;
    }

    IEnumerator LerpScale()
    {
        while (Vector3.Distance(transform.localScale, Vector3.zero) > GameSettings.inst.distanceThreshold)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, GameSettings.inst.elementScalingSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator LerpColor()
    {
        while (Image.color != colorData.color)
        {
            Image.color = Color.Lerp(Image.color, colorData.color, GameSettings.inst.elementColorChangingSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
