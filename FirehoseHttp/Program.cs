using System.Net;
using System.Text;
using Amazon;
using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;
using Amazon.Runtime;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var firehoseKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
var firehoseSecret = Environment.GetEnvironmentVariable("ACCESS_SECRET");

var firehoseStreamName = Environment.GetEnvironmentVariable("STREAM_NAME");
var firehoseRegion = Environment.GetEnvironmentVariable("STREAM_REGION");

var firehoseClient = new AmazonKinesisFirehoseClient(
    new BasicAWSCredentials(firehoseKey, firehoseSecret),
    RegionEndpoint.GetBySystemName(firehoseRegion));

var logger = app.Logger;

app.MapPost("/put", async (httpCtx) =>
{
    logger.LogInformation($"/put {httpCtx.Request.ContentLength} bytes");
    using var ms = new MemoryStream();
    await httpCtx.Request.Body.CopyToAsync(ms);

    var result = await firehoseClient.PutRecordAsync(new PutRecordRequest
    {
        Record = new Record
        {
            Data = ms,
        },
        DeliveryStreamName = firehoseStreamName
    });

    switch (result.HttpStatusCode)
    {
        case HttpStatusCode.OK:
            Results.Ok(result);
            break;
        default:
            Results.Problem(
                result.ToString(),
                firehoseStreamName,
                (int)result.HttpStatusCode);
            break;
    }
});

app.MapPost("/putBatch", async (httpCtx) =>
{
    logger.LogInformation($"/put {httpCtx.Request.ContentLength} bytes");
    var bodyText = await new StreamReader(httpCtx.Request.Body).ReadToEndAsync();

    var req = new PutRecordBatchRequest();
    req.DeliveryStreamName = firehoseStreamName;
    var lines = bodyText.Split(
        ["\r\n", "\r", "\n"],
        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
    );
    req.Records = lines.Select(x => new Record
    {
        Data = new MemoryStream(Encoding.UTF8.GetBytes(x))
    }).ToList();

    var result = await firehoseClient.PutRecordBatchAsync(req);

    switch (result.HttpStatusCode)
    {
        case HttpStatusCode.OK:
            Results.Ok(result);
            break;
        default:
            Results.Problem(
                result.ToString(),
                firehoseStreamName,
                (int)result.HttpStatusCode);
            break;
    }
});

app.Run();