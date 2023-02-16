using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopList : MonoBehaviour
{
    public float offsetY;
    private float itemHeight;
    private float contentHeight;
    private List<LoopListItem> _items;
    private List<LoopListModel> _models;
    private RectTransform _content;
    void Start()
    {
        _items = new List<LoopListItem>();
        _models = new List<LoopListModel>();
        GetModels();
        _content = transform.Find("Viewport/Content").GetComponent<RectTransform>();
        GameObject go = LoadPrefab("BagItem");
        int itemCount = GetShowItemCount(offsetY, itemHeight);
        //生成count+1个预制体
        SpawnItems(go, itemCount);
        SetContentSize(itemCount, offsetY, itemHeight);
        GetComponent<ScrollRect>().onValueChanged.AddListener(ChangeValue);
    }

    public void ChangeValue(Vector2 data)
    {
        foreach (var item in _items)
        {
            item.OnValueChange();
        }
    }

    private void SpawnItems(GameObject go, int itemCount)
    {
        GameObject itemPrefab = null;
        LoopListItem item = null;
        for (int i = 0; i < itemCount; i++)
        {
            itemPrefab = Instantiate(go, _content.transform);
            item = itemPrefab.AddComponent<LoopListItem>();
            item.AddListener(GetData);
            item.Init(i, offsetY, itemCount);
            _items.Add(item);
        }
    }

    public LoopListModel GetData(int index)
    {
        if (index < 0 || index > _models.Count - 1)
        {
            return default(LoopListModel);
        }
        else
        {
            return _models[index];
        }


    }

    private void GetModels()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Texture/Dog");
        LoopListModel model = null;
        foreach (var sprite in sprites)
        {
            model = new LoopListModel(sprite, sprite.name);
            _models.Add(model);
        }
    }
    private GameObject LoadPrefab(string path)
    {
        GameObject go = Resources.Load<GameObject>(path);
        itemHeight = go.GetComponent<RectTransform>().rect.height;
        return go;
    }

    private int GetShowItemCount(float offsetY, float itemHeight)
    {
        float height = GetComponent<RectTransform>().rect.height;
        return Mathf.CeilToInt(height / (offsetY + itemHeight)) + 1;
    }

    private void SetContentSize(int itemCount, float offsetY, float itemHeight)
    {
        float y = _models.Count * itemHeight + offsetY * (_models.Count - 1);
        _content.sizeDelta = new Vector2(_content.sizeDelta.x, y);
    }
}