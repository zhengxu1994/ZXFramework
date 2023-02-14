### Unity开发微信小游戏

https://developers.weixin.qq.com/minigame/dev/guide/#安装并启动开发者工具

https://github.com/wechat-miniprogram/minigame-unity-webgl-transform unity适配微信小游戏git

API：

- 微信小游戏skd初始化，只有初始化完毕才能使用其他功能

  ```c#
  WX.InitSDK((code)=>{
      
  });
  ```

- 打印屏幕信息

  ```c#
   var systemInfo = WeChatWASM.WX.GetSystemInfoSync();
              Debug.Log($"{systemInfo.screenWidth}:{systemInfo.screenHeight}, {systemInfo.windowWidth}:{systemInfo.windowHeight}, {systemInfo.pixelRatio}");
  ```

  

- 微信激励视频广告

  ```c#
  public WXRewardedVideoAd ad;
   
  ad = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam(){
      adUnitId = "";//自己申请的广告单位id
  });
  
  //一些回调方法
  ad.OnError((r)=>{});
  
  ad.OnClose((r)=>{});
  //播放
   ad.Show();
  ```

- 请求用户授权

  ```c#
  var infoButton = WX.CreateUserInfoButton(0, canvasHeight - buttonHeight, canvasWith, buttonHeight, "zh_CN", false);
              infoButton.OnTap((userInfoButonRet) =>
              {
                  Debug.Log(JsonUtility.ToJson(userInfoButonRet.userInfo));
                  txtUserInfo.text = $"nickName：{userInfoButonRet.userInfo.nickName}， avartar:{userInfoButonRet.userInfo.avatarUrl}";
              });
  ```

- 获取小游戏打开的参数

  ```c
   var options = WX.GetEnterOptionsSync();
   Debug.Log("GetEnterOptionsSync scene:" + options.scene);
  ```

- 主动转发分享

  ```c#
  WX.ShareAppMessage(new ShareAppMessageOption()
          {
              title = "分享标题xxx",
              imageUrl = "https://inews.gtimg.com/newsapp_bt/0/12171811596_909/0",
  
          });
  ```

- 发起米大师支付

  ```c#
    WX.RequestMidasPayment(new RequestMidasPaymentOption()
          {
              mode = "game",
              env = 0,
              offerId = "xxxx", //在米大师侧申请的应用 id
              currencyType = "CNY",
              success = (res) =>
              {
                  Debug.Log("pay success!");
              },
              fail = (res) =>
              {
                  Debug.Log("pay fail:" + res.errMsg);
              }
          });
  ```

- 播放音频

  ```c#
         inneraudio = WX.CreateInnerAudioContext(new InnerAudioContextParam()
          {
              src = "Sounds/Seagull 002.wav",
              needDownload = true,
          });
          inneraudio.OnEnded(() =>
          {
              Debug.Log("OnEnded called, play again");
              inneraudio.Play();
          });
          inneraudio.OnCanplay(() =>
          {
              Debug.Log("OnCanplay called");
              inneraudio.Play();
          });
  
  //停止播放
  inneraudio.Stop();
  ```

- 事件上报

  ```c#
    Dictionary<string, int> videoReport = new Dictionary<string, int>();
    videoReport.Add("video_maidian", 1);
    Debug.Log("video maidian 1");
    WX.ReportEvent("exptnormal", videoReport);
  ```

- 扫描文件系统目录 并创建目录 写入信息 读取信息

  ```c#
  public WXFileSystemManager fs = new WXFileSystemManager();
  public WeChatWASM.WXEnv env = new WXEnv();//微信环境
  fs.Stat(new WXStatOption
          {
              path = env.USER_DATA_PATH + "/__GAME_FILE_CACHE",
              recursive = true,
              success = (succ) =>
              {
                  Debug.Log($"stat success");
                  foreach (var file in succ.stats)
                  {
                      Debug.Log($"stat info. {file.path}, " +
                          $"{file.stats.size}，" +
                          $"{file.stats.mode}，" +
                          $"{file.stats.lastAccessedTime}，" +
                          $"{file.stats.lastModifiedTime}");
                  }
              },
              fail = (fail) =>
              {
                  Debug.Log($"stat fail {fail.errMsg}");
              }
          });
      // 同步接口创建目录（请勿在游戏过程中频繁调用同步接口）
      var errMsg = fs.MkdirSync(env.USER_DATA_PATH + "/mydir", true);
  
  
          // 异步写入文件
          fs.WriteFile(new WriteFileParam
          {
              filePath = env.USER_DATA_PATH + "/mydir/myfile.txt",
              encoding = "utf8",
              data = System.Text.Encoding.UTF8.GetBytes("Test FileSystemManager"),
              success = (succ) =>
              {
                  Debug.Log($"WriteFile succ {succ.errMsg}");
                   // 异步读取文件
                  fs.ReadFile(new ReadFileParam
                  {
                      filePath = env.USER_DATA_PATH + "/mydir/myfile.txt",
                      encoding = "utf8",
                      success = (succ) =>
                      {
                          Debug.Log($"ReadFile succ. stringData(utf8):{succ.stringData}");
                      },
                      fail = (fail) =>
                      {
                          Debug.Log($"ReadFile fail {fail.errMsg}");
                      }
                  });
  
                  // 异步以无编码(二进制)方式读取
                  fs.ReadFile(new ReadFileParam
                  {
                      filePath = env.USER_DATA_PATH + "/mydir/myfile.txt",
                      encoding = "",
                      success = (succ) =>
                      {
                          Debug.Log($"ReadFile succ. data(binary):{succ.binData.Length}, stringData(utf8):{System.Text.Encoding.UTF8.GetString(succ.binData)}");
                      },
                      fail = (fail) =>
                      {
                          Debug.Log($"ReadFile fail {fail.errMsg}");
                      }
                  });
  
              },
              fail = (fail) =>
              {
                  Debug.Log($"WriteFile fail {fail.errMsg}");
              },
              complete = null
          });
  
  ```

- 同步接口 写入一些本地参数并保存 跟unity的本地接口差不多

  ```c#
          // 注意！！！ PlayerPrefs为同步接口，iOS高性能模式下为"跨进程"同步调用，会阻塞游戏线程，请避免频繁调用
          PlayerPrefs.SetString("mystringkey", "myestringvalue");
          PlayerPrefs.SetInt("myintkey", 123);
          PlayerPrefs.SetFloat("myfloatkey", 1.23f);
  
          Debug.Log($"PlayerPrefs mystringkey:{ PlayerPrefs.GetString("mystringkey")}");
          Debug.Log($"PlayerPrefs myintkey:{ PlayerPrefs.GetInt("myintkey")}");
          Debug.Log($"PlayerPrefs myfloatkey:{ PlayerPrefs.GetFloat("myfloatkey")}"); 
  ```

- 分享对局回放

  ```c#
    WX.OperateGameRecorderVideo(new operateGameRecorderOption()
          {
              title = "游戏标题",
              desc = "游戏简介",
              timeRange = new int[][] {
                new int[] { 0, 2000 },
                new int[] { 5000, 8000 },
              },
              query = "test=123456",
          });
  ```

- 预下载音频

  ```c#
          WX.PreDownloadAudios(audioList, (int res) =>
          {
              if (res == 0)
              {
                  // 下载成功
  
                  // 下载后播放第2个音频
                  // playAfterDownload(1);
              }
              else
              {
                  // 下载失败
              }
          });
  ```

- 播放音频

  ```c#
  var audioIndex = createAudio();
  
          if (audioIndex == null)
          {
              return;
          }
  
          // 如果要设置的src和原音频对象一致，可以直接播放
          if (audioIndex.src == audioList[index])
          {
              audioIndex.Play();
          }
          else
          {
              // 对于已经设置了needDownload为true的audio，设置src后就会开始下载对应的音频文件
              // 如果该文件已经下载过，并且配置了缓存本地，就不会重复下载
              // 如果该文件没有下载过，等同于先调用WX.PreDownloadAudios下载后再播放
              audioIndex.src = audioList[index];
  
              // 短音频可以直接调用Play
              if (isShort)
              {
                  audioIndex.Play();
              }
              else
              {
                  // 长音频在可以播放时播放
                  audioIndex.OnCanplay(() =>
                  {
                      audioIndex.Play();
                  });
              }
          }
  ```

  