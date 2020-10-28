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
            var build = dbContext.Article.Select.Page(model.PageIndex, model.PageSize).OrderByDescing(f => f.CreateTime);
            if (model.UserId > 0)
            {
                build.Where(f => f.UserId == model.UserId);
            }

            if (!string.IsNullOrEmpty(model.Keyword))
            {
                build.Where(f => f.Title.Like(model.Keyword));
            }

            return build.ToList();
        }

        public Article Detail(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            var detail = dbContext.Article.Select.Where(f => f.Id == id).ToOne();

            return detail;
        }

        public Article Add(Article model)
        {

            var detail = dbContext.Article.Select.Where(f => f.Id == model.Id).ToOne();

            return detail;
        }

        public Article Update(int id, string title, string content)
        {
            var article = dbContext.Article.Select.Where(f => f.Id == id).ToOne();
            if (article == null)
                throw new KeyNotFoundException($"找不到Id={id} 的记录");

            article = dbContext.Article.Update.SetValue(f => f.Content, content)
                                                         .SetValue(f => f.Title, title)
                                                         .Where(f => f.Id == article.Id)
                                                         .SaveChange();

            return article;
        }

        public bool Delete(int id)
        {
            var article = dbContext.Article.Select.Where(f => f.Id == id).ToOne();
            if (article == null)
                throw new KeyNotFoundException($"找不到Id={id} 的记录");

            var affrows = dbContext.Article.Delete.Where(f => f.Id == id).SaveChange();

            return affrows > 0;
        }

        public long Total()
        {
            var total = dbContext.Article.Select.Where(f => f.State == true).Count();

            return total;
        }
    }
}
