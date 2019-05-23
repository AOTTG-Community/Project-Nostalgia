using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class StyledComboBox : StyledItem
{
    public StyledComboBoxPrefab containerPrefab;
    private bool isToggled;
    public StyledItem itemMenuPrefab;
    public StyledItem itemPrefab;
    [HideInInspector, SerializeField]
    private List<StyledItem> items = new List<StyledItem>();
    public SelectionChangedHandler OnSelectionChanged;
    [SerializeField, HideInInspector]
    private StyledComboBoxPrefab root;
    [SerializeField]
    private int selectedIndex;

    private void AddItem(object data)
    {
        if (this.itemPrefab != null)
        {
            c__AnonStoreyF yf = new c__AnonStoreyF {
                f__this = this
            };
            Vector3[] vectorArray = new Vector3[4];
            this.itemPrefab.GetComponent<RectTransform>().GetLocalCorners(vectorArray);
            Vector3 position = vectorArray[0];
            float num = position.y - vectorArray[2].y;
            position.y = this.items.Count * num;
            yf.styledItem = UnityEngine.Object.Instantiate(this.itemPrefab, position, this.root.itemRoot.rotation) as StyledItem;
            RectTransform component = yf.styledItem.GetComponent<RectTransform>();
            yf.styledItem.Populate(data);
            component.SetParent(this.root.itemRoot.transform, false);
            component.set_pivot(new Vector2(0f, 1f));
            component.set_anchorMin(new Vector2(0f, 1f));
            component.set_anchorMax(Vector2.one);
            component.set_anchoredPosition(new Vector2(0f, position.y));
            this.items.Add(yf.styledItem);
            component.set_offsetMin(new Vector2(0f, position.y + num));
            component.set_offsetMax(new Vector2(0f, position.y));
            this.root.itemRoot.set_offsetMin(new Vector2(this.root.itemRoot.get_offsetMin().x, (this.items.Count + 2) * num));
            Button button = yf.styledItem.GetButton();
            yf.curIndex = this.items.Count - 1;
            if (button != null)
            {
                button.onClick.AddListener(new UnityAction(yf, (IntPtr) this.m__0));
            }
        }
    }

    public void AddItems(params object[] list)
    {
        this.ClearItems();
        for (int i = 0; i < list.Length; i++)
        {
            this.AddItem(list[i]);
        }
        this.SelectedIndex = 0;
    }

    private void Awake()
    {
        this.InitControl();
    }

    public void ClearItems()
    {
        for (int i = this.items.Count - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyObject(this.items[i].gameObject);
        }
    }

    private void CreateMenuButton(object data)
    {
        if (this.root.menuItem.transform.childCount > 0)
        {
            for (int i = this.root.menuItem.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyObject(this.root.menuItem.transform.GetChild(i).gameObject);
            }
        }
        if ((this.itemMenuPrefab != null) && (this.root.menuItem != null))
        {
            StyledItem item = UnityEngine.Object.Instantiate(this.itemMenuPrefab) as StyledItem;
            item.Populate(data);
            item.transform.SetParent(this.root.menuItem.transform, false);
            RectTransform component = item.GetComponent<RectTransform>();
            component.set_pivot(new Vector2(0.5f, 0.5f));
            component.set_anchorMin(Vector2.zero);
            component.set_anchorMax(Vector2.one);
            component.set_offsetMin(Vector2.zero);
            component.set_offsetMax(Vector2.zero);
            this.root.gameObject.hideFlags = HideFlags.HideInHierarchy;
            Button button = item.GetButton();
            if (button != null)
            {
                button.onClick.AddListener(new UnityAction(this, (IntPtr) this.TogglePanelState));
            }
        }
    }

    public void InitControl()
    {
        if (this.root != null)
        {
            UnityEngine.Object.DestroyImmediate(this.root.gameObject);
        }
        if (this.containerPrefab != null)
        {
            RectTransform component = base.GetComponent<RectTransform>();
            this.root = UnityEngine.Object.Instantiate(this.containerPrefab, component.position, component.rotation) as StyledComboBoxPrefab;
            this.root.transform.SetParent(base.transform, false);
            RectTransform transform2 = this.root.GetComponent<RectTransform>();
            transform2.set_pivot(new Vector2(0.5f, 0.5f));
            transform2.set_anchorMin(Vector2.zero);
            transform2.set_anchorMax(Vector2.one);
            transform2.set_offsetMax(Vector2.zero);
            transform2.set_offsetMin(Vector2.zero);
            this.root.gameObject.hideFlags = HideFlags.HideInHierarchy;
            this.root.itemPanel.gameObject.SetActive(this.isToggled);
        }
    }

    public void OnItemClicked(StyledItem item, int index)
    {
        this.SelectedIndex = index;
        this.TogglePanelState();
        if (this.OnSelectionChanged != null)
        {
            this.OnSelectionChanged(item);
        }
    }

    public void TogglePanelState()
    {
        this.isToggled = !this.isToggled;
        this.root.itemPanel.gameObject.SetActive(this.isToggled);
    }

    public int SelectedIndex
    {
        get
        {
            return this.selectedIndex;
        }
        set
        {
            if ((value >= 0) && (value <= this.items.Count))
            {
                this.selectedIndex = value;
                this.CreateMenuButton(this.items[this.selectedIndex].GetText().text);
            }
        }
    }

    public StyledItem SelectedItem
    {
        get
        {
            if ((this.selectedIndex >= 0) && (this.selectedIndex <= this.items.Count))
            {
                return this.items[this.selectedIndex];
            }
            return null;
        }
    }

    [CompilerGenerated]
    private sealed class c__AnonStoreyF
    {
        internal StyledComboBox f__this;
        internal int curIndex;
        internal StyledItem styledItem;

        internal void m__0()
        {
            this.f__this.OnItemClicked(this.styledItem, this.curIndex);
        }
    }

    public delegate void SelectionChangedHandler(StyledItem item);
}

