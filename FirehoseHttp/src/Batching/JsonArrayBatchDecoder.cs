namespace WitchPixels.FirehoseHttp.Batching;

public class JsonArrayBatchDecoder : IBatchDecoder
{
    public async Task<IEnumerable<string>> Decode(HttpRequest request)
    {
        var json = await request.ReadFromJsonAsync<object[]>();
        if (json is null) throw new Exception("Unable to parse body, or it was not a array.");

        return json.Select(x => x?.ToString() ?? string.Empty);
    }
}