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

public class 王丹木 : MonoBehaviour
{
    [Header("去重设置")]
    [Tooltip("回填最近 N 句到提示词里，强制模型避免复读")]
    public int recentLinesCapacity = 60;

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
    public float fadeDuration = 0.5f;
    public float displayDuration = 2.5f;
    public float firstFadeDuration = 0.15f;
    public float firstDisplayDuration = 1.5f;
    public float gapBetweenSentences = 2f;

    private const string url = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    void Start()
    {
        // 尝试自动加载密钥（优先级：环境变量 -> EditorPrefs（仅编辑器本地）-> 持久化文件）
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = LoadApiKey();
        }

        if (dialogueText != null)
        {
            // 确保有 CanvasGroup 用于整体淡入淡出（优先放在父容器，以包含背景）
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
            // 默认隐藏，显示时再启用并淡入
            dialogueCanvasGroup.alpha = 0f;
            dialogueCanvasGroup.gameObject.SetActive(false);

            dialogueText.color = Color.white;
            dialogueText.rectTransform.anchoredPosition = anchoredPosition;
            dialogueText.text = "";
        }

        StartCoroutine(SelfTalkLoop());
    }

    string LoadApiKey()
    {
        // 1) 从系统环境变量读取（优先）
        try
        {
            // 优先读取“通用”环境变量，便于一处配置多角色共用
            string envKey = Environment.GetEnvironmentVariable("MY_AI_API_KEY");
            if (!string.IsNullOrEmpty(envKey)) return envKey;
        }
        catch (Exception) { }

#if UNITY_EDITOR
        // 2) 编辑器本地存储（EditorPrefs）——不会被提交到仓库
        try
        {
            string editorKey = EditorPrefs.GetString("王丹木_API_KEY", "");
            if (!string.IsNullOrEmpty(editorKey)) return editorKey;
        }
        catch (Exception) { }
#endif

        // 3) 持久化文件（Application.persistentDataPath），建议将该路径加入 .gitignore
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
        if (dialogueText == null || dialogueCanvasGroup == null)
        {
            yield break;
        }

        // 确保容器激活，然后显示文本并淡入
        dialogueCanvasGroup.gameObject.SetActive(true);
        dialogueText.text = line;
        // fade in
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeIn));
            yield return null;
        }
        dialogueCanvasGroup.alpha = 1f;

        // display
        yield return new WaitForSeconds(displaySec);

        // fade out
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            dialogueCanvasGroup.alpha = 1f - Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeOut));
            yield return null;
        }
        dialogueCanvasGroup.alpha = 0f;
        dialogueText.text = "";
        // 隐藏容器以避免占位或接收交互
        dialogueCanvasGroup.gameObject.SetActive(false);
    }

    IEnumerator SelfTalkLoop()
    {
        // 首句更快显示
        string reply = null;
        yield return StartCoroutine(GetAIReply((r) => reply = r));
        if (!string.IsNullOrEmpty(reply))
        {
            yield return StartCoroutine(ShowLineRoutine(reply, firstFadeDuration, firstDisplayDuration, firstFadeDuration));
            yield return new WaitForSeconds(gapBetweenSentences);
        }

        // 后续句子按正常节奏显示
        while (true)
        {
            yield return StartCoroutine(GetAIReply((r) => reply = r));
            if (!string.IsNullOrEmpty(reply))
            {
                yield return StartCoroutine(ShowLineRoutine(reply, fadeDuration, displayDuration, fadeDuration));
            }
            // 等待句间间隔，避免连着说
            yield return new WaitForSeconds(gapBetweenSentences);
        }
    }

    [Header("重复检测与重试（方案B）")]
    [Tooltip("检测到与历史重复/高度相似时，最多自动重试次数")]
    public int maxRetryCount = 5;

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
- 你叫王丹木，是小镇上主要的木材供应商兼简易木工。
- 你有妻子和一个儿子，你很爱他们。常提起他们。
- 你的店铺兼住家，临河而建。

【核心性格】
- 做事讲究结实、靠谱，讲信用。
- 熟人多，都是街坊邻居。

【语言风格】
- 语速不紧不慢，带点地方口音。

【高频词汇】
- “我得多陪陪家人。”
- “都是街坊邻居”
- “让我琢磨琢磨”

【输出要求】
- 只输出一句自言自语。
- 以聊自己的生活为主。包括一日三餐，作息，休闲活动。
- 不超过20个字。
- 绝对不要出现双引号。
- 用直接说话的口吻输出。
- 【最高优先级】绝对不能重复任何一句话（包括与自己之前输出过的内容重复或高度相似）。
- 语境要生活化，适当带点生意相关内容。
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

        string userPrompt = "请作为王丹木，生成一句简短的自言自语（不超过20字）。";
        if (attempt > 0)
        {
            userPrompt += "必须与历史记录完全不同，严禁复述或改写。";
        }

        string messagesJson = "[{\"role\":\"system\",\"content\":\"" + EscapeJsonString(systemPrompt) + "\"}, {\"role\":\"user\",\"content\":\"" + EscapeJsonString(userPrompt) + "\"}]";

        // 重试时略增随机性，帮助跳出固定句式
        float temp = 0.35f + 0.1f * attempt;
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

#if UNITY_2020_1_OR_NEWER
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
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
            var m = Regex.Match(response, "\\\"content\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
            if (m.Success)
            {
                string raw = m.Groups[1].Value;
                return raw.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
            }
        }
        catch (Exception)
        {
            // 解析失败，返回 null 让调用方显示原始响应的摘要
        }
        return null;
    }

    string TruncateForDisplay(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length <= max) return s;
        return s.Substring(0, max) + "...";
    }
}

