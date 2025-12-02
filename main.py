import streamlit as st

from chat import chat_once
from logic import should_exit_by_user, should_exit_by_ai
from roles import get_role_prompt, get_break_rules


ROLE_OPTIONS = ["Oct.yl"]


def reset_conversation(role_name: str):
    """
    基于当前角色重新创建系统提示词和对话历史
    """
    role_prompt = get_role_prompt(role_name)
    system_message = role_prompt + "\n\n" + get_break_rules()
    st.session_state.conversation_history = [{"role": "system", "content": system_message}]
    st.session_state.role_prompt = role_prompt
    st.session_state.initialized = True


def initialize_state():
    if "conversation_history" not in st.session_state:
        st.session_state.conversation_history = []
    if "selected_role" not in st.session_state:
        st.session_state.selected_role = ROLE_OPTIONS[0]
    if "role_prompt" not in st.session_state:
        st.session_state.role_prompt = ""
    if "initialized" not in st.session_state:
        st.session_state.initialized = False

    if not st.session_state.initialized:
        reset_conversation(st.session_state.selected_role)


def render_sidebar():
    with st.sidebar:
        st.header("⚙️ 设置")
        selected_role = st.selectbox("选择角色", ROLE_OPTIONS, index=ROLE_OPTIONS.index(st.session_state.selected_role))

        if selected_role != st.session_state.selected_role:
            st.session_state.selected_role = selected_role
            reset_conversation(selected_role)
            st.rerun()

        if st.button("🔄 清空对话"):
            reset_conversation(st.session_state.selected_role)
            st.rerun()

        st.markdown("---")
        st.markdown("### 📝 说明")
        st.info("- 选择角色后开始对话\n- 对话记录不会保存\n- AI的记忆基于初始记忆文件")


def render_history():
    st.subheader(f"💬 与 {st.session_state.selected_role} 的对话")
    st.code( language=None)
    st.markdown("---")

    for msg in st.session_state.conversation_history[1:]:
        with st.chat_message(msg["role"]):
            st.write(msg["content"])


def handle_user_input(user_input: str):
    if should_exit_by_user(user_input):
        st.info("对话已结束")
        st.stop()

    with st.chat_message("user"):
        st.write(user_input)

    try:
        reply = chat_once(st.session_state.conversation_history, user_input, st.session_state.role_prompt)
    except Exception as error:
        # chat_once已经把用户消息加入历史，失败时移除保持一致
        st.session_state.conversation_history.pop()
        st.error(f"发生错误: {error}")
        return

    with st.chat_message("assistant"):
        st.write(reply)

    if should_exit_by_ai(reply):
        st.info("对话已结束")
        st.stop()


def main():
    st.set_page_config(page_title="AI克隆角色聊天", page_icon="🪼", layout="wide")
    initialize_state()

    st.title("🪼 AI克隆角色聊天")
    st.markdown("---")

    render_sidebar()
    render_history()

    user_input = st.chat_input("输入你的消息...")
    if user_input:
        handle_user_input(user_input)


if __name__ == "__main__":
    main()
