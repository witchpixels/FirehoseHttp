using WitchPixels.FirehoseHttp.Batching;

namespace WitchPixels.FirehoseHttp.Tests.Batching;

public class JsonArrayBatchDecoderTests
{
    private readonly JsonArrayBatchDecoder _jsonArrayBatchDecoder = new();

    [Fact]
    public async Task CanDecodeWellFormedBatch()
    {
        var event1 = $"{{\n  \"eventId\": \"event\",\n  \"timestamp\": \"{DateTime.Now}\",\n  \"data\": {{\n    \"content\": \"string\",\n    \"value\": \"string of words\"\n  }}}}";
        var event2 = $"{{\n  \"eventId\": \"event2\",\n  \"timestamp\": \"{DateTime.Today}\",\n  \"data\": {{\n    \"content\": \"string\",\n    \"value\": \"another string of words\"\n  }}}}";
        
        var events = $"[\n  {event1},\n  {event2}\n]";
        
        var ctx = HttpContextUtil.CreateWithBody(events);
        ctx.Request.ContentType = "application/json";
        var results = (await _jsonArrayBatchDecoder.Decode(ctx.Request)).ToArray();
        
        Assert.Equal(2, results.Length);
        
        Assert.Equal(event1, results[0]);
        Assert.Equal(event2, results[1]);
    }

    [Fact]
    public async Task FunkyButSupportedJsonArrayOfStrings()
    {
        var events = new[]
        {
            "one fish",
            "two fish",
            "red fish",
            "blue fish"
        };

        var body = $"[\n  {string.Join(",\n  ",events.Select(x => $"\"{x}\""))}\n]";
        
        var ctx = HttpContextUtil.CreateWithBody(body);
        ctx.Request.ContentType = "application/json";
        var results = (await _jsonArrayBatchDecoder.Decode(ctx.Request)).ToArray();
        
        Assert.Equal(events, results);
    }
}