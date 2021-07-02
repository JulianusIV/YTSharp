# YTSharp
 
YouTube API-Client to get all uploads from a channel

## Usage

YTSharp [API_KEY] [CHANNEL_ID] [AMOUNT_TO_FETCH]

## Returns

A json object with this template:
[{
 "Name": "Title of the video",
 "URL": "URL postfix of the video",
 "CreationTime": "Upload time of the video",
 "Description": "Description of the video"
}]

### Time format

The CreationTime timestamp format complies with [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601).
Timezone is UTC