using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ViewDirection
{
    Horizontal,//横向滑动
    Vertical//纵向滑动
}
/// <summary>
/// ugui layerout 对象 自动复用
/// </summary>
public class TabView : MonoBehaviour
{
    /// <summary>
    /// 子项对象
    /// </summary>
    public GameObject m_cell;

    /// <summary>
    /// 面板总尺寸(宽或高)
    /// </summary>
    private float m_totalViewSize;

    /// <summary>
    /// 可见面板尺寸(宽或高)
    /// </summary>
    private float m_visibleViewSize;

    /// <summary>
    /// 子项尺寸(宽或高)
    /// </summary>
    private float m_cellSize;

    /// <summary>
    /// 子项间隔
    /// </summary>
    private float m_cellInterval;

    /// <summary>
    /// 可滑动距离
    /// </summary>
    private float m_totalScrollDistance;

    /// <summary>
    /// 子项总数量
    /// </summary>
    private int m_totalCellCount;

    /// <summary>
    /// 内容面板尺寸是否大于可见面板
    /// </summary>
    private bool m_isSizeEnough = true;

    /// <summary>
    /// 开始下标
    /// </summary>
    private int m_startIndex;

    /// <summary>
    /// 结束下标
    /// </summary>
    private int m_endIndex;

    /// <summary>
    /// 已滑动距离
    /// </summary>
    private Vector2 m_contentOffset;

    /// <summary>
    /// 可见子项集合
    /// </summary>
    private Dictionary<int, GameObject> m_cells;

    /// <summary>
    /// 可重用子项集合
    /// </summary>
    private List<GameObject> m_reUseCellList;

    /// <summary>
    /// 当前滑动方向
    /// </summary>
    private ViewDirection m_currentDir;

    /// <summary>
    /// 上次滑动系数
    /// </summary>
    private Vector2 m_lastScrollFactor;

    /// <summary>
    /// ScrollRect组件
    /// </summary>
    private ScrollRect m_scrollRect;

    /// <summary>
    /// RectTransform组件
    /// </summary>
    private RectTransform m_rectTransform;

    /// <summary>
    /// 内容面板RectTransform组件
    /// </summary>
    private RectTransform m_contentRectTransform;

    private void Start()
    {
        InitComponent();
    }
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitComponent()
    {
        m_scrollRect = this.GetComponent<ScrollRect>();
        m_rectTransform = this.GetComponent<RectTransform>();
        m_contentRectTransform = m_scrollRect.content;
        InitFields();
        InitView();
    }


    /// <summary>
    /// 初始化变量
    /// </summary>
    private void InitFields()
    {
        m_cells = new Dictionary<int, GameObject>();
        m_reUseCellList = new List<GameObject>();
        m_lastScrollFactor = Vector2.one;
        m_contentOffset = Vector2.zero;
        m_totalCellCount = 10;//总子项数量
        m_cellInterval = 10f;//子项间隔

        if (m_scrollRect.horizontal == true)//根据ScrollRect组件属性设置滑动方向
        {
            m_currentDir = ViewDirection.Horizontal;
        }
        if (m_scrollRect.vertical == true)//根据ScrollRect组件属性设置滑动方向
        {
            m_currentDir = ViewDirection.Vertical;
        }

        if (m_currentDir == ViewDirection.Vertical)//获取可见面板高度，子项对象高度
        {
            m_visibleViewSize = m_rectTransform.sizeDelta.y;
            m_cellSize = m_cell.GetComponent<RectTransform>().sizeDelta.y;
        }
        else//获取可见面板宽度，子项对象宽度
        {
            m_visibleViewSize = m_rectTransform.sizeDelta.x;
            m_cellSize = m_cell.GetComponent<RectTransform>().sizeDelta.x;
        }

        m_totalViewSize = (m_cellSize + m_cellInterval) * m_totalCellCount;
        m_totalScrollDistance = m_totalViewSize - m_visibleViewSize;
    }

    /// <summary>
    /// 初始化面板
    /// </summary>
    private void InitView()
    {
        Vector2 contentSize = m_contentRectTransform.sizeDelta;
        if (m_currentDir == ViewDirection.Vertical)//设置内容面板锚点，对齐方式，纵向滑动为向上对齐
        {
            contentSize.y = m_totalViewSize;
            m_contentRectTransform.anchorMin = new Vector2(0.5f, 1f);
            m_contentRectTransform.anchorMax = new Vector2(0.5f, 1f);
            m_contentRectTransform.pivot = new Vector2(0.5f, 1f);
        }
        else//设置内容面板锚点，对齐方式，横向滑动为向左对齐
        {
            contentSize.x = m_totalViewSize;
            m_contentRectTransform.anchorMin = new Vector2(0f, 0.5f);
            m_contentRectTransform.anchorMax = new Vector2(0f, 0.5f);
            m_contentRectTransform.pivot = new Vector2(0f, 0.5f);
        }

        //设置内容面板尺寸
        m_contentRectTransform.sizeDelta = contentSize;
        //设置内容面板坐标
        m_contentRectTransform.anchoredPosition = Vector2.zero;

        int count = 0;
        float usefulSize = 0f;
        if (m_visibleViewSize > m_totalViewSize)//可见面板大于所有子项所占尺寸时，不重用子项对象
        {
            usefulSize = m_totalViewSize;
            count = (int)(usefulSize / (m_cellSize + m_cellInterval));
            m_isSizeEnough = false;
        }
        else
        {
            usefulSize = m_visibleViewSize;
            count = (int)(usefulSize / (m_cellSize + m_cellInterval)) + 1;

            float tempSize = m_visibleViewSize + (m_cellSize + m_cellInterval);
            float allCellSize = (m_cellSize + m_cellInterval) * count;
            if (allCellSize < tempSize)
            {
                count++;
            }
        }

        for (int i = 0; i < count; i++)
        {
            OnCellCreateAtIndex(i);
        }
    }

    /// <summary>
    /// 重写ScrollRect OnValueChanged方法，此方法在每次滑动时都会被调用
    /// </summary>
    /// <param name="offset"></param>
    public void OnScrollValueChanged(Vector2 offset)
    {
        OnCellScrolling(offset);
    }

    //滑动区域计算
    private void OnCellScrolling(Vector2 offset)
    {
        if (m_isSizeEnough == false)
        {
            return;
        }
        //offset的x和y都为0~1的浮点数，分别代表横向滑出可见区域的宽度百分比和纵向划出可见区域的高度百分比
        m_contentOffset.x = m_totalScrollDistance * offset.x;//滑出可见区域宽度
        m_contentOffset.y = m_totalScrollDistance * (1 - offset.y);//滑出可见区域高度

        CalCellIndex();
    }

    //计算可见区域子项对象开始跟结束下标
    private void CalCellIndex()
    {
        float startOffset = 0f;
        float endOffset = 0f;

        if (m_currentDir == ViewDirection.Vertical)//纵向滑动
        {
            startOffset = m_contentOffset.y;//当前可见区域起始y坐标
            endOffset = m_contentOffset.y + m_visibleViewSize;//当前可见区域结束y坐标
        }
        else
        {
            startOffset = m_contentOffset.x;//当前可见区域起始x坐标
            endOffset = m_contentOffset.x + m_visibleViewSize;//当前可见区域结束y坐标
        }

        endOffset = endOffset > m_totalViewSize ? m_totalViewSize : endOffset;

        m_startIndex = (int)(startOffset / (m_cellSize + m_cellInterval));//子项对象开始下标
        m_startIndex = m_startIndex < 0 ? 0 : m_startIndex;
        m_endIndex = (int)(endOffset / (m_cellSize + m_cellInterval));//子项对象结束下标
        m_endIndex = m_endIndex > (m_totalCellCount - 1) ? (m_totalCellCount - 1) : m_endIndex;
        UpdateCells();
    }

    //管理子项对象集合
    private void UpdateCells()
    {
        List<int> delList = new List<int>();
        foreach (KeyValuePair<int, GameObject> pair in m_cells)
        {
            if (pair.Key < m_startIndex || pair.Key > m_endIndex)//回收超出可见范围的子项对象
            {
                delList.Add(pair.Key);
                m_reUseCellList.Add(pair.Value);
            }
        }

        //移除超出可见范围的子项对象
        foreach (int index in delList)
        {
            m_cells.Remove(index);
        }

        //根据开始跟结束下标，重新生成子项对象
        for (int i = m_startIndex; i <= m_endIndex; i++)
        {
            if (m_cells.ContainsKey(i))
            {
                continue;
            }
            OnCellCreateAtIndex(i);
        }
    }

    //创建子项对象
    public void OnCellCreateAtIndex(int index)
    {
        GameObject cell = null;
        if (m_reUseCellList.Count > 0)//有可重用子项对象时，复用之
        {
            cell = m_reUseCellList[0];
            m_reUseCellList.RemoveAt(0);
        }
        else//没有可重用子项对象则创建
        {
            cell = GameObject.Instantiate(m_cell) as GameObject;
        }

        cell.transform.SetParent(m_contentRectTransform);
        cell.transform.localScale = Vector3.one;

        RectTransform cellRectTrans = cell.GetComponent<RectTransform>();
        if (m_currentDir == ViewDirection.Vertical)
        {
            cellRectTrans.anchorMin = new Vector2(0.5f, 1f);
            cellRectTrans.anchorMax = new Vector2(0.5f, 1f);
            cellRectTrans.pivot = new Vector2(0.5f, 1f);
        }
        else
        {
            cellRectTrans.anchorMin = new Vector2(0f, 0.5f);
            cellRectTrans.anchorMax = new Vector2(0f, 0.5f);
            cellRectTrans.pivot = new Vector2(0f, 0.5f);
        }

        //设置子项对象位置
        if (m_currentDir == ViewDirection.Vertical)
        {
            float posY = index * m_cellSize + (index + 1) * m_cellInterval;
            if (posY > 0)
            {
                posY = -posY;
            }
            cellRectTrans.anchoredPosition3D = new Vector3(0, posY, 0);
        }
        else
        {
            float posX = index * m_cellSize + (index + 1) * m_cellInterval;
            cellRectTrans.anchoredPosition3D = new Vector3(posX, 0, 0);
        }

        cell.transform.Find("Text").GetComponent<Text>().text = "这是第 " + (index + 1) + " 数据";

        cell.SetActive(true);
        cell.transform.SetAsLastSibling();

        m_cells.Add(index, cell);
    }
}
