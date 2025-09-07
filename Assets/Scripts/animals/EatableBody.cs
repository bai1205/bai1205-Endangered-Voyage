using System.Collections;
using UnityEngine;

public class EatableBody : MonoBehaviour, IEatable
{
    [SerializeField] private int nutrition = 100;

    public int GetNutrition() => nutrition;

    public void OnEaten(float delay = 0f)
    {
        string cachedName = name;  // ✅ 缓存 name
        Debug.Log($"{cachedName} 被吃掉，{delay} 秒后销毁");
        StartCoroutine(DestroySelf(delay));
    }

    private IEnumerator DestroySelf(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
