using UnityEngine;

public class EatFruitEffect : MonoBehaviour
{
    public GameObject eatQuad;       // assign your quad
    public Material eatFruitMat;     // assign your eat fruit material
    public float effectDuration = 1.5f;

    private float timer = 0f;
    private bool active = false;

    void Start()
    {
        if (eatQuad != null)
            eatQuad.SetActive(false); // hide at start
    }

    public void TriggerEffect()
    {
        if (eatQuad == null) return;

        timer = 0f;
        active = true;
        eatQuad.SetActive(true); // show quad
    }

    void Update()
    {
        if (!active) return;

        timer += Time.deltaTime;
        eatFruitMat.SetFloat("_TimeSinceEat", timer);

        if (timer >= effectDuration)
        {
            active = false;
            eatQuad.SetActive(false); // hide quad
            eatFruitMat.SetFloat("_TimeSinceEat", 0f); // reset shader
        }
    }
}
