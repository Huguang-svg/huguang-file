# Python 基础知识总结

本文档总结了所有Python文件中的编程知识点。

## 文件概览

### 1. 101.py - 基础变量和运算符
### 2. grn.py - 函数定义和API调用（环境变量、异常处理）
### 3. 3.PY - 随机选择和循环控制
### 4. 4.py - 游戏逻辑和多轮对话（随机权重、自定义模块导入）
### 5. 5.py - 外部记忆系统和持久化对话（JSON文件存储、对话历史管理）
### 6. xunfei_tts.py - 语音合成模块（类、WebSocket、文件操作、线程）

---

## 知识点总结

### 一、变量赋值 (101.py)

```python
x = 86
y = 38
```

**知识点：**
- Python是动态类型语言，无需声明变量类型
- 使用 `=` 进行赋值操作
- 变量名遵循命名规则：字母、数字、下划线，不能以数字开头
- 整数类型 (int) 在Python 3中无大小限制

### 二、运算符和表达式 (101.py)

```python
print(x + y // 2)
```

**知识点：**
- **算术运算符：**
  - `+` 加法
  - `//` 整除运算符（向下取整，返回整数）
  - `/` 普通除法（返回浮点数）
  - `*` 乘法
  - `%` 取余
  - `**` 幂运算

- **运算符优先级：**
  1. 括号 `()`
  2. 幂运算 `**`
  3. 乘除、整除、取余 `*`, `/`, `//`, `%`
  4. 加减 `+`, `-`

- **表达式求值：**
  - `x + y // 2` 先计算 `y // 2`，再与 `x` 相加
  - 结果：`86 + 38 // 2 = 86 + 19 = 105`

### 三、print() 函数 (101.py)

```python
print(x + y // 2)
```

**知识点：**
- `print()` 是Python内置函数，用于输出内容到控制台
- 可以输出变量、表达式结果、字符串等
- 自动在输出后换行

### 四、模块导入 (grn.py)

```python
import requests
import json
```

**知识点：**
- `import` 关键字用于导入模块
- `requests` 是第三方库，用于HTTP请求（需要安装：`pip install requests`）
- `json` 是Python标准库，用于JSON数据处理
- 导入后可以使用模块中的函数和类

### 五、函数定义 (grn.py)

```python
def call_zhipu_api(messages, model="glm-4-flash"):
    # 函数体
```

**知识点：**
- 使用 `def` 关键字定义函数
- 函数名遵循变量命名规则
- **参数：**
  - `messages` - 必需参数
  - `model="glm-4-flash"` - 默认参数（可选）
- 函数体需要缩进（通常4个空格）
- 使用 `return` 返回结果

### 六、字典 (Dictionary) (grn.py)

```python
headers = {
    "Authorization": "...",
    "Content-Type": "application/json"
}

data = {
    "model": model,
    "messages": messages,
    "temperature": 0.5
}
```

**知识点：**
- 字典用花括号 `{}` 定义
- 格式：`{键: 值}`
- 键值对用逗号分隔
- 可以通过键访问值：`dict["key"]`
- 字典是可变的数据结构

### 七、字符串 (String) (grn.py)

```python
url = "https://open.bigmodel.cn/api/paas/v4/chat/completions"
"role": "user"
"content": "你今天过的怎么样"
```

**知识点：**
- 字符串用单引号 `'` 或双引号 `"` 包围
- 字符串是不可变类型
- 可以包含中文字符（Python 3默认支持UTF-8）

### 八、列表 (List) (grn.py)

```python
messages = [
    {"role": "user", "content": "你今天过的怎么样"}
]
```

**知识点：**
- 列表用方括号 `[]` 定义
- 元素用逗号分隔
- 可以包含不同类型的数据（这里是字典）
- 列表是可变的，可以添加、删除、修改元素

### 九、API调用 (grn.py)

```python
response = requests.post(url, headers=headers, json=data)
```

**知识点：**
- `requests.post()` 发送POST请求
- 参数：
  - `url` - 请求地址
  - `headers` - 请求头（字典格式）
  - `json` - JSON数据（自动序列化）
- 返回 `Response` 对象

### 十、条件判断 (grn.py)

```python
if response.status_code == 200:
    return response.json()
else:
    raise Exception(f"API调用失败: {response.status_code}, {response.text}")
```

**知识点：**
- `if-else` 条件语句
- `==` 比较运算符（判断相等）
- `response.status_code` 访问对象属性
- `return` 返回值
- `raise Exception()` 抛出异常
- **f-string格式化：** `f"字符串 {变量}"` - Python 3.6+特性

### 十一、异常处理 (grn.py)

```python
raise Exception(f"API调用失败: {response.status_code}, {response.text}")
```

**知识点：**
- `raise` 关键字用于抛出异常
- `Exception` 是异常基类
- 异常信息可以包含变量值（使用f-string）

### 十二、字典访问 (grn.py)

```python
print(result['choices'][0]['message']['content'])
```

**知识点：**
- 通过键访问字典值：`dict['key']`
- 列表通过索引访问：`list[0]`（索引从0开始）
- 可以链式访问嵌套结构
- 注意：如果键不存在会抛出 `KeyError`

---

## 代码执行结果

### 101.py
- 输出：`105`
- 计算过程：`86 + 38 // 2 = 86 + 19 = 105`

### grn.py
- 调用智谱AI API，返回AI回复内容
- 需要配置正确的API密钥才能运行

### 5.py
- 实现带外部记忆系统的对话程序
- 对话历史保存到 `conversation_memory.json` 文件
- 程序重启后可以恢复之前的对话
- 支持角色扮演和结束对话规则
- 需要配置正确的API密钥才能运行

---

## 学习要点回顾

1. ✅ 变量赋值和基本数据类型
2. ✅ 运算符优先级和表达式求值
3. ✅ 函数定义和参数
4. ✅ 字典和列表的使用
5. ✅ 模块导入和第三方库使用
6. ✅ API调用和HTTP请求
7. ✅ 条件判断和异常处理
8. ✅ 字符串格式化（f-string）

---

## 扩展学习建议

- 学习更多数据类型：元组、集合
- 掌握循环语句：for、while
- 学习文件操作：读写文件
- 深入学习异常处理：try-except-finally
- 了解面向对象编程：类和对象
- 学习列表推导式和生成器
在 `grn.py` 和 `3.PY` 中，我们结合 `while True` 循环与 `input()` 构建多轮对话，并在用户输入"再见"或收到特定回复时调用 `break` 结束循环；每次循环都会调用 `call_zhipu_api()` 并依据 `response.status_code` 判断是否需要抛出异常。

---

## 新增知识点总结（4.py, xunfei_tts.py, grn.py, 3.PY）

### 十三、from ... import 导入 (4.py)

```python
from requests.utils import stream_decode_response_unicode
from xunfei_tts import text_to_speech
```

**知识点：**
- `from 模块 import 函数/类` - 从模块中导入特定函数或类
- 导入后可以直接使用函数名，无需加模块前缀
- 可以导入自定义模块（如 `xunfei_tts`）

### 十四、随机选择 (4.py, 3.PY)

```python
# 带权重的随机选择
current_role = random.choices(role_system, weights=role_weights, k=1)[0]

# 简单随机选择
current_role = random.choice(role_system)
```

**知识点：**
- `random.choices(序列, weights=权重列表, k=数量)` - 带权重的随机选择
  - `weights` 参数控制每个元素被选中的概率
  - `k` 参数指定选择的数量
  - 返回列表，需要 `[0]` 获取第一个元素
- `random.choice(序列)` - 从序列中随机选择一个元素
- 需要先 `import random`

### 十五、多行字符串 (4.py)

```python
game_system = f"""你正在玩"寻找伪人"游戏。你的身份是：{current_role}

游戏规则：
1. 用户会通过提问来猜测你的身份
...
"""
```

**知识点：**
- 三引号 `"""` 或 `'''` 用于定义多行字符串
- 可以包含换行符、引号等特殊字符
- 可以与 f-string 结合使用：`f"""...{变量}..."""`
- 常用于长文本、文档字符串、SQL查询等

### 十六、列表方法 - append() (4.py)

```python
conversation_history.append({"role": "user", "content": user_input})
```

**知识点：**
- `list.append(元素)` - 在列表末尾添加元素
- 直接修改原列表，不返回新列表
- 常用于动态构建列表（如对话历史）

### 十七、字符串包含检查 (4.py, 3.PY)

```python
if "再见" in assistant_reply:
    print("游戏结束")

if user_input in ['B是小偷']:
    break
```

**知识点：**
- `in` 操作符用于检查元素是否在序列中
- `"子串" in 字符串` - 检查子串是否在字符串中
- `元素 in 列表` - 检查元素是否在列表中
- 返回布尔值（True/False）

### 十八、环境变量 (grn.py)

```python
api_key = os.environ.get("ZHIPU_API_KEY")
if not api_key:
    raise RuntimeError("请先在环境变量 ZHIPU_API_KEY 中配置智谱 API Key")
```

**知识点：**
- `os.environ.get("变量名")` - 获取环境变量的值
- 如果变量不存在，返回 `None`
- 可以设置默认值：`os.environ.get("变量名", "默认值")`
- 用于安全存储敏感信息（如API密钥）
- 需要先 `import os`

### 十九、私有函数命名 (grn.py)

```python
def _load_api_key():
    # 函数体
```

**知识点：**
- 以下划线 `_` 开头的函数名表示"私有"函数
- 约定俗成的命名规范，表示该函数仅供内部使用
- Python不会强制限制访问，只是约定

### 二十、主程序入口 (grn.py)

```python
if __name__ == "__main__":
    # 主程序代码
```

**知识点：**
- `__name__` 是Python的特殊变量
- 当文件直接运行时，`__name__ == "__main__"`
- 当文件被导入时，`__name__ == "模块名"`
- 用于区分"直接运行"和"被导入"两种情况
- 常用于测试代码、示例代码

### 二十一、字符串方法 - strip() (grn.py)

```python
user_input = input("请输入：").strip()
```

**知识点：**
- `str.strip()` - 去除字符串两端的空白字符（空格、换行、制表符等）
- `str.lstrip()` - 只去除左端空白
- `str.rstrip()` - 只去除右端空白
- 常用于处理用户输入，去除意外空格

### 二十二、continue 语句 (grn.py)

```python
try:
    result = call_zhipu_api(messages)
except Exception as exc:
    print(f"调用失败：{exc}")
    continue
```

**知识点：**
- `continue` - 跳过当前循环的剩余代码，直接进入下一次循环
- 与 `break` 的区别：
  - `break` - 完全退出循环
  - `continue` - 只跳过本次循环，继续下一次
- 常用于异常处理中，出错后继续处理下一个数据

### 二十三、异常变量捕获 (grn.py)

```python
except Exception as exc:
    print(f"调用失败：{exc}")
```

**知识点：**
- `except Exception as 变量名` - 捕获异常并赋值给变量
- 可以通过变量访问异常信息
- `exc` 是常用的异常变量名（exception的缩写）
- 可以打印异常信息用于调试

### 二十四、类定义 (xunfei_tts.py)

```python
class Ws_Param(object):
    def __init__(self, APPID, APIKey, APISecret, Text):
        self.APPID = APPID
        self.APIKey = APIKey
        # ...
```

**知识点：**
- `class 类名(object):` - 定义类（Python 3中 `object` 可省略）
- `__init__()` - 构造函数，创建对象时自动调用
- `self` - 指向对象本身的引用，必须作为第一个参数
- `self.属性名 = 值` - 定义实例属性
- 面向对象编程的基础

### 二十五、全局变量 (xunfei_tts.py)

```python
tts_audio_file = None
tts_complete = False

def on_message(ws, message):
    global tts_audio_file, tts_complete
    tts_complete = True
```

**知识点：**
- 在函数内修改全局变量需要使用 `global` 关键字
- `global 变量名` - 声明要使用全局变量
- 如果只是读取全局变量，不需要 `global`
- 全局变量在函数外定义

### 二十六、try-except 异常处理 (xunfei_tts.py)

```python
try:
    import pygame
    pygame.mixer.init()
    HAS_PYGAME = True
except:
    HAS_PYGAME = False
```

**知识点：**
- `try-except` - 捕获和处理异常
- `try:` 块包含可能出错的代码
- `except:` 块处理异常（可以指定异常类型）
- 可以嵌套多个 `try-except`
- 用于优雅地处理错误，避免程序崩溃

### 二十七、Base64 编码/解码 (xunfei_tts.py)

```python
text = str(base64.b64encode(self.Text.encode('utf-8')), "UTF8")
audio = base64.b64decode(audio)
```

**知识点：**
- `base64.b64encode(字节)` - 将字节编码为Base64字符串
- `base64.b64decode(字符串)` - 将Base64字符串解码为字节
- 常用于传输二进制数据（如图片、音频）
- 需要先 `import base64`
- 字符串需要先 `.encode('utf-8')` 转为字节

### 二十八、加密和哈希 (xunfei_tts.py)

```python
import hashlib
import hmac

signature_sha = hmac.new(api_secret.encode('utf-8'), 
                        signature_origin.encode('utf-8'),
                        digestmod=hashlib.sha256).digest()
```

**知识点：**
- `hashlib` - 提供哈希算法（MD5、SHA256等）
- `hmac` - 提供HMAC（基于密钥的哈希消息认证码）
- `hmac.new(密钥, 消息, digestmod=算法).digest()` - 生成HMAC
- 常用于API签名、密码加密等安全场景

### 二十九、文件操作 (xunfei_tts.py)

```python
# 检查文件是否存在
if os.path.exists(tts_audio_file):
    os.remove(tts_audio_file)

# 创建目录
if not os.path.exists(AUDIO_SAVE_DIR):
    os.makedirs(AUDIO_SAVE_DIR)

# 打开文件写入
with open(tts_audio_file, 'ab') as f:
    f.write(audio)

# 获取文件大小
os.path.getsize(tts_audio_file)
```

**知识点：**
- `os.path.exists(路径)` - 检查文件/目录是否存在
- `os.makedirs(路径)` - 创建目录（包括父目录）
- `os.remove(路径)` - 删除文件
- `os.path.getsize(路径)` - 获取文件大小（字节）
- `os.path.join(路径1, 路径2)` - 拼接路径（跨平台）
- `open(文件, 模式)` - 打开文件
  - `'ab'` - 追加二进制模式
  - `'wb'` - 写入二进制模式
  - `'r'` - 只读模式
- `with open() as f:` - 上下文管理器，自动关闭文件

### 三十、时间操作 (xunfei_tts.py)

```python
import time
from datetime import datetime
from time import mktime

timestamp = int(time.time())
time.sleep(0.5)
now = datetime.now()
```

**知识点：**
- `time.time()` - 返回当前时间戳（秒，浮点数）
- `int(time.time())` - 转为整数时间戳
- `time.sleep(秒数)` - 暂停执行指定秒数
- `datetime.now()` - 获取当前日期时间对象
- `mktime(时间元组)` - 将时间元组转为时间戳
- 常用于计时、延迟、日志记录

### 三十一、字符串方法 (xunfei_tts.py)

```python
# 查找子串位置
stidx = requset_url.index("://")

# 字符串切片
host = requset_url[stidx + 3:]

# 字符串格式化（旧式）
signature_origin = "host: {}\ndate: {}\n{} {} HTTP/1.1".format(host, date, method, path)

# 字符串格式化（% 方式）
authorization_origin = "api_key=\"%s\", algorithm=\"%s\"" % (api_key, "hmac-sha256")
```

**知识点：**
- `str.index(子串)` - 查找子串第一次出现的位置（找不到会抛异常）
- `str.find(子串)` - 查找子串位置（找不到返回-1）
- 字符串切片：`字符串[起始:结束]` 或 `字符串[起始:]`
- `.format()` - 格式化字符串（Python 2.7+）
- `%` 格式化 - 旧式字符串格式化
- `.encode('utf-8')` - 将字符串编码为字节
- `.decode('utf-8')` - 将字节解码为字符串

### 三十二、字典方法 - get() (xunfei_tts.py)

```python
audio = message["payload"]["audio"].get('audio', '')
```

**知识点：**
- `dict.get(键, 默认值)` - 安全获取字典值
- 如果键存在，返回对应的值
- 如果键不存在，返回默认值（不会抛异常）
- 比 `dict[键]` 更安全，避免 `KeyError`

### 三十三、WebSocket 编程 (xunfei_tts.py)

```python
import websocket

ws = websocket.WebSocketApp(wsUrl, 
                           on_message=on_message, 
                           on_error=on_error, 
                           on_close=on_close)
ws.on_open = lambda ws: on_open(ws, wsParam)
ws.run_forever(sslopt={"cert_reqs": ssl.CERT_NONE})
ws.send(json.dumps(d))
ws.close()
```

**知识点：**
- WebSocket 是双向通信协议（不同于HTTP的单向）
- `websocket.WebSocketApp()` - 创建WebSocket应用
- 回调函数：
  - `on_message` - 收到消息时调用
  - `on_error` - 出错时调用
  - `on_close` - 关闭时调用
  - `on_open` - 连接打开时调用
- `ws.send()` - 发送消息
- `ws.close()` - 关闭连接
- `ws.run_forever()` - 运行WebSocket循环

### 三十四、线程编程 (xunfei_tts.py)

```python
import _thread as thread

def run(*args):
    # 线程执行的代码
    pass

thread.start_new_thread(run, ())
```

**知识点：**
- `_thread` - Python的低级线程模块
- `thread.start_new_thread(函数, 参数元组)` - 启动新线程
- 线程用于并发执行任务（如网络请求和主程序同时运行）
- `*args` - 可变参数，接收任意数量的参数
- 注意：线程间共享全局变量，需要注意同步问题

### 三十五、lambda 表达式 (xunfei_tts.py)

```python
ws.on_open = lambda ws: on_open(ws, wsParam)
```

**知识点：**
- `lambda 参数: 表达式` - 匿名函数（一行函数）
- 等价于：
```python
def func(ws):
    return on_open(ws, wsParam)
ws.on_open = func
```
- 常用于简单的回调函数、排序键等

### 三十六、嵌套函数定义 (xunfei_tts.py)

```python
def on_open(ws, wsParam):
    def run(*args):
        d = {"header": wsParam.CommonArgs, ...}
        ws.send(json.dumps(d))
    thread.start_new_thread(run, ())
```

**知识点：**
- 可以在函数内部定义函数（嵌套函数）
- 内部函数可以访问外部函数的变量（闭包）
- 常用于封装逻辑、回调函数等

### 三十七、上下文管理器 - with 语句 (xunfei_tts.py)

```python
with open(tts_audio_file, 'ab') as f:
    f.write(audio)
```

**知识点：**
- `with 语句` - 上下文管理器
- 自动管理资源的打开和关闭（如文件）
- 即使发生异常也会正确关闭资源
- 比 `try-finally` 更简洁

### 三十八、平台检测 (xunfei_tts.py)

```python
import platform

system = platform.system()
if system == "Windows":
    os.system(f'start "" "{abs_path}"')
elif system == "Darwin":
    os.system(f'afplay "{abs_path}"')
else:
    os.system(f'mpg123 "{abs_path}"')
```

**知识点：**
- `platform.system()` - 返回操作系统名称
  - `"Windows"` - Windows系统
  - `"Darwin"` - macOS系统
  - `"Linux"` - Linux系统
- 用于编写跨平台代码，根据不同系统执行不同命令

### 三十九、子进程 (xunfei_tts.py)

```python
import subprocess

subprocess.run(['ffplay', '-nodisp', '-autoexit', abs_path], 
              capture_output=True, check=False)
```

**知识点：**
- `subprocess.run()` - 运行外部命令
- 参数：
  - 第一个参数：命令列表或字符串
  - `capture_output=True` - 捕获输出
  - `check=False` - 不检查返回码（不抛异常）
- 用于调用系统命令、其他程序等

### 四十、条件表达式 - if-elif-else (xunfei_tts.py)

```python
if HAS_PYGAME:
    # pygame代码
elif HAS_PLAYSOUND:
    # playsound代码
else:
    # 备选方案
```

**知识点：**
- `if-elif-else` - 多条件判断
- `elif` 是 `else if` 的缩写
- 按顺序检查条件，第一个满足的会执行
- 可以有多个 `elif`

### 四十一、字符串拼接 (3.PY)

```python
{"role": "user", "content": current_role + user_input}
```

**知识点：**
- `字符串1 + 字符串2` - 字符串拼接
- 使用 `+` 操作符连接字符串
- 注意：只能拼接字符串，其他类型需要先转换

### 四十二、布尔常量 (xunfei_tts.py)

```python
HAS_PYGAME = True
SAVE_AUDIO = True
if not tts_complete:
    # ...
```

**知识点：**
- `True` - 真值（布尔类型）
- `False` - 假值（布尔类型）
- `not` - 逻辑非运算符
- 用于条件判断、标志位等

### 四十三、URL解析和编码 (xunfei_tts.py)

```python
from urllib.parse import urlencode

values = {"host": host, "date": date, "authorization": authorization}
return requset_url + "?" + urlencode(values)
```

**知识点：**
- `urlencode(字典)` - 将字典编码为URL查询字符串
- 自动处理特殊字符的URL编码
- 常用于构建带参数的URL

### 四十四、动态类型创建 (xunfei_tts.py)

```python
return type('Url', (), {'host': host, 'path': path, 'schema': schema})()
```

**知识点：**
- `type(类名, 基类元组, 属性字典)` - 动态创建类
- 这是高级用法，通常不常用
- 等价于创建一个简单的对象来存储数据

---

## 新增知识点总结（5.py）

### 四十五、JSON文件读写 (5.py)

```python
# 读取JSON文件
with open(MEMORY_FILE, 'r', encoding='utf-8') as f:
    data = json.load(f)

# 写入JSON文件
with open(MEMORY_FILE, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
```

**知识点：**
- `json.load(文件对象)` - 从文件读取并解析JSON数据，返回Python对象（字典或列表）
- `json.dump(对象, 文件对象, 参数)` - 将Python对象写入JSON文件
  - `ensure_ascii=False` - 不将非ASCII字符转义（中文直接保存，不变成 `\uXXXX`）
  - `indent=2` - 格式化输出，每个层级缩进2个空格，让文件更易读
- 文件模式：
  - `'r'` - 只读模式（读取文件）
  - `'w'` - 写入模式（覆盖原有内容）
- `encoding='utf-8'` - 指定文件编码，确保中文正确读写
- 需要先 `import json`

### 四十六、文件存在检查 (5.py)

```python
if os.path.exists(MEMORY_FILE):
    # 文件存在时的操作
else:
    # 文件不存在时的操作
```

**知识点：**
- `os.path.exists(路径)` - 检查文件或目录是否存在
- 返回布尔值（True/False）
- 常用于判断文件是否存在，避免文件不存在时的错误
- 需要先 `import os`

### 四十七、字典方法 - get() 带默认值 (5.py)

```python
history = data.get('history', [])
role = role_dict.get(role_name, "你是一个普通的人，没有特殊角色")
```

**知识点：**
- `dict.get(键, 默认值)` - 安全获取字典值
- 如果键存在，返回对应的值
- 如果键不存在，返回默认值（不会抛出 `KeyError`）
- 比 `dict[键]` 更安全，避免程序崩溃
- 常用于处理可能缺失的键

### 四十八、列表切片 (5.py)

```python
api_messages = [{"role": "system", "content": system_message}] + conversation_history[1:]
```

**知识点：**
- `列表[起始:结束]` - 列表切片，返回新列表
- `列表[1:]` - 从索引1开始到末尾（跳过索引0）
- `列表[:结束]` - 从开头到结束（不包含结束索引）
- `列表[起始:结束]` - 从起始到结束（不包含结束索引）
- 切片不会修改原列表，返回新列表
- 常用于跳过某些元素或提取部分元素

### 四十九、列表拼接 (5.py)

```python
api_messages = [{"role": "system", "content": system_message}] + conversation_history[1:]
```

**知识点：**
- `列表1 + 列表2` - 列表拼接，返回新列表
- 将两个列表的元素合并成一个新列表
- 不修改原列表，返回新列表
- 常用于组合多个列表

### 五十、布尔判断 - not 运算符 (5.py)

```python
if not conversation_history:
    conversation_history = [{"role": "system", "content": system_message}]
```

**知识点：**
- `not` - 逻辑非运算符，取反布尔值
- `not True` = `False`，`not False` = `True`
- 空列表的布尔值为 `False`，非空列表为 `True`
- `not []` = `True`（空列表取反为True）
- `not [1, 2]` = `False`（非空列表取反为False）
- 常用于判断列表、字符串等是否为空

### 五十一、字符串方法 - replace() (5.py)

```python
reply_cleaned = assistant_reply.strip().replace(" ", "").replace("！", "").replace("!", "")
```

**知识点：**
- `str.replace(旧子串, 新子串)` - 替换字符串中的子串
- 返回新字符串，不修改原字符串
- 可以链式调用多个 `replace()`
- 常用于清理字符串，去除不需要的字符
- 注意：字符串是不可变的，每次 `replace()` 都返回新字符串

### 五十二、字符串方法链式调用 (5.py)

```python
reply_cleaned = assistant_reply.strip().replace(" ", "").replace("！", "").replace("!", "")
```

**知识点：**
- 可以连续调用多个字符串方法
- 每个方法返回新字符串，可以继续调用下一个方法
- 从左到右依次执行
- 常用于对字符串进行多步处理
- 示例：`字符串.strip().replace().replace()` - 先去除空白，再替换字符

### 五十三、len() 函数 (5.py)

```python
print(f"✓ 已加载 {len(history)} 条历史对话")
if len(reply_cleaned) <= 5:
    # ...
```

**知识点：**
- `len(序列)` - 返回序列的长度（元素个数）
- 可以用于列表、字符串、字典、元组等
- 返回整数
- 常用于判断序列是否为空、获取元素数量等

### 五十四、字符串拼接 - + 运算符 (5.py)

```python
system_message = role_system + "\n\n" + break_message
```

**知识点：**
- `字符串1 + 字符串2` - 字符串拼接
- 使用 `+` 操作符连接多个字符串
- 返回新字符串，不修改原字符串
- 可以连接多个字符串：`str1 + str2 + str3`
- 注意：只能拼接字符串，其他类型需要先转换

### 五十五、input() 函数 (5.py)

```python
user_input = input("\n请输入你要说的话（输入\"再见\"退出）：")
```

**知识点：**
- `input(提示信息)` - 获取用户输入
- 程序会暂停，等待用户输入
- 用户按回车后，返回输入的字符串（不包含换行符）
- 提示信息会显示在控制台
- 返回的是字符串类型，如需其他类型需要转换

### 五十六、while True 无限循环 (5.py)

```python
while True:
    user_input = input("...")
    if user_input in ['再见']:
        break
    # ...
```

**知识点：**
- `while True:` - 无限循环，直到遇到 `break`
- 常用于需要持续运行的程序（如交互式程序）
- 必须要有退出条件（如 `break`），否则会一直运行
- 与 `break` 配合使用，实现条件退出

### 五十七、break 语句 (5.py)

```python
if user_input in ['再见']:
    print("对话结束，记忆已保存")
    break
```

**知识点：**
- `break` - 立即退出当前循环
- 只能用在循环中（`while`、`for`）
- 执行 `break` 后，循环立即结束，继续执行循环后的代码
- 常用于满足某个条件时退出循环

### 五十八、异常处理 - KeyboardInterrupt (5.py)

```python
try:
    while True:
        # 主程序代码
except KeyboardInterrupt:
    print("\n\n程序被用户中断，正在保存记忆...")
    save_memory(conversation_history, role_system)
```

**知识点：**
- `KeyboardInterrupt` - 键盘中断异常
- 当用户按 `Ctrl+C` 时触发
- 需要在 `try-except` 中捕获
- 常用于优雅地处理用户中断程序的情况
- 可以在异常处理中执行清理操作（如保存数据）

### 五十九、异常处理 - 多个except (5.py)

```python
try:
    # 主程序代码
except KeyboardInterrupt:
    # 处理键盘中断
except Exception as e:
    # 处理其他异常
```

**知识点：**
- 可以有多个 `except` 子句，处理不同类型的异常
- 按顺序检查，第一个匹配的会执行
- `KeyboardInterrupt` - 键盘中断异常
- `Exception` - 所有异常的基类，可以捕获所有异常
- `except Exception as e` - 捕获异常并赋值给变量 `e`
- 常用于区分不同类型的错误，执行不同的处理逻辑

### 六十、datetime 模块 - strftime() (5.py)

```python
from datetime import datetime

last_update = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
```

**知识点：**
- `datetime.now()` - 获取当前日期时间对象
- `datetime对象.strftime(格式字符串)` - 将日期时间格式化为字符串
- 格式字符串：
  - `%Y` - 四位年份（如 2024）
  - `%m` - 月份（01-12）
  - `%d` - 日期（01-31）
  - `%H` - 小时（00-23）
  - `%M` - 分钟（00-59）
  - `%S` - 秒（00-59）
- 常用于记录时间戳、日志等

### 六十一、函数内部导入 (5.py)

```python
def save_memory(conversation_history, role_system):
    from datetime import datetime
    # 使用 datetime
```

**知识点：**
- 可以在函数内部导入模块
- 导入语句可以放在函数开头
- 模块只在函数被调用时才导入
- 可以延迟导入，减少程序启动时间
- 注意：如果函数被多次调用，导入会重复执行（但Python会缓存已导入的模块）

### 六十二、常量定义 (5.py)

```python
MEMORY_FILE = "conversation_memory.json"
```

**知识点：**
- 常量通常用全大写字母命名（约定俗成）
- Python没有真正的常量，只是约定不变
- 常量用于存储不会改变的值（如配置、路径等）
- 便于维护和修改

### 六十三、系统消息角色 - role="system" (5.py)

```python
conversation_history = [
    {"role": "system", "content": system_message}
]

api_messages = [{"role": "system", "content": system_message}] + conversation_history[1:]
```

**知识点：**
- 在对话API中，`role="system"` 用于设置AI的系统提示
- 系统消息通常放在对话历史的第一条
- 用于定义AI的角色、行为规则、约束条件等
- 与 `role="user"`（用户消息）和 `role="assistant"`（AI回复）区分
- 每次API调用时，系统消息会重新强调AI的行为规则

### 六十四、对话历史管理 (5.py)

```python
# 添加用户消息
conversation_history.append({"role": "user", "content": user_input})

# 添加AI回复
conversation_history.append({"role": "assistant", "content": assistant_reply})
```

**知识点：**
- 对话历史是包含多个消息的列表
- 每个消息是字典，包含 `role` 和 `content`
- `role` 可以是 `"system"`、`"user"`、`"assistant"`
- 通过 `append()` 不断添加新消息
- 完整的对话历史让AI能够"记住"之前的对话
- 这是实现连续对话的关键

### 六十五、外部记忆系统（持久化）(5.py)

```python
# 加载记忆
conversation_history = load_memory()

# 保存记忆
save_memory(conversation_history, role_system)
```

**知识点：**
- **外部记忆系统**：将对话历史保存到文件中，程序重启后可以恢复
- **为什么需要？**
  - 程序关闭后，内存中的对话历史会丢失
  - 有了记忆系统，下次启动程序时可以继续之前的对话
  - 就像人类的记忆一样，可以"记住"之前说过的话
- **实现方式：**
  - 使用JSON文件存储对话历史
  - 程序启动时加载历史
  - 每次对话后保存到文件
- **JSON文件格式：**
  ```json
  {
    "role_system": "系统提示词",
    "history": [对话历史列表],
    "last_update": "最后更新时间"
  }
  ```

### 六十六、字符串包含检查 - in 操作符 (5.py)

```python
if user_input in ['再见']:
    break

if "再见" in reply_cleaned:
    # ...
```

**知识点：**
- `元素 in 列表` - 检查元素是否在列表中
- `"子串" in 字符串` - 检查子串是否在字符串中
- 返回布尔值（True/False）
- 常用于条件判断、搜索等

### 六十七、逻辑运算符 - or (5.py)

```python
if reply_cleaned == "再见" or (len(reply_cleaned) <= 5 and "再见" in reply_cleaned):
    break
```

**知识点：**
- `or` - 逻辑或运算符
- `条件1 or 条件2` - 如果条件1为True，返回True；否则返回条件2的值
- 只要有一个条件为True，整个表达式就为True
- 常用于多个条件中至少满足一个的情况

### 六十八、逻辑运算符 - and (5.py)

```python
if len(reply_cleaned) <= 5 and "再见" in reply_cleaned:
    # ...
```

**知识点：**
- `and` - 逻辑与运算符
- `条件1 and 条件2` - 如果条件1为False，返回False；否则返回条件2的值
- 只有所有条件都为True，整个表达式才为True
- 常用于多个条件必须同时满足的情况

### 六十九、函数返回列表 (5.py)

```python
def load_memory():
    if os.path.exists(MEMORY_FILE):
        # ...
        return history
    else:
        return []
```

**知识点：**
- 函数可以返回列表
- `return []` - 返回空列表
- `return history` - 返回包含数据的列表
- 调用函数后，可以用变量接收返回值
- 常用于获取数据、处理结果等

### 七十、函数参数传递 (5.py)

```python
def save_memory(conversation_history, role_system):
    # 使用参数
    pass

save_memory(conversation_history, role_system)
```

**知识点：**
- 函数可以接收多个参数
- 调用函数时，按顺序传入参数
- 参数可以是任何类型（列表、字符串、字典等）
- 函数内部可以使用这些参数进行计算或操作

---

## 综合应用示例

### 多轮对话系统 (4.py)
- 结合 `while True` 循环、`input()`、`append()` 维护对话历史
- 使用 `random.choices()` 带权重随机选择角色
- 使用 `in` 操作符检查游戏结束条件
- 调用自定义模块 `xunfei_tts` 进行语音合成

### 外部记忆系统 (5.py)
- 使用JSON文件持久化存储对话历史
- 使用 `json.load()` 和 `json.dump()` 读写JSON文件
- 使用 `os.path.exists()` 检查文件是否存在
- 使用 `try-except` 处理文件读写异常
- 使用 `dict.get()` 安全获取字典值
- 使用列表切片 `[1:]` 跳过系统消息
- 使用列表拼接组合消息
- 使用 `while True` 和 `break` 实现对话循环
- 使用 `KeyboardInterrupt` 优雅处理用户中断
- 实现程序重启后恢复对话的功能

### 语音合成模块 (xunfei_tts.py)
- 使用类封装WebSocket参数
- 使用线程实现异步网络通信
- 使用文件操作保存和播放音频
- 使用平台检测实现跨平台音频播放
- 使用异常处理优雅处理各种错误情况

### API调用封装 (grn.py, 4.py, 3.PY, 5.py)
- 使用环境变量安全存储API密钥
- 使用函数封装API调用逻辑
- 使用异常处理处理API错误
- 使用字典和列表构建请求数据
- 使用 `role="system"` 设置AI系统提示

---

## 完整学习要点回顾

1. ✅ 变量赋值和基本数据类型
2. ✅ 运算符优先级和表达式求值
3. ✅ 函数定义和参数（默认参数、私有函数）
4. ✅ 字典和列表的使用（append、get、in操作符）
5. ✅ 模块导入（import、from...import、自定义模块）
6. ✅ API调用和HTTP请求
7. ✅ 条件判断和异常处理（if-elif-else、try-except、continue）
8. ✅ 字符串格式化（f-string、format、%）
9. ✅ 循环语句（while True、break、continue）
10. ✅ 随机选择（random.choice、random.choices）
11. ✅ 环境变量（os.environ.get）
12. ✅ 主程序入口（if __name__ == "__main__"）
13. ✅ 面向对象编程（类、__init__、self）
14. ✅ 全局变量（global关键字）
15. ✅ 文件操作（open、exists、makedirs、remove）
16. ✅ 时间操作（time.time、time.sleep、datetime）
17. ✅ WebSocket编程
18. ✅ 线程编程（_thread）
19. ✅ Base64编码/解码
20. ✅ 加密和哈希（hashlib、hmac）
21. ✅ 平台检测（platform.system）
22. ✅ 子进程（subprocess）
23. ✅ lambda表达式
24. ✅ 上下文管理器（with语句）
25. ✅ 字符串方法（strip、index、encode、decode、replace）
26. ✅ 多行字符串（三引号）
27. ✅ URL解析（urlencode）
28. ✅ JSON文件读写（json.load、json.dump）
29. ✅ 文件存在检查（os.path.exists）
30. ✅ 列表切片和拼接（[1:]、+ 运算符）
31. ✅ 布尔判断（not运算符、空列表判断）
32. ✅ 用户输入（input函数）
33. ✅ 异常处理（KeyboardInterrupt、多个except）
34. ✅ 日期时间格式化（datetime.strftime）
35. ✅ 逻辑运算符（and、or）
36. ✅ 系统消息角色（role="system"）
37. ✅ 外部记忆系统（持久化存储）
38. ✅ 对话历史管理
39. ✅ 字符串方法链式调用
40. ✅ 函数内部导入
41. ✅ 常量定义