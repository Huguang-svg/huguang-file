def should_exit_by_user(message: str) -> bool:
    """
    判断用户是否表达结束意图
    """
    return message.strip() == "再见"


def _clean_reply(reply: str) -> str:
    cleaned = reply.strip()
    for char in (" ", "！", "!", "，", ","):
        cleaned = cleaned.replace(char, "")
    return cleaned


def should_exit_by_ai(reply: str) -> bool:
    """
    检查AI回复是否表示要结束对话
    """
    cleaned = _clean_reply(reply)
    return cleaned == "再见" or (len(cleaned) <= 5 and "再见" in cleaned)
