using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestPanelController : MonoBehaviour
{
    public Transform content;          
    public TaskUIController taskItemPrefab;

    private readonly Dictionary<string, TaskUIController> items = new();
    
    public void AddTask(string id, string title, float cur = 0, float max = 1)
    {
        if(items.ContainsKey(id))
        {
            UpdateTask(id,cur,max);
            return;
        }
        var item = Instantiate(taskItemPrefab, content);
        item.SetTitle(title);
        item.SetProgress(cur,max);
        items[id] = item;
    }

    public void UpdateTask(string id, float cur, float max)
    {
        if(!items.TryGetValue(id, out var item))
            return;
        item.SetProgress(cur,max);
    }

    public void CompleteTask(string id)
    {
        if(!items.TryGetValue(id, out var item))
            return;
        Destroy(item.gameObject);
        items.Remove(id);
    }
}
