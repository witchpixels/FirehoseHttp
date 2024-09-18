using System.Net;
using System.Text;
using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;
using WitchPixels.FirehoseHttp.Batching;

namespace WitchPixels.FirehoseHttp;

public class FirehoseHttpApi(
    ILogger<FirehoseHttpApi> logger,
    AmazonKinesisFirehoseClient firehoseClient,
    string firehoseStreamName,
    IDictionary<string, IBatchDecoder> batchMessageBodyHandlers,
    IBatchDecoder defaultMessageBodyHandler)
{
    public async Task SinglePutRequest(HttpContext httpContext)
    {
        using var ms = new MemoryStream();
        await httpContext.Request.Body.CopyToAsync(ms);

        var result = await firehoseClient.PutRecordAsync(new PutRecordRequest
        {
            Record = new Record
            {
                Data = ms
            },
            DeliveryStreamName = firehoseStreamName
        });

        switch (result.HttpStatusCode)
        {
            case HttpStatusCode.OK:
            case HttpStatusCode.Accepted:
                logger.LogInformation($"{HttpStatusCode.OK} /put {httpContext.Request.ContentLength} bytes");
                Results.Ok(result);
                break;
            default:
                logger.LogInformation($"{result.HttpStatusCode} /put {httpContext.Request.ContentLength} bytes");
                Results.Problem(
                    result.ToString(),
                    firehoseStreamName,
                    (int)result.HttpStatusCode);
                break;
        }
    }

    public async Task BatchPutRequest(HttpContext httpContext)
    {
        logger.LogInformation($"/putBatch {httpContext.Request.ContentLength} bytes");

        if (!batchMessageBodyHandlers.TryGetValue(httpContext.Request.ContentType ?? "", out var handler))
            handler = defaultMessageBodyHandler;

        var records = await handler.Decode(httpContext.Request);

        var req = new PutRecordBatchRequest
        {
            DeliveryStreamName = firehoseStreamName,
            Records = records.Select(x => Encoding.UTF8.GetBytes(x))
                .Select(x => new MemoryStream(x))
                .Select(x => new Record
                {
                    Data = x
                })
                .ToList()
        };

        var result = await firehoseClient.PutRecordBatchAsync(req);

        switch (result.HttpStatusCode)
        {
            case HttpStatusCode.OK:
            case HttpStatusCode.Accepted:
                logger.LogInformation($"{HttpStatusCode.OK} /putBatch {httpContext.Request.ContentLength} bytes");
                Results.Ok(result);
                break;
            default:
                logger.LogInformation($"{result.HttpStatusCode} /putBatch {httpContext.Request.ContentLength} bytes");
                Results.Problem(
                    result.ToString(),
                    firehoseStreamName,
                    (int)result.HttpStatusCode);
                break;
        }
    }
}