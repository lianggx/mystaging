using MyStaging.xUnitTest.Models;
using Xunit;

namespace MyStaging.Common
{
    public class CheckNotNullTest
    {
        [Fact]
        public void NotNull()
        {
            var user = new UserModel { Id = 1 };
            CheckNotNull.NotNull(user, nameof(user));

            user = null;
            CheckNotNull.NotNull(user, nameof(user));
        }

        [Fact]
        public void NotEmpty()
        {
            var name = string.Empty;
            CheckNotNull.NotEmpty(name, nameof(name));
        }
    }
}
