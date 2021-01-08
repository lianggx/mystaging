using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class PageViewModel
    {
        /// <summary>
        ///  页码，默认=1
        /// </summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>
        ///  每页查询数量，默认=10
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

    public class UserListViewModel : PageViewModel
    {
        /// <summary>
        ///  状态，-1=全部，0=正常，1=未激活，2=冻结，3=删除
        /// </summary>
        [Required] public int State { get; set; } = 0;
        /// <summary>
        ///  名称，搜索条件，非模糊搜索
        /// </summary>
        public string Name { get; set; }
    }
}
