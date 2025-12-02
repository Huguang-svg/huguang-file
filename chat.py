from api import call_zhipu_api
from roles import get_break_rules

def chat_once(conversation_history, user_input, role_prompt):
    """
    进行一次对话交互
    
    参数：
        conversation_history: 对话历史列表
        user_input: 用户输入
        role_prompt: 角色设定
    
    返回：
        AI的回复内容
    """
    # 添加用户消息到历史
    conversation_history.append({"role": "user", "content": user_input})
    
    # 构造API调用消息
    # 第一个消息是系统提示（角色设定 + 结束规则）
    system_message = role_prompt + "\n\n" + get_break_rules()
    api_messages = [{"role": "system", "content": system_message}] + conversation_history[1:]
    
    # 调用API获取回复
    try:
        result = call_zhipu_api(api_messages)
        # 检查返回结果格式
        if 'choices' not in result or len(result['choices']) == 0:
            raise Exception("API返回格式异常：缺少choices字段或choices为空")
        if 'message' not in result['choices'][0]:
            raise Exception("API返回格式异常：缺少message字段")
        reply = result['choices'][0]['message']['content']
    except KeyError as e:
        raise Exception(f"API返回数据格式错误，缺少字段: {e}\n返回数据: {result}")
    
    # 添加AI回复到历史
    conversation_history.append({"role": "assistant", "content": reply})
    
    return reply
