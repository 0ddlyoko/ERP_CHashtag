namespace Test.util;

using lib.util;

public class TestStringUtil
{
    [Fact]
    public void GetOrderedGraph_NoDependencies_ReturnsSingleNodeFirst()
    {
        Assert.Equal("salut", StringUtil.ToSnakeCase("Salut"));
        Assert.Equal("comment_ca_va", StringUtil.ToSnakeCase("CommentCaVa"));
        Assert.Equal("", StringUtil.ToSnakeCase(""));
    }
}
