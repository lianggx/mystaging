using Mysql.Model;
using MyStaging.Function;
using System.Collections.Generic;

namespace Mysql.Services
{
    public class ArticleService
    {
        private readonly MysqlDbContext dbContext;
        public ArticleService(MysqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<Article> List(PageModel model)
        {
            var build = dbContext.Article.Select.Page(model.PageIndex, model.PageSize).OrderByDescing(f => f.createtime);
            if (model.UserId > 0)
            {
                build.Where(f => f.userid == model.UserId);
            }

            if (!string.IsNullOrEmpty(model.Keyword))
            {
                build.Where(f => f.title.Like(model.Keyword));
            }

            return build.ToList();
        }

        public Article Detail(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            var detail = dbContext.Article.Select.Where(f => f.id == id).ToOne();

            return detail;
        }

        public Article Add(Article model)
        {

            var detail = dbContext.Article.Select.Where(f => f.id == model.id).ToOne();

            return detail;
        }

        public Article Update(int id, string title, string content)
        {
            var article = dbContext.Article.Select.Where(f => f.id == id).ToOne();
            if (article == null)
                throw new KeyNotFoundException($"找不到Id={id} 的记录");

            article = dbContext.Article.Update.SetValue(f => f.content, content)
                                                         .SetValue(f => f.title, title)
                                                         .Where(f => f.id == article.id)
                                                         .SaveChange();

            return article;
        }

        public bool Delete(int id)
        {
            var article = dbContext.Article.Select.Where(f => f.id == id).ToOne();
            if (article == null)
                throw new KeyNotFoundException($"找不到Id={id} 的记录");

            var affrows = dbContext.Article.Delete.Where(f => f.id == id).SaveChange();

            return affrows > 0;
        }
    }
}
