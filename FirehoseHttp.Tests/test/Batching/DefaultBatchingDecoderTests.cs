using WitchPixels.FirehoseHttp.Batching;

namespace WitchPixels.FirehoseHttp.Tests.Batching;

public class DefaultBatchDecoderTests
{
    private readonly DefaultBatchDecoder _defaultBatchingDecoder = new();

    [Fact]
    public async Task HandlesSingleValues()
    {
        const string expectedValue = "Test";
        
        var ctx = HttpContextUtil.CreateWithBody(expectedValue);
        var result = await _defaultBatchingDecoder.Decode(ctx.Request);

        var value = Assert.Single(result);
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public async Task HandlesMultipleValues()
    {
        var expectedResults = new[]
        {
            "TestValue1",
            "TestValue2",
            "TestValue3",
            "ect",
            "{ \"some\": \"Json for \\n shits\" }",
            "well thanks for reading my code <3"
        };
        
        var body = string.Join("\n\r\n", expectedResults);
        var ctx = HttpContextUtil.CreateWithBody(body);
        
        var result = (await _defaultBatchingDecoder.Decode(ctx.Request)).ToArray();
        
        Assert.Equal(expectedResults.Length, result.Length);
        Assert.Equal(expectedResults, result);
    }
}
