using System;
using YooAsset.Editor;

/// <summary>
/// 资源加密
/// </summary>
public class GameEncryption : IEncryptionServices
{
    /// <summary>
    /// 检测资源是否需要加密
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public bool Check(string bundleName)
    {
        return bundleName.Contains("asset/config/");
    }

    /// <summary>
    /// 对数据进行加密，并返回加密后的数据
    /// </summary>
    /// <param name="fileData"></param>
    /// <returns></returns>
    public byte[] Encrypt(byte[] fileData)
    {
        int offset = 32;
        var temper = new byte[fileData.Length + offset];
        Buffer.BlockCopy(fileData, 0, temper, offset, fileData.Length);
        return temper;
    }
}
