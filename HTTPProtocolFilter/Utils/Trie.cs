using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTPProtocolFilter.Utils
{
    public static class TrieDomainExtentions
    {
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static void InsertDomain(this Trie<DomainPolicy> t, DomainPolicy d)
        {
            if (d.Type == AllowDomainType.SUBDOMAINS && d.DomainFormat[0] != '.')
                d.DomainFormat = '.' + d.DomainFormat;
            t.Insert(Reverse(d.DomainFormat.ToLower()), d);
        }

        public static void InsertDomainRange(this Trie<DomainPolicy> t, List<DomainPolicy> items)
        {
            for (int i = 0; i < items.Count; i++)
                t.InsertDomain(items[i]);
        }

        public static TrieNode<DomainPolicy> SearchDomain(this Trie<DomainPolicy> t, string d)
        {
            return t.Search(Reverse(d.ToLower()));
        }

        public static TrieNode<DomainPolicy> PostfixDomain(this Trie<DomainPolicy> t, string d)
        {
            return t.Prefix(Reverse(d.ToLower()));
        }

        /// <summary>
        /// Find domains that either exect or allowed under subdomain
        /// </summary>
        /// <param name="t"></param>
        /// <param name="d">The hostname (no prefix '.')</param>
        /// <returns>Null if not found a match that is a result of a rule</returns>
        public static TrieNode<DomainPolicy> CheckDomain(this Trie<DomainPolicy> t, string d)
        {
            TrieNode<DomainPolicy> resultNode = null;
            resultNode = t.SearchDomain(d) ?? t.SearchDomain("." + d);

            if (resultNode == null)
            {
                TrieNode<DomainPolicy> postfix = t.PostfixDomain(d);
                if (postfix != null 
                    && postfix.Tag != null // not leaf
                    && postfix.Tag.Type == AllowDomainType.SUBDOMAINS) // Check if found subdomain rule
                {
                    resultNode = postfix;
                }
            }
            return resultNode;
        }
    }

    public class TrieNode<T>
    {
        public char Value { get; set; }
        public T Tag { get; set; }

        public List<TrieNode<T>> Children { get; set; }
        public TrieNode<T> Parent { get; set; }
        public int Depth { get; set; }

        public TrieNode(char value, T tag, int depth, TrieNode<T> parent)
        {
            Value = value;
            Tag = tag;

            Children = new List<TrieNode<T>>();
            Depth = depth;
            Parent = parent;
        }

        public bool IsLeaf()
        {
            return Children.Count == 0;
        }

        public TrieNode<T> FindChildNode(char c)
        {
            foreach (var child in Children)
                if (child.Value == c)
                    return child;

            return null;
        }

        public void DeleteChildNode(char c)
        {
            for (var i = 0; i < Children.Count; i++)
                if (Children[i].Value == c)
                    Children.RemoveAt(i);
        }
    }

    public class Trie<T>
    {
        //https://visualstudiomagazine.com/Articles/2015/10/20/Text-Pattern-Search-Trie-Class-NET.aspx?Page=1
        private readonly TrieNode<T> _root;

        public Trie()
        {
            _root = new TrieNode<T>('^', default(T), 0, null);
        }

        public TrieNode<T> Prefix(string s)
        {
            var currentNode = _root;
            var result = currentNode;

            foreach (var c in s)
            {
                currentNode = currentNode.FindChildNode(c);
                if (currentNode == null)
                    break;
                result = currentNode;
            }

            return result;
        }

        public TrieNode<T> Search(string s)
        {
            var prefix = Prefix(s);
            if (prefix.Depth == s.Length && prefix.FindChildNode('$') != null)
            {
                return prefix;
            }
            return null;
        }

        public void InsertRange<K>(List<K> items, Func<K,string> getValue, Func<K,T> getTag)
        {
            for (int i = 0; i < items.Count; i++)
                Insert(getValue(items[i]), getTag(items[i]));
        }

        public void Insert(string s, T tag)
        {
            var commonPrefix = Prefix(s);
            var current = commonPrefix;

            for (var i = current.Depth; i < s.Length; i++)
            {
                var newNode = new TrieNode<T>(s[i], default(T), current.Depth + 1, current);
                current.Children.Add(newNode);
                current = newNode;
            }

            current.Tag = tag; // put our data only on leafs!

            current.Children.Add(new TrieNode<T>('$', default(T), current.Depth + 1, current));
        }

       

    }
}
