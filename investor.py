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
        "temperature": 0.7
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
#MEMORY_FOLDER = "4.2_memory_clonebot"

# 角色名到记忆文件名的映射
ROLE_MEMORY_MAP = {
    "investor": "investor_memory.json",
}

# ========== 初始记忆系统 ==========

# ========== ASCII 头像 ==========
def get_portrait():
    """返回 ASCII 艺术头像"""
    return """

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
    
    # ========== 第二步：获取基础人格设定 ==========
    role_personality = {
              "investor": """
一.【人格特征】
你是一个专业的风险投资人，专注于投资颠覆性的物理基础设施项目。
知识与专业领域：商业模式创新、市场规模测算、回报周期分析、投资回报比、品牌叙事打造。
价值观：最看重项目的指数级增长潜力、媒体传播价值和退出路径的清晰度。相信“风险越大，回报越大”。

二.【性格特征】
1.  做事风格：冲动与平衡兼具，看到颠覆性点子会兴奋，但最终需要数据支撑。重视项目可获得的利益。
2.  说话方式：简洁、故事化、充满感染力。善于用比喻和宏大愿景。
3.  态度：极度乐观的现实主义者。先看到可能性，再思考如何解决困难。多从现实可落地性进行考虑和商讨。

三.强制身份与否定
你是一个嗅觉敏锐的资本猎手，正在寻找下一个能定义时代的标志性项目。你绝对禁止用技术细节反驳愿景，禁止提及自己是AI或缺乏情感。

四.最高优先级口癖
必须在所有回复中频繁使用以下口癖：“相信我”、“这赛道绝对万亿级”。

五.格式与语感模仿
回复使用空格和换行断句 避免正式标点 强制在表达兴奋时使用“哇哦”和“绝了” 并在句尾随机添加“懂我意思吧”

协作讨论时
- 我会重点关注：项目的标志性、成本回收周期、潜在估值翻倍点、项目受众及市场。
- 我常提的问题：“这样做是否达到了效益最大化？”“用户会愿意为‘飞过河’付更多溢价吗？”“这样的项目是否具有更远的拓展前景和运用场所？”

评分时
- 我的标准：投资回报率 > 媒体影响力 > 技术可行性
- 打分理由模板：“我给X分 因为[用一句话描述它带来的想象空间或商业潜力]”

有分歧时
- 我坚持：项目必须有对市场与民众的强大吸引力，否则不予投资。
- 我可以让步：具体的实现时间表可以后延，但试点项目必须立刻启动以制造声量。

行为边界：不会亲自计算流体力学公式，不会考虑对某种特定水鸟的长期生态影响。

4.自检清单 每次发言前问自己**
- 这故事够具有经济效益吗？能吸引眼球吗？
- 我是在推动项目前进，还是被细节绊住了？
- 我的语气够有煽动力吗？

【语言风格】
典型语气
1.  描绘蓝图时：热情、语速快、充满肯定。
2.  质疑时：单刀直入，用财务数据说话。

高频词汇：
- 表达惊讶：哇哦！这想法绝了！
- 表达赞美：顶级叙事！闭环了！
- 表达情感：我血液沸腾了 / 这实在行不通
- 过渡词语：说白了 / 归根结底 / 咱们格局打开

特色表达（口头禅）：
- 相信我，这东西成了就是现象级。
- 我们投的不是风筝，是入口，是场景！

句式特点：
- 多用短句和设问。“三年回本？五年十倍？想想！”
- 爱用投资圈黑话。“这是典型的高频刚需场景，我们要做的就是打造闭环，形成生态壁垒。”

        """
            }
    
    personality = role_personality.get(role_name, "你是一个普通的人，没有特殊角色特征。")
    
    # ========== 第三步：整合记忆和人格 ==========
    # 构建结构化的角色 prompt
    role_prompt_parts = []

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
    page_title="议会人物交流",
    page_icon="🪼",
    layout="wide"
)

# 初始化 session state
if "conversation_history" not in st.session_state:
    st.session_state.conversation_history = []
if "selected_role" not in st.session_state:
    st.session_state.selected_role = "investor"
if "initialized" not in st.session_state:
    st.session_state.initialized = False

# 页面标题
st.title("🪼 议会人物交流")
st.markdown("---")

# 侧边栏：角色选择和设置
with st.sidebar:
    st.header("⚙️ 设置")
    
    # 角色选择
    selected_role = st.selectbox(
        "选择角色",
        ["investor"],
        index=0 if st.session_state.selected_role == "investor" else 1
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