using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBlackTree;
using UnityEditor;

public class Main : MonoBehaviour
{
	#region  Public方法
	#endregion

	#region Private方法
	public static void Collapse(GameObject go, bool collapse = true)
	{
		// bail out immediately if the go doesn't have children
		if (go.transform.childCount == 0) return;
		// get a reference to the hierarchy window
		var hierarchy = GetFocusedWindow("Hierarchy");
		// select our go
		SelectObject(go);
		// create a new key event (RightArrow for collapsing, LeftArrow for folding)
		var key = new Event { keyCode = collapse ? KeyCode.RightArrow : KeyCode.LeftArrow, type = EventType.KeyDown };
		// finally, send the window the event
		hierarchy.SendEvent(key);
	}
	public static void SelectObject(Object obj)
	{
		Selection.activeObject = obj;
	}
	public static EditorWindow GetFocusedWindow(string window)
	{
		FocusOnWindow(window);
		return EditorWindow.focusedWindow;
	}
	public static void FocusOnWindow(string window)
	{
		EditorApplication.ExecuteMenuItem("Window/General/" + window);
	}
	#endregion

	#region  Unity消息
	protected void Start()
	{
		m_Tree = new RedBlackTree<int>();
		m_History = new List<int>();
	}

	protected void Update()
	{

		if (Input.GetKeyDown(KeyCode.Space))
		{
			int nodeValue = Random.Range(0, 100);
			for (int i = 0; i < 20; i++)
			{
				m_Tree.Insert(i);
				m_History.Add(i);
				m_Tree.IsValid();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Delete))
		{
			int delIndex = Random.Range(0, m_History.Count - 1);
			if (delIndex >= 0)
			{
				m_Tree.Remove(m_History[delIndex]);
				Debug.LogWarning("Delete Value: " + m_History[delIndex]);
				m_History.RemoveAt(delIndex);
			}

			m_Tree.DebugOutput();
		}
		else if (Input.GetKeyDown(KeyCode.O))
		{
			m_Tree.DebugOutput();
			m_Tree.IsValid();
		}
	}
	#endregion

	#region Public变量
	#endregion

	#region Private变量
	RedBlackTree<int> m_Tree;
	List<int> m_History;
	#endregion
}
