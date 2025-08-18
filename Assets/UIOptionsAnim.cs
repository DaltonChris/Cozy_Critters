using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOptionsAnim : MonoBehaviour
{
    public RectTransform panel;
    public float slideDuration = 0.6f;
    public float displayTime = 3f;
    public float bobAmp = 10f;
    public float bobFrequency = 2f;

    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private Coroutine slideRoutine;

    void Start()
    {
        shownPos = panel.anchoredPosition;
        hiddenPos = shownPos + new Vector2(0, 600);
        panel.anchoredPosition = hiddenPos;
    }

    public void ShowPanel()
    {
        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideInOut());
    }

    private IEnumerator SlideInOut()
    {
        yield return StartCoroutine(Slide(panel, hiddenPos, shownPos, slideDuration));
        float elapsed = 0;
        Vector2 basePos = panel.anchoredPosition;
        while (elapsed < displayTime)
        {
            elapsed += Time.deltaTime;
            float offset = Mathf.Sin(Time.time * bobFrequency) * bobAmp;
            panel.anchoredPosition = basePos + new Vector2(0, offset);
            yield return null;
        }

        panel.anchoredPosition = basePos;
        yield return StartCoroutine(Slide(panel, panel.anchoredPosition, hiddenPos, slideDuration));
    }

    private IEnumerator Slide(RectTransform rect, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            rect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        rect.anchoredPosition = to;
    }
}
