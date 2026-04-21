namespace Configs
{
    /// <summary>
    /// 所有配表数据类的统一接口，强制要求有一个唯一主键 Id
    /// </summary>
    public interface IConfig
    {
        int Id { get; }
    }
}