using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

namespace BigLogSearch
{
    public class MatchParser
    {
        ArrayList m_IndexesAndStrings = new ArrayList();

        string m_MatchStart = "match[";
        string m_MatchEnd = "]";

        public MatchParser(string format)
        {
            var parts = format.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in parts)
            {
                p.Trim();

                var matchStartIndex = p.IndexOf(m_MatchStart);
                var matchEndIndex = p.IndexOf(m_MatchEnd);

                if ((matchStartIndex == 0) && (matchEndIndex == (p.Length - 1)))
                {
                    var index = int.Parse(p.Substring(m_MatchStart.Length, matchEndIndex - m_MatchStart.Length));
                    m_IndexesAndStrings.Add(index);
                    continue;
                }
            }
        }
    }
}
