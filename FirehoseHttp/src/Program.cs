using Amazon;
using Amazon.KinesisFirehose;
using Amazon.Runtime;
using WitchPixels.FirehoseHttp;
using WitchPixels.FirehoseHttp.Batching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

var firehoseKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
var firehoseSecret = Environment.GetEnvironmentVariable("ACCESS_SECRET");

var firehoseStreamName = Environment.GetEnvironmentVariable("STREAM_NAME");
var firehoseRegion = Environment.GetEnvironmentVariable("STREAM_REGION");

if (string.IsNullOrEmpty(firehoseKey)) throw new Exception("Please set environment variable ACCESS_KEY");
if (string.IsNullOrEmpty(firehoseSecret)) throw new Exception("Please set environment variable ACCESS_SECRET");
if (string.IsNullOrEmpty(firehoseStreamName)) throw new Exception("Please set environment variable STREAM_NAME");
if (string.IsNullOrEmpty(firehoseRegion)) throw new Exception("Please set environment variable STREAM_REGION");

var firehoseClient = new AmazonKinesisFirehoseClient(
    new BasicAWSCredentials(firehoseKey, firehoseSecret),
    RegionEndpoint.GetBySystemName(firehoseRegion));

var firehoseHttpApi = new FirehoseHttpApi(
    app.Services.GetRequiredService<ILogger<FirehoseHttpApi>>(),
    firehoseClient,
    firehoseStreamName,
    new Dictionary<string, IBatchDecoder>
    {
        { "application/json", new JsonArrayBatchDecoder() }
    },
    new DefaultBatchDecoder());

app.Use(async (ctx, next) =>
{
    try
    {
        await next(ctx);
    }
    catch (Exception e)
    {
        app.Logger.LogError(e, "Uncaught exception had occurred");
        throw;
    }
});


app.MapHealthChecks("/zhealth");
app.MapPost("/put", async httpCtx => { await firehoseHttpApi.SinglePutRequest(httpCtx); });
app.MapPost("/putBatch", async httpCtx => { await firehoseHttpApi.BatchPutRequest(httpCtx); });

app.Run();