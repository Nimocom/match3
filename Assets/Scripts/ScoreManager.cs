using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager inst;


    [SerializeField] Text scoreValue;

    Coroutine scaleCoroutine;

    void Awake()
    {
        inst = this;

        scaleCoroutine = StartCoroutine(LerpScale());
    }

    public void SetScore(int score)
    {
        scoreValue.text = score.ToString();

        StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(LerpScale());
    }

    IEnumerator LerpScale()
    {
       scoreValue.transform.localScale = Vector3.one * GameSettings.inst.scoreTextScalingMlt;

        while (Vector3.Distance(scoreValue.transform.localScale, Vector3.one) > 0.1f)
        {
            scoreValue.transform.localScale = Vector3.Lerp(scoreValue.transform.localScale, Vector3.one, GameSettings.inst.scoreTextScalingSpeed * Time.deltaTime);
            yield return null;
        }

        scoreValue.transform.localScale = Vector3.one;
    }
}
