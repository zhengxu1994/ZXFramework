using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopListModel
{
    public Sprite sprite;
    public string name;

    public LoopListModel(Sprite sprite, string name)
    {
        this.sprite = sprite;
        this.name = name;
    }
}

public class LoopListItem : MonoBehaviour
{
    private int _id = -1;
    private float _offsetY;
    private int _itemCount;
    private Func<int, LoopListModel> _getData;
    private LoopListModel model;
    private RectTransform _content;

    private RectTransform content
    {
        get
        {
            if (_content == null)
            {
                _content = transform.parent.GetComponent<RectTransform>();
            }
            return _content;
        }
    }
    private RectTransform _rect;

    private RectTransform rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>();
            }
            return _rect;
        }
    }

    private Image _image;

    private Image image
    {
        get
        {
            if (_image == null)
            {
                _image = GetComponentInChildren<Image>();
            }
            return _image;
        }
    }

    private Text _text;

    private Text text
    {
        get
        {
            if (_text == null)
            {
                _text = GetComponentInChildren<Text>();
            }
            return _text;
        }
    }

    public void Init(int i, float offsetY, int itemCount)
    {
        _offsetY = offsetY;
        _itemCount = itemCount;
        ChangeID(i);
    }

    public void AddListener(Func<int, LoopListModel> getData)
    {
        _getData = getData;
    }

    public void OnValueChange()
    {
        int _startId, _endId;
        UpdateIdRange(out _startId, out _endId);
        JudgeSelfId(_startId, _endId);
    }

    private void JudgeSelfId(int _startId, int _endId)
    {

        if (_id < _startId)
        {
            ChangeID(_endId);
        }
        else if (_id > _endId)
        {
            ChangeID(_startId);
        }
    }

    private void UpdateIdRange(out int _startId, out int _endId)
    {
        _startId = Mathf.FloorToInt(content.anchoredPosition.y / (rect.rect.height + _offsetY));
        if (_startId < 0)
        {
            _startId = 0;
        }
        _endId = _startId + _itemCount - 1;
    }

    private void ChangeID(int Id)
    {
        if (_id != Id && IsVaild(Id))
        {
            _id = Id;
            model = _getData(_id);
            image.sprite = model.sprite;
            text.text = model.name;
            SetPos();
        }

    }

    public void SetPos()
    {
        rect.anchoredPosition = new Vector2(0, -_id * (_offsetY + rect.rect.height));
    }
    private bool IsVaild(int i)
    {
        Debug.Log("index:" + i);
        return _getData(i) != (default(LoopListModel));
    }
}