using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public GameObject dialogueObject;
    public float duration = 4f;
}

public class 动态 : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"[动态] Awake() - {gameObject.name} (场景: {gameObject.scene.name})");
    }
    // 每条对话对应一个 DialogueEntry（包含 GameObject 与显示时长）
    public DialogueEntry[] dialogueEntries;
    // 场景运行后等待多长时间开始显示第一条（秒）
    public float startDelay = 0f;
    // 支持点击/触摸跳转到下一句

    // 当前正在显示的对话索引（-1 表示尚未开始）
    private int currentIndex = -1;
    // 当用户点击/触摸时设为 true，用于中断等待并进入下一条
    private bool advanceRequested = false;
    // 当前运行的协程引用（可选）
    private Coroutine sequenceCoroutine;
    

    void Start()
    {
        Debug.Log($"[动态] Start() - {gameObject.name} (场景: {gameObject.scene.name})");
        if (dialogueEntries == null || dialogueEntries.Length == 0)
        {
            Debug.LogWarning("dialogueEntries 未设置或为空，请在 Inspector 中设置对话列表。");
            return;
        }

        // 初始时全部隐藏（防止在编辑器中误显示）
        for (int i = 0; i < dialogueEntries.Length; i++)
        {
            var entry = dialogueEntries[i];
            if (entry != null && entry.dialogueObject != null)
                if (entry.dialogueObject != null) entry.dialogueObject.SetActive(false);
        }

        

        sequenceCoroutine = StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        // 支持在 startDelay 期间通过点击提前开始
        if (startDelay > 0f)
        {
            float elapsed = 0f;
            advanceRequested = false;
            while (elapsed < startDelay && !advanceRequested)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            // 重置 advanceRequested 以免影响后续逻辑
            advanceRequested = false;
        }

        yield return StartCoroutine(ShowSequentially());
    }

    IEnumerator ShowSequentially()
    {
        // 使用可中断的等待逻辑：在每条对话显示期间轮询 elapsed 并响应 advanceRequested
        for (currentIndex = 0; currentIndex < dialogueEntries.Length; currentIndex++)
        {
            var entry = dialogueEntries[currentIndex];
            if (entry == null || entry.dialogueObject == null) continue;

            entry.dialogueObject.SetActive(true);

            // 保护性：确保时长为正
            float dur = entry.duration;
            if (dur <= 0f) dur = 0.01f;

            float elapsed = 0f;
            advanceRequested = false;
            while (elapsed < dur && !advanceRequested)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            entry.dialogueObject.SetActive(false);
        }
        // 重置索引
        currentIndex = -1;
        // 所有对话完成后，尝试触发场景中的 GirlEntrance 的淡出（如果存在）
        var ge = FindObjectOfType<GirlEntrance>();
        if (ge != null)
        {
            ge.TriggerFadeOut();
        }
        // 确保所有对话对象在序列结束后都被隐藏（以防有遗漏）
        if (dialogueEntries != null)
        {
            for (int j = 0; j < dialogueEntries.Length; j++)
            {
                var e = dialogueEntries[j];
                if (e != null && e.dialogueObject != null)
                    e.dialogueObject.SetActive(false);
            }
        }
        // 清理协程引用与标志
        sequenceCoroutine = null;
        advanceRequested = false;
    }

    void Update()
    {
        // 检测鼠标左键点击或触摸开始事件
        bool userClicked = Input.GetMouseButtonDown(0) || Input.touchCount > 0;
        if (!userClicked) return;

        // 如果当前正在显示某条对话，设置 advanceRequested 以跳到下一条
        if (currentIndex >= 0 && currentIndex < dialogueEntries.Length)
        {
            advanceRequested = true;
        }
        else
        {
            // 如果尚在 startDelay（协程存在但尚未开始），也允许点击跳过 delay
            if (sequenceCoroutine != null && currentIndex == -1)
            {
                advanceRequested = true;
            }
        }
    }
}
