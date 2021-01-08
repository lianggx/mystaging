using MyStaging.Core;
using Xunit;

namespace MyStaging.xUnitTest.Core
{
    public class ConnectionManagerTest
    {
        [Fact]
        public void Add()
        {
            ConnectionManager.Add("mysql", ConstantUtil.PGSQL_CONNECTION, false);
            ConnectionManager.Add("mysql", ConstantUtil.PGSQL_CONNECTION, true);
            Assert.Equal(1, ConnectionManager.dict.Keys.Count);
            Assert.Equal(2, ConnectionManager.dict["mysql"].Count);
        }

        [Fact]
        public void Get()
        {
            Add();
            var masterCM = ConnectionManager.Get("mysql", false);
            var slaveCM = ConnectionManager.Get("mysql", true);

            Assert.Equal(masterCM.ConnectionString, ConstantUtil.PGSQL_CONNECTION);
            Assert.Equal(slaveCM.ConnectionString, ConstantUtil.PGSQL_CONNECTION);
        }

        [Fact]
        public void Remove()
        {
            Add();
            ConnectionManager.Remove("mysql");
            Assert.Equal(0, ConnectionManager.dict.Keys.Count);
        }
    }
}
