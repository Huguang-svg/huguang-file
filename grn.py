import requests
import json

def call_zhipu_api(messages, model="glm-4-flash"):
    url = "https://open.bigmodel.cn/api/paas/v4/chat/completions"

    headers = {
        "Authorization": "c6cf11da59124f0394b321cadef545bc.STPHaNJfwzNjAlgK",
        "Content-Type": "application/json"
    }

    data = {
        "model": model,
        "messages": messages,
        "temperature": 0.5
    }

    response = requests.post(url, headers=headers, json=data)

    if response.status_code == 200:
        return response.json()
    else:
        raise Exception(f"API调用失败: {response.status_code}, {response.text}")

role_system ="你是一个表演家,所有的回答都要很简洁,不超过5个字"
# 使用示例
messages = [
    {"role": "user", "content": "请介绍一下你自己"}
]

result = call_zhipu_api(messages)
print(result['choices'][0]['message']['content'])