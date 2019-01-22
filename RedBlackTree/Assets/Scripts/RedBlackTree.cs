using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RedBlackTree
{
	/// <summary>
	/// https://en.wikipedia.org/wiki/Red%E2%80%93black_tree
	/// 
	/// Rule:
	/// 1. Each node is either red or black.
	/// 2. The root is black.
	/// 3. All leaves (NIL) are black.
	/// 4. If a node is red, then both its children are black.
	/// 5. Every path from a given node to any of its descendant NIL nodes contains the same number of black nodes.
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>

	public class RedBlackTree<T>
	{
		#region  Public方法
		public RedBlackTree()
		{
			NIL = new Node<T>(default(T), NodeColor.Black);
			m_Comparer = Comparer<T>.Default;
		}

		public void Insert(T key)
		{
			Node<T> newNode = new Node<T>(key, NodeColor.Red, null, NIL, NIL);
			
			// 查找插入位置
			Node<T> parent = null;
			Node<T> iter = m_Root;
			while (iter != null && iter != NIL)
			{
				parent = iter;
				iter = m_Comparer.Compare(key, iter.Value) <= 0
					? iter.LeftChild
					: iter.RightChild;
			}

			// 插入元素
			newNode.Parent = parent;
			if (parent != null)
			{
				if (m_Comparer.Compare(key, parent.Value) <= 0)
				{
					newNode.LeftChild = parent.LeftChild;
					if (parent.LeftChild != NIL)
						parent.LeftChild.Parent = newNode;

					parent.LeftChild = newNode;
				}
				else
				{
					newNode.RightChild = parent.RightChild;
					if (parent.RightChild != NIL)
						parent.RightChild.Parent = newNode;

					parent.RightChild = newNode;
				}
			}
			else
			{
				m_Root = newNode;
			}

			RepairInsert(newNode);

			m_Root = newNode;
			while (m_Root.Parent != null)
				m_Root = m_Root.Parent;
		}

		// TODO: 待测试
		public void Remove(T key)
		{
			if (m_Root == null)
			{
				return;
			}

			// 查找要删除的元素
			Node<T> deleteNode = m_Root;
			while (deleteNode != NIL)
			{
				int compare = m_Comparer.Compare(key, deleteNode.Value);
				if (compare < 0)
					deleteNode = deleteNode.LeftChild;
				else if (compare > 0)
					deleteNode = deleteNode.RightChild;
				else
					break;
			}

			if (deleteNode == NIL)
				return;

			// 删除元素
			if (deleteNode.LeftChild != NIL && deleteNode.RightChild != NIL)
			{
				Node<T> replaceNode = GetMaxNode(deleteNode.LeftChild);
				if (replaceNode == NIL)
					replaceNode = GetMinNode(deleteNode.RightChild);
				
				Replace(deleteNode, replaceNode);
				deleteNode = replaceNode;
			}

			Node<T> childOfDelete = deleteNode.LeftChild != NIL ? deleteNode.LeftChild : deleteNode.RightChild;

			// 整理删除节点以前的红黑树
			// UNDONE. 注释
			if (deleteNode.Color != childOfDelete.Color)
			{
				childOfDelete.Color = NodeColor.Black;
			}
			else // 子节点的颜色等于删除节点的颜色, 也就是都是黑色
			{
				RepairDelete(deleteNode);
			}

			// 删除deleteNode节点, 用他的子节点替换他的位置
			if (childOfDelete != NIL)
				childOfDelete.Parent = deleteNode.Parent;

			if (deleteNode.Parent.LeftChild == deleteNode)
			{
				deleteNode.Parent.LeftChild = childOfDelete;
			}
			else
			{
				deleteNode.Parent.RightChild = childOfDelete;
			}
		}

		public void Replace(Node<T> a, Node<T> b)
		{
			T tempValue = a.Value;
			a.Value = b.Value;
			b.Value = tempValue;
		}

		public Node<T> GetMaxNode(Node<T> root)
		{
			if (root == NIL)
				return root;

			Node<T> iter = root;
			while (iter.RightChild != NIL)
			{
				iter = iter.RightChild;
			}

			return iter;
		}

		public Node<T> GetMinNode(Node<T> root)
		{
			if (root == NIL)
				return root;

			Node<T> iter = root;
			while (iter.LeftChild != NIL)
			{
				iter = iter.LeftChild;
			}

			return iter;
		}
		#endregion

		#region Private方法
		private void RepairInsert(Node<T> newNode)
		{
			Node<T> p = newNode.Parent;

			// Case 1
			if (newNode.Parent == null)
			{
				m_Root = newNode;
				m_Root.Color = NodeColor.Black;
				return;
			}

			Node<T> g = p.Parent;
			if (g == null)
				return;

			Node<T> u = newNode.Uncle;

			if (p.Color == NodeColor.Black)
			{
				// Case 2
				return;
			}
			else
			{
				if (u.Color == NodeColor.Red)
				{
					// Case 3. 违反了Rule4
					p.Color = NodeColor.Black;
					u.Color = NodeColor.Black;
					g.Color = NodeColor.Red;
					// Case 3是唯一递归检查的一步, 按照我的理解, 标记为红色节点就是改动的节点, 通过不同Case的处理, 红色会向父节点传递
					// 最后有可能传递到根节点. 这种调整最终会避免违反Rule 5
					RepairInsert(g);
				}
				else if (u.Color == NodeColor.Black)
				{
					// Case 4. 违反了Rule4
					if (p == g.LeftChild)
					{
						// 把Case 4 变为Case 5
						if (newNode == p.RightChild)
						{
							RotateLeft(p);
							p = newNode;
							g = newNode.Parent;
						}
						// Case 5. 违反了Rule 4
						g.Color = NodeColor.Red;
						p.Color = NodeColor.Black;
						RotateRight(g);
					}
					else if (p == g.RightChild)
					{
						// 把Case 4 变为Case 5
						if (newNode == p.LeftChild)
						{
							RotateRight(p);
							p = newNode;
							g = newNode.Parent;
						}
						// Case 5. 违反了Rule 4
						g.Color = NodeColor.Red;
						p.Color = NodeColor.Black;
						RotateLeft(g);
					}
				}
			}
		}

		// TODO: 待测试
		private void RepairDelete(Node<T> deleteNode)
		{
			if (deleteNode.Parent == null && deleteNode != NIL)
			{
				// Case 1. Parent为null, 且没有子节点, 那就无所谓了, 根节点的路径少了个黑色, 爱少不少
				return;
			}
			else
			{
				if (deleteNode.Sibling.Color == NodeColor.Red)
				{
					// Case 2. Sibling为红色
					// 经过Case2的变换, deleteNode所在的路径还是少一个黑色, 所以进入CASE 3
					deleteNode.Parent.Color = NodeColor.Red;
					deleteNode.Sibling.Color = NodeColor.Black;

					if (deleteNode == deleteNode.Parent.LeftChild)
						RotateLeft(deleteNode.Parent);
					else
						RotateRight(deleteNode.Parent);
				}

				if (deleteNode.Parent.Color == NodeColor.Black
					&& deleteNode.Sibling.Color == NodeColor.Black
					&& deleteNode.Sibling.LeftChild.Color == NodeColor.Black
					&& deleteNode.Sibling.RightChild.Color == NodeColor.Black)
				{
					// Case 3. sibling和他的子节点都是黑的. 把sibling改为红色, 改完以后sibling和childOfDelete的父节点所在路径就少了个黑色节点, 所以整理父节点
					deleteNode.Sibling.Color = NodeColor.Red;
					RepairDelete(deleteNode.Parent);
				}
				else if (deleteNode.Parent.Color == NodeColor.Red 
						&& deleteNode.Sibling.Color == NodeColor.Black 
						&& deleteNode.Sibling.LeftChild.Color == NodeColor.Black
						&& deleteNode.Sibling.RightChild.Color == NodeColor.Black)
				{
					// Case 4. 此时要平衡deleteNode和sibling, 只需要把sibling的颜色改为红色
					//			要恢复deleteNode的Parent, 只需要把Parent改为黑色
					deleteNode.Parent.Color = NodeColor.Black;
					deleteNode.Sibling.Color = NodeColor.Red;
				}
				else
				{
					if (deleteNode == deleteNode.Parent.LeftChild)
					{
						if (deleteNode.Sibling.LeftChild.Color == NodeColor.Red
							&& deleteNode.Sibling.RightChild.Color == NodeColor.Black)
						{
							// Case 5. Sibling一定是黑色, 因为Sibling是红色在Case 2判断了. 现在的情况就是sibling是黑色, 但是他的左右孩子颜色不一致
							//			Case 5只是为Case 6做准备的一步, 把deleteNode的sibling整理成适合Case 6旋转的样式
							RotateRight(deleteNode.Sibling);
						}

						// Case 6 此时deleteNode路径多一个黑
						//			旋转后sibling路径黑的数量不变, deleteNode路径多一个黑. 所以parent路径黑色数量不变
						Debug.Assert(deleteNode.Sibling.Color == NodeColor.Black
									&& deleteNode.Sibling.RightChild.Color == NodeColor.Red);

						RotateLeft(deleteNode.Parent);
						deleteNode.Uncle.Color = NodeColor.Black;
						deleteNode.Grandparent().Color = deleteNode.Parent.Color;
						deleteNode.Parent.Color = NodeColor.Black;
					}
					else //(childOfDelete == childOfDelete.Parent.RightChild)
					{
						if (deleteNode.Sibling.RightChild.Color == NodeColor.Red
							&& deleteNode.Sibling.LeftChild.Color == NodeColor.Black)
						{
							// Case 5
							RotateLeft(deleteNode.Sibling);
						}

						// Case 6
						Debug.Assert(deleteNode.Sibling.Color == NodeColor.Black
									&& deleteNode.Sibling.LeftChild.Color == NodeColor.Red);

						RotateRight(deleteNode.Parent);
						deleteNode.Uncle.Color = NodeColor.Black;
						deleteNode.Grandparent().Color = deleteNode.Parent.Color;
						deleteNode.Parent.Color = NodeColor.Black;
					}
				}
			}
		}

		private void RotateLeft(Node<T> node)
		{
			if (node == NIL)
				skDebug.Assert(false, "rotate nil", null);

			node.RightChild.Parent = node.Parent;
			if (node.Parent != null)
			{
				if (node.Parent.LeftChild == node)
				{
					node.Parent.LeftChild = node.RightChild;
				}
				else
				{
					node.Parent.RightChild = node.RightChild;
				}
			}
			node.Parent = node.RightChild;

			node.RightChild = node.Parent.LeftChild;
			if (node.RightChild != NIL)
				node.RightChild.Parent = node;

			node.Parent.LeftChild = node;
		}

		private void RotateRight(Node<T> node)
		{
			if (node == NIL)
				skDebug.Assert(false, "rotate nil", null);

			node.LeftChild.Parent = node.Parent;
			if (node.Parent != null)
			{
				if (node.Parent.LeftChild == node)
				{
					node.Parent.LeftChild = node.LeftChild;
				}
				else
				{
					node.Parent.RightChild = node.LeftChild;
				}
			}
			node.Parent = node.LeftChild;

			node.LeftChild = node.Parent.RightChild;
			if (node.LeftChild != NIL)
				node.LeftChild.Parent = node;

			node.Parent.RightChild = node;
		}

		#endregion

		#region 调试输出

		public void DebugOutput()
		{
			//OutputInOrder(m_Root);

			ClearDebugHierarchy();
			m_DebugGO = OutputInHierarchy(m_Root, null, true);
		}

		public bool IsValid()
		{
			int redAdjacentCount = 0;
			int blackCount = 0;
			int blackCountToLeaf = 0;

			return IsValidRecurse(m_Root, redAdjacentCount, blackCount, blackCountToLeaf);
		}
		/// <summary>
		/// 中序输出
		/// </summary>
		private void OutputInOrder(Node<T> root)
		{
			if (root == null || root == NIL)
				return;

			OutputInOrder(root.LeftChild);

			Debug.LogFormat("Value: {0}, Color: {1}", root.Value, root.Color);

			OutputInOrder(root.RightChild);
		}

		/// <summary>
		/// 把红黑树在Hierarchy中输出, GameObject的名字是: 左右_颜色_值
		/// </summary>
		/// <param name="root"></param>
		/// <param name="rootTransform"></param>
		/// <param name="bLeft"></param>
		private GameObject OutputInHierarchy(Node<T> root, Transform rootTransform, bool bLeft)
		{
			if (root == null || root == NIL)
				return null;

			GameObject go = new GameObject();
			go.transform.parent = rootTransform;
			if (rootTransform == null)
			{
				go.name = "Root_" + root.Value;
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				if (bLeft)
				{
					sb.Append("L_");
					go.transform.SetAsFirstSibling();
				}
				else
				{
					sb.Append("R_");
					go.transform.SetAsLastSibling();
				}
				sb.Append(root.Color.ToString());
				sb.Append("_");
				sb.Append(root.Value);
				go.name = sb.ToString();
			}

			OutputInHierarchy(root.LeftChild, go.transform, true);
			OutputInHierarchy(root.RightChild, go.transform, false);

			return go;
		}
		
		private bool IsValidRecurse(Node<T> root, int redAdjacentCount, int blackCountInThisPath, int lastBlackCountToLeaf)
		{
			// 增加计数
			if (root.Color == NodeColor.Red)
			{
				redAdjacentCount++;
				if (redAdjacentCount > 1)
				{
					Debug.LogErrorFormat("Double Red. This: {0}, Parent: {1}", root, root.Parent);
					return false;
				}
			}
			else
			{
				redAdjacentCount = 0;
				blackCountInThisPath++;
			}

			if (root != NIL)
			{
				if (root.LeftChild == null || root.RightChild == null)
				{
					int iii = 0;
					iii = 0;
				}

				// 查看指针
				if (root.LeftChild != NIL)
				{
					if (root.LeftChild.Parent != root)
					{
						int iii = 0;
						iii++;
					}
				}

				if (root.RightChild != NIL)
				{
					if (root.RightChild.Parent != root)
					{
						int iii = 0;
						iii++;
					}
				}
			}

			// 递归遍历子树
			if (root == NIL)
			{
				if (lastBlackCountToLeaf != 0 && blackCountInThisPath != lastBlackCountToLeaf)
				{
					Debug.LogErrorFormat("Unequal black node count. This: {0}, Parent: {1}", root, root.Parent);
					return false;
				}
				lastBlackCountToLeaf = blackCountInThisPath;
			}
			else
			{
				IsValidRecurse(root.LeftChild, redAdjacentCount, blackCountInThisPath, lastBlackCountToLeaf);
				IsValidRecurse(root.RightChild, redAdjacentCount, blackCountInThisPath, lastBlackCountToLeaf);
			}

			Debug.LogFormat("Value: {0}, Red: {1}, Black: {2}, BlackToLastLeaf: {3}", root.Value, redAdjacentCount, blackCountInThisPath, lastBlackCountToLeaf);

			return true;
		}

		public void ClearDebugHierarchy()
		{
			GameObject.DestroyImmediate(m_DebugGO);
		}
		#endregion

		#region Public变量
		#endregion

		#region Private变量
		private Node<T> NIL;

		public Node<T> m_Root;
		private Comparer<T> m_Comparer;

		public GameObject m_DebugGO;
		#endregion
	}
}
