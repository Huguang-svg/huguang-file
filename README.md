# Python 基础知识总结

本文档总结了今天学习的两个Python文件中的基础知识。

## 文件概览

### 1. 101.py - 基础变量和运算符
### 2. grn.py - 函数定义和API调用

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
