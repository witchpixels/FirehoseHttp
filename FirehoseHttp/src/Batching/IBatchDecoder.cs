namespace WitchPixels.FirehoseHttp.Batching;

public interface IBatchDecoder
{
    Task<IEnumerable<string>> Decode(HttpRequest request);
}