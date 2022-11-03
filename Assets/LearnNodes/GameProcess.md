### GameProcess

- #### 游戏初始化

  - 平台初始化 
    - 根据设备，初始化对应平台信息
    - 初始化sdk信息
  - 从游戏服获取当前游戏信息
    - 从游戏服获取当前游戏版本信息和当前最新ab包列表
  - 更新，更新所有ab资源
    - 对应ab包（md5）信息，更新ab包
  - 加载更热代码
    - 从ab包中加载staticdata.bytes 和 game_pdb.bytes，通过System.Reflection.Assembly.Load(dllData,pdbData)加载热更代码。
  - 正式进入游戏
    - 初始化UI信息
    - 加载本地LocalStore信息
    - 初始化消息协议
    - 加载语言信息
    - 进入登录界面

- #### 登录

- #### 大厅

  