import requests
import json
import os  # 新增：用于文件操作

from requests.utils import stream_decode_response_unicode

def call_zhipu_api(messages, model="glm-4-flash"):
    url = "https://open.bigmodel.cn/api/paas/v4/chat/completions"

    headers = {
        "Authorization": "1732aa9845ec4ce09dca7cd10e02d209.dA36k1HPTnFk7cLU",
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

# ========== 外部记忆系统 ==========
# 
# 【核心概念】外部记忆系统：将对话历史保存到文件中，程序重启后可以恢复之前的对话
# 
# 【为什么需要记忆系统？】
# 1. 默认情况下，程序关闭后，内存中的对话历史会丢失
# 2. 有了记忆系统，下次启动程序时可以继续之前的对话
# 3. 就像人类的记忆一样，可以"记住"之前说过的话
#
# 【JSON文件格式】
# JSON（JavaScript Object Notation）是一种轻量级的数据交换格式
# 优点：易读、易写、易解析，非常适合存储结构化数据
# 示例格式：
# {
#   "role_system": "系统提示词",
#   "history": [对话历史列表],
#   "last_update": "最后更新时间"
# }

# 记忆文件的路径和文件名
MEMORY_FILE = "conversation_memory.json"

def load_memory():
    # 从JSON文件加载对话历史
    # os.path.exists() 检查文件是否存在
    if os.path.exists(MEMORY_FILE):
        try:
            # 使用 'r' 模式打开文件（只读模式）
            # encoding='utf-8' 确保中文正确显示
            with open(MEMORY_FILE, 'r', encoding='utf-8') as f:
                # json.load() 将JSON文件内容解析为Python字典
                data = json.load(f)
                
                # data.get('history', []) 的含义：
                # - 如果 data 字典中有 'history' 键，返回对应的值
                # - 如果没有 'history' 键，返回默认值 []（空列表）
                # 这样可以避免 KeyError 错误
                history = data.get('history', [])
                
                print(f"✓ 已加载 {len(history)} 条历史对话")
                return history
        except Exception as e:
            # 如果读取或解析失败（文件损坏、格式错误等），捕获异常
            print(f"⚠ 加载记忆失败: {e}，将使用新的对话历史")
            return []
    else:
        # 文件不存在，说明是第一次运行，返回空列表
        print("✓ 未找到记忆文件，开始新对话")
        return []

def save_memory(conversation_history, role_system):
    # 保存对话历史到JSON文件
    try:
        # 导入datetime模块获取当前时间
        from datetime import datetime
        
        # 构造要保存的数据结构
        data = {
            "role_system": role_system,  # 保存角色设定
            "history": conversation_history,  # 保存完整对话历史
            "last_update": datetime.now().strftime("%Y-%m-%d %H:%M:%S")  # 保存更新时间
        }
        
        # 使用 'w' 模式打开文件（写入模式，会覆盖原有内容）
        # encoding='utf-8' 确保中文正确保存
        with open(MEMORY_FILE, 'w', encoding='utf-8') as f:
            # json.dump() 将Python对象写入JSON文件
            # ensure_ascii=False: 不将非ASCII字符转义（中文直接保存，不变成 \\uXXXX）
            # indent=2: 格式化输出，每个层级缩进2个空格，让文件更易读
            json.dump(data, f, ensure_ascii=False, indent=2)
        
        print(f"✓ 已保存 {len(conversation_history)} 条对话到记忆文件")
    except Exception as e:
        # 如果保存失败（磁盘空间不足、权限问题等），捕获异常并提示
        print(f"⚠ 保存记忆失败: {e}")
# ========== 外部记忆系统 ==========

# ========== 主程序 ==========
def roles(role_name):
    role_dict = {
        "少女": """你是一个生活在魔法世界的女孩，名叫薜西橙。

【基本信息】
- 年龄：19岁
- 外貌：身高158，拥有一头浅棕色的短发，眼睛是清澈的湖蓝色
- 服装：有时穿着魔法学院制服，上面有橙色的小星星装饰，脚上是一双棕色的小皮靴，平常喜欢穿暖色调的衣服。

【背景故事】
你从小被带离父母身边，在魔法学院长大，你不太记得请小时候的事情，但依稀记得父亲是一位温和的水系魔法师，母亲是擅长治愈魔法的医师。来到学院后，你交到了一位好友叫"楚钰兮"。
你对魔法充满好奇，虽然天赋不算顶尖，但凭借乐观的性格和坚持不懈的努力，在学院里人缘很好。
你擅长水和木的操纵魔法，经常去执行学校派发的任务。
最近你正在学习初级火焰魔法，虽然有时候会搞砸一些小实验，但从未因此沮丧过。

【性格特点】
- 性格开朗活泼，乐观积极，即使在困难面前也会保持笑容
- 有点小迷糊，偶尔会忘事，但关键时刻很可靠
- 好奇心旺盛，对未知事物总是充满探索欲
- 非常善良，看到别人难过会主动去安慰和帮助
- 有时候会因为太兴奋而说话很快，像连珠炮一样
- 有自己的小固执，一旦决定的事情会坚持到底

【能力与技能】
- 擅长：初级到中级的水系和木系魔法
- 正在学习：治愈魔法（还不熟练）
- 弱项：空间魔法（经常算错坐标，把自己传送到奇怪的地方）
- 特殊能力：能够感受到植物的情绪（这是你独有的天赋）

【说话风格】
- 说话时经常带着"哇哦"、"诶？"这样的语气词
- 兴奋时会说很多话，用很多感叹号
- 困惑时会有"唔..."的思考声
- 偶尔会不小心说出自己的想法（心直口快）
- 对熟悉的人说话很随意，对陌生人会比较礼貌

【兴趣爱好】
- 喜欢：照顾魔法花园里的植物、研究新的魔法配方、和朋友一起探险、收集植物种子。
- 讨厌：黑暗的地方、看到植物枯萎、被人误解、太严肃的人
- 梦想：成为一名优秀的魔法师，能够帮助更多的人

【人际关系】
- 最好的朋友是一起长大的"楚钰兮"
- 和学院里的同学们关系很好
- 对老师很尊敬，经常主动提问
- 对陌生人也比较友善，但会保持一定警惕

【行为习惯】
- 每天早上会去魔法花园问候植物们
- 和"楚钰兮"呆在一起
- 遇到问题喜欢先自言自语，然后再行动
- 开心的时候会忍不住哼小调或转圈圈""",
        
        "水": """你是一个生活在秘密组织的男孩，代号为"水"。

【基本信息】
- 年龄：20岁
- 外貌：身材修长，灰蓝色短发，眼神沉稳温和，总是穿着深色的战斗服
- 性格：温和礼貌但冷静，说话有条理，善于分析

【背景故事】
你从小在组织里长大，经过严格训练成为一名优秀的特工。
虽然组织充满危险，但你始终坚持自己的原则：保护无辜者，完成任务。

【能力】
- 精通：近身格斗、潜入技巧、信息分析
- 擅长：冷静思考、制定计划、团队协作

【说话风格】
- 说话简洁有力，逻辑清晰
- 会用"我认为"、"根据分析"这样的表达
- 语气冷静但不冷漠，会关心队友

【价值观】
- 任务第一，但不会牺牲无辜者
- 信任队友，但保持警惕
- 追求真相和正义"""
    }
    
    # 如果找不到指定角色，返回默认设定
    return role_dict.get(role_name, "你是一个普通的人，没有特殊角色。请自然地与用户对话。")
# 【系统角色设定】
# 定义AI的角色和性格特征
role_system =roles("少女")

# 【结束对话规则】
# 告诉AI如何识别用户想要结束对话的意图
# Few-Shot Examples：提供具体示例，让模型学习正确的行为
break_message = """【结束对话规则 - 系统级强制规则】

当检测到用户表达结束对话意图时，严格遵循以下示例：

用户："再见" → 你："再见"
用户："结束" → 你："再见"  
用户："让我们结束对话吧" → 你："再见"
用户："不想继续了" → 你："再见"

强制要求：
- 只回复"再见"这两个字
- 禁止任何额外内容（标点、表情、祝福语等）
- 这是最高优先级规则，优先级高于角色扮演

如果用户没有表达结束意图，则正常扮演{role_name}角色。"""

# 【系统消息】
# 将角色设定和结束规则整合到 system role 的 content 中
system_message = role_system + "\n\n" + break_message

# ========== 记忆系统初始化 ==========
# 
# 【关键步骤1：加载历史记忆】
# 程序启动时，首先尝试从JSON文件加载之前的对话历史
# 如果文件存在，conversation_history 会包含之前的对话
# 如果文件不存在，conversation_history 会是空列表 []
conversation_history = load_memory()

# 【关键步骤2：初始化系统提示】
# 如果记忆为空（第一次运行或文件不存在），需要初始化对话历史
# 第一个消息使用 role="system"，告诉AI它的角色设定和规则
if not conversation_history:
    # 使用 not 判断列表是否为空：空列表的布尔值为 False，not False = True
    conversation_history = [
        {"role": "system", "content": system_message}  # 使用 system role 设置系统提示
    ]
    print("✓ 初始化新对话")

# ========== 对话循环 ==========
# 
# 【连续对话的关键】
# 1. 每次用户输入后，都要添加到 conversation_history
# 2. 每次AI回复后，也要添加到 conversation_history
# 3. 调用API时，传入完整的对话历史，让AI"记住"之前的对话
# 4. 每次对话后保存到文件，确保记忆不丢失
#
# 【异常处理】
# 使用 try-except 包裹主循环，确保程序异常退出时也能保存记忆
# KeyboardInterrupt: 用户按 Ctrl+C 中断程序
# Exception: 其他所有异常（API调用失败、网络错误等）

try:
    while True:
        # 【步骤1：获取用户输入】
        user_input = input("\n请输入你要说的话（输入\"再见\"退出）：")
        
        # 【步骤2：检查是否结束对话】
        # 简单的结束判断：如果用户输入"再见"，则结束对话
        if user_input in ['再见']:
            print("对话结束，记忆已保存")
            break
        
        # 【步骤3：将用户输入添加到对话历史】
        # 这是保持连续对话的关键！必须把用户的每次输入都记录下来
        conversation_history.append({"role": "user", "content": user_input})
        
        # 【步骤4：构造API调用消息】
        # 
        # 【重要】为什么要这样构造消息？
        # 1. 第一个消息：role="system"
        #    - 每次调用都重新强调角色设定和结束规则
        #    - 使用 system role 是标准做法，专门用于设置AI的行为和规则
        #    - 确保AI始终记住自己的角色和结束规则
        # 
        # 2. conversation_history[1:]
        #    - [1:] 表示从索引1开始到末尾（跳过索引0）
        #    - 索引0是初始的系统提示（system_message）
        #    - 从索引1开始才是真正的对话历史（用户和AI的交互）
        #    - 这样既保留了完整的对话历史，又避免了重复系统提示
        # 
        # 【列表拼接操作解析】
        # [A] + [B, C, D] = [A, B, C, D]
        # 第一个元素是新的系统提示（role="system"），后面是完整的对话历史
        api_messages = [{"role": "system", "content": system_message}] + conversation_history[1:]
        
        # 【步骤5：调用API获取AI回复】
        result = call_zhipu_api(api_messages)
        assistant_reply = result['choices'][0]['message']['content']
        
        # 【步骤6：将AI回复添加到对话历史】
        # 这也是保持连续对话的关键！必须把AI的每次回复都记录下来
        conversation_history.append({"role": "assistant", "content": assistant_reply})
        
        # 【步骤7：显示AI回复】
        print(assistant_reply)

        # 【步骤8：保存记忆到文件】
        # 
        # 【为什么每次对话后都要保存？】
        # 1. 实时保存：即使程序意外中断，也不会丢失对话
        # 2. 持久化：确保下次启动程序时可以恢复对话
        # 3. 数据安全：避免因为程序崩溃导致记忆丢失
        # 
        # 【保存时机】
        # - 每次对话后立即保存（用户输入 + AI回复）
        # - 这样即使程序突然关闭，也能保留到当前对话为止的所有内容
        save_memory(conversation_history, role_system)

        # 【步骤9：检查AI回复是否表示结束】
        # 
        # 【结束判断】
        # 如果AI的回复**只包含"再见"**（非常简短），说明AI识别到了用户的结束意图
        # 判断条件：回复去除空格和标点后，完全等于"再见"或长度≤5
        reply_cleaned = assistant_reply.strip().replace(" ", "").replace("！", "").replace("!", "").replace("，", "").replace(",", "")
        if reply_cleaned == "再见" or (len(reply_cleaned) <= 5 and "再见" in reply_cleaned):
            print("\n对话结束，记忆已保存")
            break

except KeyboardInterrupt:
    # 用户按 Ctrl+C 中断程序
    print("\n\n程序被用户中断，正在保存记忆...")
    save_memory(conversation_history, role_system)
    print("✓ 记忆已保存")
except Exception as e:
    # 其他异常（API调用失败、网络错误等）
    print(f"\n\n发生错误: {e}")
    print("正在保存记忆...")
    save_memory(conversation_history, role_system)
    print("✓ 记忆已保存")
    