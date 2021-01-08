using System.ComponentModel.DataAnnotations;

namespace IdentityHost.ViewModel
{
    public class AddResourceViewModel
    {
        /// <summary>
        ///  名称
        /// </summary>
        [Required] public string Name { get; set; }
        /// <summary>
        ///  资源内容
        /// </summary>
        [Required] public string Content { get; set; }
        /// <summary>
        ///  资源类型，0=API，1=网页元素
        /// </summary>
        [Required] public int Type { get; set; }
        /// <summary>
        ///  是否需要授权才允许访问
        /// </summary>
        [Required] public bool Authorize { get; set; }
        /// <summary>
        ///  资源分类编号
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        ///  排序号，倒序排序
        /// </summary>
        public int Sort { get; set; }
    }

    public class EditResourceViewModel : AddResourceViewModel
    {
        /// <summary>
        ///  Id
        /// </summary>
        [Required] public int Id { get; set; }
    }

    public class ResourceListViewModel
    {
        /// <summary>
        ///  资源类型，0=API，1=网页元素，默认=0
        /// </summary>
        [Required] public int Type { get; set; } = 0;
    }

    public class ResourceRootViewModel
    {
        /// <summary>
        ///  资源类型，0=API，1=网页元素，默认= null
        /// </summary>
        public int? Type { get; set; } = null;
    }
}
