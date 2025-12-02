import json
import os

MEMORY_FOLDER = "4.2_memory_clonebot"

ROLE_MEMORY_MAP = {
    "Oct.yl": "Oct.yl_memory.json",
}


def load_memory(role_name: str) -> str:
    """
    加载指定角色的外部记忆内容

    参数：
        role_name: 角色名称

    返回：
        记忆内容字符串，若无内容则返回空字符串
    """
    memory_file = ROLE_MEMORY_MAP.get(role_name)
    if not memory_file:
        return ""

    memory_path = os.path.join(MEMORY_FOLDER, memory_file)
    if not os.path.exists(memory_path):
        return ""

    try:
        with open(memory_path, "r", encoding="utf-8") as file:
            data = json.load(file)
    except Exception:
        return ""

    if isinstance(data, list):
        contents = [
            item.get("content", "")
            for item in data
            if isinstance(item, dict) and item.get("content")
        ]
        return "\n".join(filter(None, contents))

    if isinstance(data, dict):
        return data.get("content", str(data))

    return str(data)
