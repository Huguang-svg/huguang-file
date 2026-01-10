using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 临时测试方案（方案C）：从 StreamingAssets/api_key.json 读取 API Key。
/// 注意：把 Key 打进包里会泄露，千万别公开发布。
/// </summary>
public static class ApiKeyConfig
{
    private const string FileName = "api_key.json";

    [Serializable]
    private class ApiKeyFile
    {
        public string apiKey;
    }

    /// <summary>
    /// 读取 API Key：优先 StreamingAssets/api_key.json。
    /// 读取失败则返回空字符串。
    /// </summary>
    public static string LoadFromStreamingAssets()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, FileName);

            // Windows/Standalone 下 streamingAssetsPath 是本地文件路径，可直接读
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[ApiKeyConfig] 未找到配置文件：{path}");
                return "";
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return "";

            var data = JsonUtility.FromJson<ApiKeyFile>(json);
            if (data == null || string.IsNullOrWhiteSpace(data.apiKey)) return "";

            return data.apiKey.Trim();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ApiKeyConfig] 读取 StreamingAssets API Key 失败：{e.Message}");
            return "";
        }
    }
}

