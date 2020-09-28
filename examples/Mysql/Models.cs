using System;
using System.Collections.Generic;
using System.Text;

namespace Mysql
{
    public class PageModel
    {
        public int UserId { get; set; }
        public string Keyword { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public struct IdModel
    {
        public int Id { get; set; }
    }
}
