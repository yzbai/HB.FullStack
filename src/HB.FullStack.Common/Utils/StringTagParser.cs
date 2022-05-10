

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// 自定义标签字符串解析器
    /// 全局可以维护一个静态的解析器，初始化后（增加所有的Tag与Value），用于解析标签字符串
    /// </summary>
    public class StringTagParser
    {
        private readonly string _matchPattern = @"(\[%\w+%\])";
        private readonly Hashtable _tagCache = new Hashtable();

        public StringTagParser()
        {
        }

        public StringTagParser(string matchPattern)
        {
            _matchPattern = matchPattern;
        }

        #region 操作标签

        public void AddTags(IList<KeyValuePair<string, string>> tags)
        {
            foreach (KeyValuePair<string, string> tag in tags)
            {
                _tagCache.Add(tag.Key, tag.Value);
            }
        }

        public bool Exist(string Tag)
        {
            return _tagCache.ContainsKey(Tag);
        }

        public void AddTag(string Tag, string Value)
        {
            _tagCache.Add(Tag, Value);
        }

        public void AddOrReplace(string Tag, string Value)
        {
            _tagCache[Tag] = Value;
        }

        public void RemoveTag(string Tag)
        {
            _tagCache.Remove(Tag);
        }

        public void ClearTags()
        {
            _tagCache.Clear();
        }

        #endregion 操作标签

        /// <summary>
        /// Parse
        /// </summary>
        /// <param name="stringWithTag"></param>
        /// <returns></returns>

        public string Parse(string stringWithTag)
        {
            MatchEvaluator replaceCallback = new MatchEvaluator(ReplaceTagHandler);
            return Regex.Replace(stringWithTag, _matchPattern, replaceCallback);
        }

        private string ReplaceTagHandler(Match token)
        {
            return _tagCache.Contains(token.Value) ? _tagCache[token.Value]!.ToString()! : string.Empty;
        }
    }
}

