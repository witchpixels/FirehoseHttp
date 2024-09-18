FirehoseHTTP
------------
And Containerized Http Interface for AWS Firehose for testing, or very very basic event ingestion.

Created basically for the singular purpose of testing analytics collectors in various engines, namely Unreal.

# Usage

## Configuration
The primary intended use of this is as a docker container, but in either containerized mode, or running on a bare VM you
will need to set the following environment variables:

| Environment Var | Example                                  | Description                                                                                              |
|-----------------|------------------------------------------|----------------------------------------------------------------------------------------------------------|
| `ACCESS_KEY`    | *a bunch of capital letters and numbers* | Get this from IAM Console, should be for a user with Put* access to the firehose stream you want to use. |
| `ACCESS_SECRET` | *bunch more random chars*                | The Secret from the `ACCESS_KEY` variable.                                                               |
| `STREAM_NAME`   | MyCoolFirehoseStream                     | Whatever you named the firehose stream on AWS, **not the full ARN**.                                     |
| `STREAM_REGION` | ca-central-1                             | The region you created the stream in. Not the human friendly one, but the region code.                   |

You are going to want to make sure the `ACCESS_KEY` and `ACCESS_SECRET` belong to a user that has access to your 
Firehose Stream.

## API
The following routes are provided

### `POST /put`
Takes any content type and directly streams whatever is in your post body into the Firehose client, unvalidated.

#### Responses
It will either respond with `200 OK` with no response body, or will echo back the response from AWS as well as the error 
details.


### `POST /putBatch`
Batch uses, as you might expect, the `PutBatch` Firehose Client function and provides two ways of delimiting batches.

#### Unprocessed (default behavior)
You can send it newline-delimited (any newline format) text bodies which it will split on those newlines, submitting 
each line as its own message batch. Use this if you are not sending JSON, or well formatted json.

#### Json Array Deconstruction
If you set the content type to `application/json` it will try to parse the body as a JSON array.

Provided the body is an array, each object in the array will be submitted as its own record in the batch without any 
further processing. 

#### Responses
It will either respond with `200 OK` with no response body, or will echo back the response from AWS as well as the error 
details.

