namespace MyStaging.Interface
{
    public interface ISaveChanged
    {
        /// <summary>
        ///  将当前更改保存到数据库
        /// </summary>
        /// <returns></returns>
        int SaveChange();

        string ToSQL();
    }
}
