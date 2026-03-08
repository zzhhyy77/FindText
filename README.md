# 找文本 （WPF）
在文件夹内搜索字符的工具。

#### 为什么需要这工具
**当要钉钉子的时候，才发现自己手里还没有一把锤子**</br>
虽然现在AI越来越强，甚至可以把整个电脑都交给AI管理，但一些简单场景只需要使用简单的工具。

闲暇时间弄了几个单机版页游，除了玩玩怀旧，还想学学游戏是怎么做的，在游戏中看到“禁随机”等提示消息，很自然想到肯定存在某个文件里，但真想搜索时发现并不容易。
1. 用Windows搜索，启用文件内容选项，进度条跑得慢，搜出来的文件还不相关
2. 下载了Everything，发现不能搜文件内容（可能是我不会用）
3. 又下载了Anytxt，什么都还没搜，先开了后台任务建索引，操作也不直观

我只是想 **指定一个文件夹，输入关键字就能搜索出相关的文件**，
</br>越绿色越好，用完了就关掉，下次打开还能继续接着用，不需要索引，因为速度不是关键指标。

#### 界面截图

<img src="https://github.com/zzhhyy77/FindText/blob/master/mandwnd.png" title="界面截图" style="zoom💯%;"/>


## 其他
内置了两个颜色主题
（并非花哨功能，程序放到虚拟机上发现字看不清，只能添加颜色主题解决此问题）
</br> 改控件样式太耗时，真正的功能部分没花多少时间，但控件样式搞了好几天，
</br>项目包括常用的控件样式，都包含在 CiontrolStyles.xaml：
1. TextBox
2. Buttons
3. ToggleButton
4. CheckBoxStyle
5. RadioButton
6. ComboBox
7. Listbox
8. ListView
9. TabControl
10. DataGrid
11. ScrollViewer
12. ScrollBar
13. Thumb
其他控件暂时用不上

## 依赖环境
1. dotNet8
2. NuGet 引用了 System.Text.Encoding.CodePages（原则上尽可能减少依赖， 但默认支持的编码太少，只能增加一个引用）

