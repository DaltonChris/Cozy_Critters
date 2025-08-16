using UnityEngine;

public class Fruit : MonoBehaviour
{
    public float eatDistance = 4f;

    private void OnTriggerEnter(Collider other)
    {
        SimpleWanderAI ai = other.GetComponent<SimpleWanderAI>();
        if (ai != null)
        {
            ai.SetTargetFruit(this); // tell AI about this fruit
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SimpleWanderAI ai = other.GetComponent<SimpleWanderAI>();
        if (ai != null)
        {
            ai.ClearTargetFruit(this); // AI stops caring about this fruit if it leaves range
        }
    }

    public void Eat(SimpleWanderAI eater)
    {
        eater.EatFruit();
        Destroy(gameObject); // remove fruit
    }
}
