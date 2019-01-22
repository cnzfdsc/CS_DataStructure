using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedBlackTree
{
	public enum NodeColor
	{
		Black = 0,
		Red = 1,
	}

	public class Node<T>
	{
		public Node(T v, NodeColor c, Node<T> p = null, Node<T> lc = null, Node<T> rc = null)
		{
			Value = v;
			Color = c;
			Parent = p;
			LeftChild = lc;
			RightChild = rc;
		}

		~Node()
		{
			Debug.LogFormat("Delete Node. V: {0}, C: {1}, P: {2}", Value, Color, Parent);
		}

		#region Public变量
		public Node<T> Grandparent()
		{
			return Parent != null ? Parent.Parent : null;
		}

		public Node<T> Uncle
		{
			get
			{
				if (Grandparent() != null)
				{
					return Parent == Grandparent().LeftChild ? Grandparent().RightChild : Grandparent().LeftChild;
				}
				else
				{
					return null;
				}
			}
		}

		public Node<T> Sibling
		{
			get
			{
				if (Parent != null)
				{
					return this == Parent.LeftChild ? Parent.RightChild : Parent.LeftChild;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Public变量
		public T Value;
		public NodeColor Color;
		public Node<T> Parent
		{
			get
			{
				return m_Parent;
			}
			set
			{
				m_Parent = value;
			}
		}

		private Node<T> m_Parent;
		public Node<T> LeftChild;
		public Node<T> RightChild;
		#endregion
	}
}
