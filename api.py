import requests

def call_zhipu_api(messages, model="glm-4-flash"):
    """
    调用智谱API获取AI回复
    
    参数：
        messages: 对话消息列表，格式：[{"role": "user", "content": "..."}]
        model: 模型名称，默认为 "glm-4-flash"
    
    返回：
        API返回的JSON数据（字典格式）
    """
    url = "https://open.bigmodel.cn/api/paas/v4/chat/completions"

    headers = {
        "Authorization": "c6cf11da59124f0394b321cadef545bc.STPHaNJfwzNjAlgK",
        "Content-Type": "application/json"
    }

    data = {
        "model": model,
        "messages": messages,
        "temperature": 0.6
    }

    response = requests.post(url, headers=headers, json=data)

    if response.status_code == 200:
        return response.json()
    else:
        raise Exception(f"API调用失败: {response.status_code}, {response.text}")
