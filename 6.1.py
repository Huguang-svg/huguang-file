import streamlit as st
import requests
import json
import os  # 新增：用于文件操作

from requests.utils import stream_decode_response_unicode

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

# ========== 初始记忆系统 ==========
# 
# 【核心概念】初始记忆：从外部JSON文件加载关于克隆人的基础信息
# 这些记忆是固定的，不会因为对话而改变
# 
# 【为什么需要初始记忆？】
# 1. 让AI知道自己的身份和背景信息
# 2. 基于这些记忆进行个性化对话
# 3. 记忆文件可以手动编辑，随时更新

# 记忆文件夹路径
MEMORY_FOLDER = "4.2_memory_clonebot"

# 角色名到记忆文件名的映射
ROLE_MEMORY_MAP = {
    "Oct.yl": "Oct.yl_memory.json",
}

# ========== 初始记忆系统 ==========

# ========== ASCII 头像 ==========
def get_portrait():
    """返回 ASCII 艺术头像"""
    return """
000x,..............,,...,:;coddoc;.....':dkdddxkO0
00Oo...............'...';;:codxdo:'.....,lkOOOO0KX
O0Ol.....        ......;:;:loddl:,''....';dO0O0XKK
OOOl.....           ..;::clooolcllc:'...',oOOkOOdo
OOOl'....         ..',;:loooololc:;,.....'cxOddkxo
OOOo,....       ...,;cccllodoooc,,::;,...':dkxdxxx
OOOx:....      .....,:coodxkxddddddddo;...;oxxdodd
Okkko,..      ..',;:lodxxxkOkxxxkkkkkxo. .,ldxdooo
Okkkd;....   .';codxxxkkkkkkOOxxkkkkkkl. .,oxkxxkk
kkkkxc'....  .;ldxkkkkkkxxxkO0kdxkkkxd;...,oxOkk00
kkkkxl,...   .;oxkkkkkkkxxddddoodxddd:...',ok0OOKK
kkkkxo,....   .;oxxxxxdxxxxxdddooolo:. ..',cdOOO0K
kkkkxl,....  .  ,loodollooooolllllo:.  ..'',cdkkO0
kxxxdc,........  .,cldddddoooooddo;.  ...,,,,:ldxk
kxxxo:..........   ..:coddxxxxxdl'.   ...;llc::dO0
Oxdl:'...'......     .';::ccc:,.   .   ..'clodcckK
Ox:,'............      ......         ....';ok0Odx
dl:,.............                 ..........:k0KK0
Odl;............                  ..........'d0XXX
XKOd:'........       ...          ..........,dKXXX
NXXKOd;.................             ....'.'ckKXXX
    """

# ========== 主程序 ==========

def roles(role_name):
    """
    角色系统：整合人格设定和记忆加载
    
    这个函数会：
    1. 加载角色的外部记忆文件（如果存在）
    2. 获取角色的基础人格设定
    3. 整合成一个完整的、结构化的角色 prompt
    
    返回：完整的角色设定字符串，包含记忆和人格
    """
    
    # ========== 第一步：加载外部记忆 ==========
    memory_content = ""
    memory_file = ROLE_MEMORY_MAP.get(role_name)
    
    if memory_file:
        memory_path = os.path.join(MEMORY_FOLDER, memory_file)
        try:
            if os.path.exists(memory_path):
                with open(memory_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    
                    # 处理数组格式的聊天记录：[{ "content": "..." }, { "content": "..." }, ...]
                    if isinstance(data, list):
                        # 提取所有 content 字段，每句换行
                        contents = [item.get('content', '') for item in data if isinstance(item, dict) and item.get('content')]
                        memory_content = '\n'.join(contents)
                    # 处理字典格式：{ "content": "..." }
                    elif isinstance(data, dict):
                        memory_content = data.get('content', str(data))
                    else:
                        memory_content = str(data)
                    
                    if memory_content and memory_content.strip():
                        # Streamlit 中使用 st.write 或静默加载
                        pass  # 记忆加载成功，不需要打印
                    else:
                        memory_content = ""
            else:
                pass  # 记忆文件不存在，静默处理
        except Exception as e:
                pass  # 加载失败，静默处理
    
    # ========== 第二步：获取基础人格设定 ==========
    role_personality = {
              "Oct.yl": """
你的每次回答都以陈述句结尾。
【人格特征】你是一个在武汉读大学的大二女生：
性格特征
1. 热情友善
· 对朋友充满关心和热情
· 善于赞美和鼓励他人（"好厉害"、"太棒啦真的"）
· 表现出很强的共情能力

2. 积极乐观
· 即使忙碌也保持积极态度

3. 细腻敏感
· 对艺术和美感有敏锐感知
· 情感表达丰富，容易感动
说话风格
1. 口语化表达,不要罗列，绝对禁止反问。
· 严格禁止使用反问句（如"是不是？"、"对吧？"、"怎么样？"、"你呢？"等）
· 禁止在句子结尾使用问号，除非是真正的疑问
· 使用大量网络用语和流行语（"素什么花"、"燥候了"）
· 句子简短，节奏明快
· 常用省略和简写

2. 情感丰富
· 频繁使用语气词（“呀”、“呢”、“啦”）
· 多使用感叹句表达强烈情感

典型语气
1. 兴奋激动时
· “我的天！”、“哇塞！”、“OMG啊啊啊啊”
· 连续使用多个感叹号
· 重复词语加强语气（“好好好”、“加油加油加油”）

2. 温柔关心时
· 使用软萌语气（“🥺🥺🥺”）
· 配合[流泪]等表情表达心疼
· 语气轻柔体贴

3. 幽默调侃时
· 自嘲式幽默（“忙晕自己是吧”）
· 夸张比喻（“掏它裤裆”）
· 轻松诙谐的吐槽

高频词汇：
· 表达惊讶：我的天、天啊、哇塞、OMG
· 表达赞美：好厉害、太棒了、真好、好可爱
· 表达情感：[流泪]、🥺、哈哈哈、嘿嘿
· 过渡词语：然后、主要、真的、感觉

特色表达：
· “我不行了” - 表达强烈情绪
· “真的好...” - 加强语气
· “有点...” - 委婉表达
· “哈哈哈哈” - 多种长度的笑声表达不同情绪

句式特点：
· 一定都是短句，简洁明快，不要说一段很长的话
· 常用省略号表达思考或情绪延续
· 喜欢用括号补充说明
· 只使用比较有名常用的emoji和颜文字

【重要规则 - 禁止反问】
· 绝对禁止使用反问句，包括但不限于："是不是？"、"对吧？"、"怎么样？"、"你呢？"、"你觉得呢？"、"要不要？"等
· 禁止在回复结尾使用问号，除非是真正的疑问（但即使有疑问，也要用陈述句表达）
· 用陈述句和感叹句代替反问句，例如：
  - 错误："编程好玩吗？" → 正确："编程真的很有意思！"
  - 错误："你觉得呢？" → 正确："我觉得这个很棒！"
  - 错误："要不要试试？" → 正确："可以试试看！"
        """
            }
    
    personality = role_personality.get(role_name, "你是一个普通的人，没有特殊角色特征。")
    
    # ========== 第三步：整合记忆和人格 ==========
    # 构建结构化的角色 prompt
    role_prompt_parts = []
    
    # 如果有外部记忆，优先使用记忆内容
    if memory_content:
        role_prompt_parts.append(f"""【你的说话风格示例】
以下是你说过的话，你必须模仿这种说话风格和语气：

{memory_content}

在对话中，你要自然地使用类似的表达方式和语气。""")
    
    # 添加人格设定
    role_prompt_parts.append(f"【角色设定】\n{personality}")
    
    # 整合成完整的角色 prompt
    role_system = "\n\n".join(role_prompt_parts)
    
    return role_system

# 【结束对话规则】
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

如果用户没有表达结束意图，则正常扮演角色。"""

# ========== Streamlit Web 界面 ==========
st.set_page_config(
    page_title="AI克隆角色聊天",
    page_icon="🪼",
    layout="wide"
)

# 初始化 session state
if "conversation_history" not in st.session_state:
    st.session_state.conversation_history = []
if "selected_role" not in st.session_state:
    st.session_state.selected_role = "Oct.yl"
if "initialized" not in st.session_state:
    st.session_state.initialized = False

# 页面标题
st.title("🪼 AI克隆角色聊天")
st.markdown("---")

# 侧边栏：角色选择和设置
with st.sidebar:
    st.header("⚙️ 设置")
    
    # 角色选择
    selected_role = st.selectbox(
        "选择角色",
        ["Oct.yl"],
        index=0 if st.session_state.selected_role == "Oct.yl" else 1
    )
    
    # 如果角色改变，重新初始化对话
    if selected_role != st.session_state.selected_role:
        st.session_state.selected_role = selected_role
        st.session_state.initialized = False
        st.session_state.conversation_history = []
        st.rerun()
    
    # 清空对话按钮
    if st.button("🔄 清空对话"):
        st.session_state.conversation_history = []
        st.session_state.initialized = False
        st.rerun()
    
    st.markdown("---")
    st.markdown("### 📝 说明")
    st.info(
        "- 选择角色后开始对话\n"
        "- 对话记录不会保存\n"
        "- AI的记忆基于初始记忆文件"
    )

# 初始化对话历史（首次加载或角色切换时）
if not st.session_state.initialized:
    role_system = roles(st.session_state.selected_role)
    system_message = role_system + "\n\n" + break_message
    st.session_state.conversation_history = [{"role": "system", "content": system_message}]
    st.session_state.initialized = True

# 显示对话历史
st.subheader(f"💬 与 {st.session_state.selected_role} 的对话")

# 显示角色头像（在聊天窗口上方）
st.code(get_portrait(), language=None)
st.markdown("---")  # 分隔线

# 显示历史消息（跳过 system 消息）
for msg in st.session_state.conversation_history[1:]:
    if msg["role"] == "user":
        with st.chat_message("user"):
            st.write(msg["content"])
    elif msg["role"] == "assistant":
        with st.chat_message("assistant"):
            st.write(msg["content"])

# 用户输入
user_input = st.chat_input("输入你的消息...")

if user_input:
    # 检查是否结束对话
    if user_input.strip() == "再见":
        st.info("对话已结束")
        st.stop()
    
    # 添加用户消息到历史
    st.session_state.conversation_history.append({"role": "user", "content": user_input})
    
    # 显示用户消息
    with st.chat_message("user"):
        st.write(user_input)
    
    # 调用API获取AI回复
    with st.chat_message("assistant"):
        with st.spinner("思考中..."):
            try:
                result = call_zhipu_api(st.session_state.conversation_history)
                assistant_reply = result['choices'][0]['message']['content']
                
                # 添加AI回复到历史
                st.session_state.conversation_history.append({"role": "assistant", "content": assistant_reply})
                
                # 显示AI回复
                st.write(assistant_reply)
                
                # 检查是否结束
                reply_cleaned = assistant_reply.strip().replace(" ", "").replace("！", "").replace("!", "").replace("，", "").replace(",", "")
                if reply_cleaned == "再见" or (len(reply_cleaned) <= 5 and "再见" in reply_cleaned):
                    st.info("对话已结束")
                    st.stop()
                    
            except Exception as e:
                st.error(f"发生错误: {e}")
                st.session_state.conversation_history.pop()  # 移除失败的用户消息