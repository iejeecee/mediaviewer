using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.AutoCompleteBox
{
    //http://igoro.com/archive/efficient-auto-complete-with-a-ternary-search-tree/
    //for the best performance strings should *NOT* be added in alphabethical order

    public class TernaryTree
    {
        private Node m_root = null;

        private void Add(string s, int pos, ref Node node, object item)
        {
            if (node == null) { node = new Node(s[pos], false); }

            if (s[pos] < node.m_char) { Add(s, pos, ref node.m_left, item); }
            else if (s[pos] > node.m_char) { Add(s, pos, ref node.m_right, item); }
            else
            {
                if (pos + 1 == s.Length)
                {
                    node.m_wordEnd = true;
                    node.item = item;
                }
                else
                {
                    Add(s, pos + 1, ref node.m_center, item);
                }
            }
        }

        public void Add(object item)
        {
            if (item == null) throw new ArgumentException();

            String s = item.ToString();

            if (String.IsNullOrEmpty(s)) throw new ArgumentException();

            Add(s, 0, ref m_root, item);
        }
        public void Clear()
        {
            m_root = null;
        }

        public bool Contains(object item)
        {
            if (item == null) throw new ArgumentException();

            String s = item.ToString();

            if (String.IsNullOrEmpty(s)) throw new ArgumentException();

            int pos = 0;
            Node node = m_root;
            while (node != null)
            {
                int cmp = s[pos] - node.m_char;
                if (s[pos] < node.m_char) { node = node.m_left; }
                else if (s[pos] > node.m_char) { node = node.m_right; }
                else
                {
                    if (++pos == s.Length) return node.m_wordEnd;
                    node = node.m_center;
                }
            }

            return false;
        }

        public List<object> AutoComplete(String s)
        {
            if (s == null || s == "") throw new ArgumentException();

            List<object> suggestions = new List<object>();

            int pos = 0;
            Node node = m_root;
            while (node != null)
            {
                int cmp = s[pos] - node.m_char;
                if (s[pos] < node.m_char) { node = node.m_left; }
                else if (s[pos] > node.m_char) { node = node.m_right; }
                else
                {
                    if (++pos == s.Length)
                    {
                        if (node.m_wordEnd == true)
                        {
                            suggestions.Add(node.item);
                        }

                        FindSuggestions(s, suggestions, node.m_center);
                        return (suggestions);
                    }
                    node = node.m_center;
                }
            }

            return (suggestions);
        }

        private void FindSuggestions(string s, List<object> suggestions, Node node)
        {
            if (node == null)
            {
                return;
            }

            if (node.m_wordEnd == true)
            {
                suggestions.Add(node.item);
            }

            FindSuggestions(s, suggestions, node.m_left);
            FindSuggestions(s + node.m_char, suggestions, node.m_center);
            FindSuggestions(s, suggestions, node.m_right);
        }
    }

    class Node
    {
        internal char m_char;
        internal Node m_left, m_center, m_right;
        internal bool m_wordEnd;
        internal object item;

        public Node(char ch, bool wordEnd)
        {
            m_char = ch;
            m_wordEnd = wordEnd;
            item = null;
        }
    }
}
