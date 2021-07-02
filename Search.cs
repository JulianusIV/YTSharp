using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YTSharp
{
	internal class Search
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                new Search().Run(args).Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.Error.WriteLine("Error: " + e.Message);
                }
            }
        }

        private async Task Run(string[] args)
        {
            //configure yt api client
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = args[0],
                ApplicationName = this.GetType().ToString()
            });

            //create a channel request for the channel
            var channelListRequest = youtubeService.Channels.List("contentDetails");
            //get channelid from args
            channelListRequest.Id = args[1];
            //execute request and get the channels uploads playlist from the result
            var channelResponse = await channelListRequest.ExecuteAsync();
            string uploadsID = channelResponse.Items.First().ContentDetails.RelatedPlaylists.Uploads;

            //create a playlist item request
            var playlistListRequest = youtubeService.PlaylistItems.List("snippet,contentDetails");
            //set playlistid to channels uploads id and set MaxResults from args
            playlistListRequest.PlaylistId = uploadsID;
            playlistListRequest.MaxResults = int.Parse(args[2]);
            //Create enumerable for videos
            List<Video> videos = new();
            //for each available page of the response amount (defined in args)
            PlaylistItemListResponse playlistResponse = null;
            for (int i = 0; i < (int)Math.Ceiling(double.Parse(args[2]) / 50); i++)
			{
                if (playlistResponse is not null)
                    playlistListRequest.PageToken = playlistResponse.NextPageToken;
                //execute request
                playlistResponse = await playlistListRequest.ExecuteAsync();
                //loop response items and add them to the list
				foreach (var item in playlistResponse.Items)
				{
                    videos.Add(new Video
                    {
                        Name = item.Snippet.Title,
                        Description = item.Snippet.Description,
                        CreationTime = item.ContentDetails.VideoPublishedAt.Value.ToUniversalTime(),
                        URL = item.Snippet.ResourceId.VideoId
                    });
				}
			}
            //serialize and output the enumerable
            string output = JsonSerializer.Serialize(videos);
            Console.WriteLine(output);
        }
    }
}
