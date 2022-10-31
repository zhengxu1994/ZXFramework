using System;
namespace Bepop.Core.UI
{
    public enum UILayerEnum
    {
        /// <summary>
        /// 普通UI层
        /// </summary>
        UI = 0,
        /// <summary>
        /// UI常驻界面 如玩家金钱战力显示界面
        /// </summary>
        Top = 1,
        /// <summary>
        /// 覆盖层 如 奖励界面 物品信息介绍界面 一般都是带有半透明背景的
        /// </summary>
        Cover = 2,
        /// <summary>
        /// 提示层，如消息提示框
        /// </summary>
        Tip = 3,
        /// <summary>
        /// 引导层 如 放一些新手引导界面
        /// </summary>
        Guide = 4,
        /// <summary>
        /// 等待层 如 正在请求服务器信息 这个时候什么都不能点
        /// </summary>
        Waiting = 5,

        //层级个数
        Count = 6
    }

    public enum CommonTopState
    {
        Main = 0,
        Normal = 1,
        Shop = 2,
        Battle = 3,
        Activity = 4
    }
}