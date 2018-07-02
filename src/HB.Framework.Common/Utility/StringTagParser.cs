using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HB.Framework.Common
{
    /// <summary>
    /// 自定义标签字符串解析器
    /// 全局可以维护一个静态的解析器，初始化后（增加所有的Tag与Value），用于解析标签字符串
    /// </summary>
    public class StringTagParser
    {
        private string _matchPattern = @"(\[%\w+%\])";
        private Hashtable tagCache = new Hashtable();       

        public StringTagParser() { }

        public StringTagParser(string matchPattern)
        {
            _matchPattern = matchPattern;
        }

        #region 操作标签

        public void AddTags(IList<KeyValuePair<string, string>> tags)
        {
            foreach (KeyValuePair<string, string> tag in tags)
            {
                tagCache.Add(tag.Key, tag.Value);
            }
        }

        public bool Exist(string Tag)
        {
            return tagCache.ContainsKey(Tag);
        }

        public void AddTag(string Tag, string Value)
        {
            tagCache.Add(Tag, Value);
        }

        public void AddOrReplace(string Tag, string Value)
        {
            tagCache[Tag] = Value;
        }

        public void RemoveTag(string Tag)
        {
            tagCache.Remove(Tag);
        }

        public void ClearTags()
        {
            tagCache.Clear();
        }

        #endregion

        private string replaceTagHandler(Match token)
        {
            return tagCache.Contains(token.Value) ? tagCache[token.Value].ToString() : string.Empty;
        }

        public string Parse(string stringWithTag)
        {
            MatchEvaluator replaceCallback = new MatchEvaluator(replaceTagHandler);
            return Regex.Replace(stringWithTag, _matchPattern, replaceCallback);
        }
    }
}
