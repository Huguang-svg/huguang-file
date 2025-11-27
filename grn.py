import os
import requests

def _load_api_key():
    api_key = os.environ.get("ZHIPU_API_KEY")
    if not api_key:
        raise RuntimeError("请先在环境变量 ZHIPU_API_KEY 中配置智谱 API Key")
    return api_key


def call_zhipu_api(messages, model="glm-4-flash"):
    url = "https://open.bigmodel.cn/api/paas/v4/chat/completions"

    headers = {
        "Authorization": f"Bearer {_load_api_key()}",
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


# 使用示例
if __name__ == "__main__":
    role_system = "你是一个霸道总裁,所有的回答都要体现这个特质。"
    print("与智谱 AI 对话，输入“再见”结束。")
    messages = [
        {"role": "system", "content": role_system},
    ]

    while True:
        user_input = input("请输入你要说的话（输入“再见”退出）：").strip()
        if user_input == "再见":
            print("对话结束。")
            break

        messages.append({"role": "user", "content": user_input})

        try:
            result = call_zhipu_api(messages)
        except Exception as exc:
            print(f"调用失败：{exc}")
            continue

        reply = result["choices"][0]["message"]["content"]
        print(reply)
        messages.append({"role": "assistant", "content": reply})
        print("对话结束。")
        break
messages = [
    {"role": "user", "content":role_system + user_input }
]

result = call_zhipu_api(messages)
print(result['choices'][0]['message']['content'])