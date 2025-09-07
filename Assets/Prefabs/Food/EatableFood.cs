using System.Collections;
using UnityEngine;

public class EatableFood : MonoBehaviour, IEatable
{
    [SerializeField] private int nutrition = 20;

    public int GetNutrition() => nutrition;

    public void OnEaten(float delay = 0f)
    {
        string cachedName = name;  // ✅ 缓存 name，避免访问已销毁对象
        Debug.Log($"{cachedName} 被吃掉，{delay} 秒后销毁");
        StartCoroutine(DestroySelf(delay));
    }

    private IEnumerator DestroySelf(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
