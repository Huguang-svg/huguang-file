using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static class AIRepeatGuard
{
    // 归一化：去空白、常见标点、统一大小写
    public static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Trim().ToLowerInvariant();
        // 去掉空白
        s = Regex.Replace(s, "\\s+", "");
        // 去掉中英文常见标点
        s = Regex.Replace(s, "[\\p{P}\\p{S}]+", "");
        return s;
    }

    // 2-gram Jaccard 相似度（对中文也适用）
    public static float Jaccard2Gram(string a, string b)
    {
        a = Normalize(a);
        b = Normalize(b);
        if (a.Length == 0 || b.Length == 0) return 0f;

        var sa = Build2GramSet(a);
        var sb = Build2GramSet(b);

        if (sa.Count == 0 || sb.Count == 0) return 0f;

        int inter = 0;
        foreach (var x in sa)
        {
            if (sb.Contains(x)) inter++;
        }
        int uni = sa.Count + sb.Count - inter;
        if (uni <= 0) return 0f;
        return (float)inter / uni;
    }

    private static HashSet<string> Build2GramSet(string s)
    {
        var set = new HashSet<string>();
        if (string.IsNullOrEmpty(s)) return set;
        if (s.Length == 1)
        {
            set.Add(s);
            return set;
        }
        for (int i = 0; i < s.Length - 1; i++)
        {
            set.Add(s.Substring(i, 2));
        }
        return set;
    }

    public static bool IsTooSimilar(string candidate, IEnumerable<string> history, float jaccardThreshold = 0.75f)
    {
        if (string.IsNullOrEmpty(candidate)) return false;
        string nc = Normalize(candidate);
        if (string.IsNullOrEmpty(nc)) return false;

        foreach (var h in history)
        {
            if (string.IsNullOrEmpty(h)) continue;
            string nh = Normalize(h);
            if (string.IsNullOrEmpty(nh)) continue;

            if (nc == nh) return true;
            // 包含/被包含：常见复读改写
            if (nc.Length >= 4 && nh.Length >= 4)
            {
                if (nc.Contains(nh) || nh.Contains(nc)) return true;
            }
            // 2-gram 相似度
            float j = Jaccard2Gram(candidate, h);
            if (j >= jaccardThreshold) return true;
        }
        return false;
    }
}

