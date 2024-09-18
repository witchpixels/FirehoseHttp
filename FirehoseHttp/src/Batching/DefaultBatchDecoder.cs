namespace WitchPixels.FirehoseHttp.Batching;

public class DefaultBatchDecoder : IBatchDecoder
{
    public async Task<IEnumerable<string>> Decode(HttpRequest request)
    {
        var bodyText = await new StreamReader(request.Body).ReadToEndAsync();
        return bodyText.Split(
            ["\r\n", "\r", "\n"],
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
        );
    }
}