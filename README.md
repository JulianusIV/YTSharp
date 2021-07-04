# YTSharp
 
YouTube API-Client to get all uploads from a channel

## Usage

YTSharp COMMAND [OPTIONS]

### Options:

| Prefix | Type   | Description								   | Remarks						|
| ------ | ------ | ------------------------------------------ | ------------------------------ |
| -k	 | string | YouTube ApiKey							   | NEEDED							|
| -c	 | string | ChannelID								   | NEEDED							|
| -a	 | int    | Max amount of videos to fetch (int or all) | GETS IGNORED IN PLAYLIST QUERY |

### Commands:

| Command   | Description											   |
| --------- | -------------------------------------------------------- |
| uploads   | Returns the channels last [AMOUNT] videos				   |
| playlists | Returns all the channels playlists with all their videos |

## Returns

### Uploads

A json object with this template:  
```json
[{  
	"Name": "Title of the video",  
	"URL": "URL postfix of the video",  
	"CreationTime": "Upload time of the video",  
	"Description": "Description of the video"  
}]  
```

### Playlists

A json object with this template:  
```json
[{  
	"ID": "Playlist ID",  
	"Name": "Playlist title",  
	"Videos":  
	[{  
		"Name": "Title of the video",  
		"URL": "URL postfix of the video",  
		"CreationTime": "Upload time of the video",  
		"Description": "Description of the video"  
	}]  
}]
```

## Time format

The CreationTime timestamp format complies with [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601).  
Timezone is UTC