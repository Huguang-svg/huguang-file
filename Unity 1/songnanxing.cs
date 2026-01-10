using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class 宋南星 : MonoBehaviour
{
    [Header("去重设置")]
    [Tooltip("回填最近 N 句到提示词里，强制模型避免复读")]
    public int recentLinesCapacity = 20;

    private readonly Queue<string> recentLines = new Queue<string>();

    [Header("API 配置（请在 Inspector 中填写；不要硬编码）")]
    public string apiKey;
    public string model = "glm-4-flash";

    [Header("显示设置（在 Inspector 中手动设置位置和 Text 组件）")]
    public Text dialogueText;
    public Vector2 anchoredPosition = new Vector2(0, 0);
    public float intervalSeconds = 4f;
    public CanvasGroup dialogueCanvasGroup;
    [Header("淡入淡出与展示时长（秒）")]
    public float fadeDuration = 0.8f;
    public float displayDuration = 3.5f;
    public float firstFadeDuration = 0.2f;
    public float firstDisplayDuration = 2f;
    public float gapBetweenSentences = 3f;

    private const string url = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    void Start()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = LoadApiKey();
        }

        if (dialogueText != null)
        {
            if (dialogueCanvasGroup == null)
            {
                Transform parent = dialogueText.transform.parent;
                if (parent != null)
                {
                    dialogueCanvasGroup = parent.GetComponent<CanvasGroup>();
                    if (dialogueCanvasGroup == null)
                    {
                        dialogueCanvasGroup = parent.gameObject.AddComponent<CanvasGroup>();
                    }
                }
                else
                {
                    dialogueCanvasGroup = dialogueText.GetComponent<CanvasGroup>();
                    if (dialogueCanvasGroup == null)
                    {
                        dialogueCanvasGroup = dialogueText.gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }
            dialogueCanvasGroup.alpha = 0f;
            dialogueCanvasGroup.gameObject.SetActive(false);
            dialogueText.color = new Color(0.9f, 0.85f, 0.8f);
            dialogueText.rectTransform.anchoredPosition = anchoredPosition;
            dialogueText.text = "";
        }

        StartCoroutine(SelfTalkLoop());
    }

    string LoadApiKey()
    {
        try
        {
            string envKey = Environment.GetEnvironmentVariable("MY_AI_API_KEY");
            if (!string.IsNullOrEmpty(envKey)) return envKey;
        }
        catch (Exception) { }

#if UNITY_EDITOR
        try
        {
            string editorKey = EditorPrefs.GetString("宋南星_API_KEY", "");
            if (!string.IsNullOrEmpty(editorKey)) return editorKey;
        }
        catch (Exception) { }
#endif

        try
        {
            string path = Path.Combine(Application.persistentDataPath, ".apikey");
            if (File.Exists(path))
            {
                string fileKey = File.ReadAllText(path).Trim();
                if (!string.IsNullOrEmpty(fileKey)) return fileKey;
            }
        }
        catch (Exception) { }

        // 4) StreamingAssets（方案C：临时测试用，打包会泄露 Key，不要公开发布）
        try
        {
            string streamingKey = ApiKeyConfig.LoadFromStreamingAssets();
            if (!string.IsNullOrEmpty(streamingKey)) return streamingKey;
        }
        catch (Exception) { }

        return "";
    }

    IEnumerator ShowLineRoutine(string line, float fadeIn, float displaySec, float fadeOut)
    {
        if (dialogueText == null || dialogueCanvasGroup == null) yield break;

        dialogueCanvasGroup.gameObject.SetActive(true);
        dialogueText.text = line;
        
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeIn));
            yield return null;
        }
        dialogueCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displaySec);

        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            dialogueCanvasGroup.alpha = 1f - Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeOut));
            yield return null;
        }
        dialogueCanvasGroup.alpha = 0f;
        dialogueText.text = "";
        dialogueCanvasGroup.gameObject.SetActive(false);
    }

    IEnumerator SelfTalkLoop()
    {
        string reply = null;
        yield return StartCoroutine(GetAIReply((r) => reply = r));
        if (!string.IsNullOrEmpty(reply))
        {
            yield return StartCoroutine(ShowLineRoutine(reply, firstFadeDuration, firstDisplayDuration, firstFadeDuration));
            yield return new WaitForSeconds(gapBetweenSentences);
        }

        while (true)
        {
            yield return StartCoroutine(GetAIReply((r) => reply = r));
            if (!string.IsNullOrEmpty(reply))
            {
                yield return StartCoroutine(ShowLineRoutine(reply, fadeDuration, displayDuration, fadeDuration));
            }
            yield return new WaitForSeconds(gapBetweenSentences);
        }
    }

    [Header("重复检测与重试（方案B）")]
    [Tooltip("检测到与历史重复/高度相似时，最多自动重试次数")]
    public int maxRetryCount = 4;

    [Tooltip("重复判定的 2-gram Jaccard 阈值，越大越严格")]
    [Range(0.5f, 0.95f)]
    public float similarityThreshold = 0.78f;

    IEnumerator GetAIReply(Action<string> onComplete)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
        {
            onComplete?.Invoke("（未配置或无效的 API Key）");
            yield break;
        }

        string final = null;
        bool ok = false;

        int retries = Mathf.Clamp(maxRetryCount, 0, 10);
        for (int attempt = 0; attempt <= retries; attempt++)
        {
            string content = null;
            yield return StartCoroutine(RequestOnce((r) => content = r, attempt));

            if (string.IsNullOrEmpty(content))
            {
                continue;
            }

            if (!content.StartsWith("（") && AIRepeatGuard.IsTooSimilar(content, recentLines, similarityThreshold))
            {
                continue;
            }

            final = content;
            ok = true;
            break;
        }

        if (!ok)
        {
            final = "（重复过多，稍后再说）";
        }

        if (!string.IsNullOrEmpty(final) && !final.StartsWith("（"))
        {
            recentLines.Enqueue(final);
            int cap = Mathf.Max(1, recentLinesCapacity);
            while (recentLines.Count > cap)
            {
                recentLines.Dequeue();
            }
        }

        onComplete?.Invoke(final);
    }

    IEnumerator RequestOnce(Action<string> onComplete, int attempt)
    {
        string systemPrompt = @"
【重要规则】
- 你叫宋南星，75岁。
- 你慈祥但健朗，是小镇的“活历史”和“大家长”。
- 你喜欢回忆过去，对年轻人充满关爱。

【语言风格】
- 回忆式语气：常说“从前啊”“我年轻那时候”。
- 叮嘱式语气：对晚辈很关心，爱操心。

【可参考词汇】
- 惊讶：“哎哟喂”“了不得”。
- 赞美：“真俊”“好孩子”“真利索”。
- 情感：“心里头暖和”“惦记着”“怪舍不得的”。
- “这人啊，就跟风筝一样…”。
- “不急，不急”。

【句式特点】
- 句子短，常省略主语，有点地方口音腔调。
- 爱用叠词和语气助词：慢慢走、稳稳的哈、好了哟。

【输出要求】
- 只输出一句自言自语。
- 不超过20个字。
- 绝对不要出现双引号。
- 用直接说话的口吻输出。
- 【最高优先级】绝对不能重复任何一句话（包括与自己之前输出过的内容重复或高度相似）。
";

        if (recentLines.Count > 0)
        {
            systemPrompt += "\n\n【历史记录 - 最高优先级禁复读】\n";
            systemPrompt += "你最近说过这些话，新输出绝对不能与任何一句相同或高度相似：\n";
            int i = 1;
            foreach (var line in recentLines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    systemPrompt += i.ToString() + ") " + line + "\n";
                    i++;
                }
            }
        }

        string userPrompt = "请作为宋南星，生成一句简短的自言自语（不超过20字）。";
        if (attempt > 0)
        {
            userPrompt += "本次必须换一句完全不同的话，严禁复述或改写。";
        }

        string messagesJson = "[{\"role\":\"system\",\"content\":\"" + EscapeJsonString(systemPrompt) + "\"}, {\"role\":\"user\",\"content\":\"" + EscapeJsonString(userPrompt) + "\"}]";
        float temp = 0.6f + 0.08f * attempt;
        if (temp > 0.9f) temp = 0.9f;
        string jsonData = "{\"model\":\"" + model + "\",\"messages\":" + messagesJson + ",\"temperature\":" + temp.ToString("0.##") + "}";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", apiKey.StartsWith("Bearer ") ? apiKey : "Bearer " + apiKey);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                onComplete?.Invoke("（网络或 API 错误）");
                yield break;
            }

            string text = req.downloadHandler.text;
            string content = ExtractContentFromResponse(text);
            if (string.IsNullOrEmpty(content)) content = TruncateForDisplay(text, 200);
            onComplete?.Invoke(content);
        }
    }

    string EscapeJsonString(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    string ExtractContentFromResponse(string response)
    {
        try
        {
            var m = Regex.Match(response, "\"content\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success)
            {
                string raw = m.Groups[1].Value;
                return raw.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
            }
        }
        catch (Exception) { }
        return null;
    }

    string TruncateForDisplay(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length <= max) return s;
        return s.Substring(0, max) + "...";
    }
}