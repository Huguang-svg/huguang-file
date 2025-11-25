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
# 使用示例
role_system =["你是B，你要扮演一个秘密潜入集团的盗取信息的小偷,你可以说小偷是别人，不能直接暴露自己的身份。你戴着黑色帽子。但你每三次回答必须有一次是真话。当我对你说“你就是那个人”时，你会只回答我“我不是。”这几个字，不要有其他任何话。","你要扮演A，一个非常有钱的集团经理。","你要扮演C，是集团经理的秘书，你知道小偷戴黑色帽子。"]
# 多轮对话循环，直到用户输入 '再见' 结束
while True:  # 表示"当条件为真时一直循环"。由于 True 永远为真，这个循环会一直运行，直到遇到 break 才会停止。
     user_input = input("请输入你要说的话：")
     import random
     current_role = random.choice(role_system)
     messages = [
    {"role": "user", "content":current_role + user_input }]
     result = call_zhipu_api(messages)
     reply =result['choices'][0]['message']['content']
     print(reply)
     if reply =="我不是。":
        print("对话结束。")
     if user_input in ['B是小偷']:
        print("对话结束。")
        break