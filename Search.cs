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
            //try
            //{
				switch (args[0].ToUpper())
				{
                    case "UPLOADS":
				        new Search().RunUploads(args).Wait();
                        break;
                    case "PLAYLISTS":
                        new Search().RunPlaylists(args).Wait();
                        break;
					default:
						Console.WriteLine("todo add help");
						break;
				}
            //}
            //catch (AggregateException ex)
            //{
            //    foreach (var e in ex.InnerExceptions)
            //    {
            //        Console.Error.WriteLine("Error: " + e.Message);
            //    }
            //}
        }

		private async Task RunUploads(string[] args)
        {
            //configure yt api client
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = args[Array.IndexOf(args, "-k") + 1],
                ApplicationName = this.GetType().ToString()
            });

            //create a channel request for the channel
            var channelListRequest = youtubeService.Channels.List("contentDetails");
            //get channelid from args
            channelListRequest.Id = args[Array.IndexOf(args, "-c") + 1];
            //execute request and get the channels uploads playlist from the result
            var channelResponse = await channelListRequest.ExecuteAsync();
            string uploadsID = channelResponse.Items.First().ContentDetails.RelatedPlaylists.Uploads;

            //create a playlist item request
            var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet,contentDetails");
            //set playlistid to channels uploads id and set MaxResults from args
            playlistItemsListRequest.PlaylistId = uploadsID;
            playlistItemsListRequest.MaxResults = args[Array.IndexOf(args, "-a") + 1].ToUpper() == "ALL" ? 50 : int.Parse(args[Array.IndexOf(args, "-a") + 1]);
            //Create enumerable for videos
            List<Models.Video> videos = new();
            PlaylistItemListResponse playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();
            //for each available page of the response amount (defined in args)
            for (int i = 0; i < Math.Ceiling(double.Parse(GetAmount(args, playlistItemsListResponse.PageInfo.TotalResults.ToString())) / 50); i++)
            {
                //loop response items and add them to the list
                foreach (var item in playlistItemsListResponse.Items)
                {
                    //private videos have no timestamp lol
                    if (item.ContentDetails.VideoPublishedAt == null)
                        continue;
                    //Add video to list
                    videos.Add(new Models.Video
					{
                        Name = item.Snippet.Title,
                        Description = item.Snippet.Description,
                        CreationTime = item.ContentDetails.VideoPublishedAt.Value.ToUniversalTime(),
                        URL = item.Snippet.ResourceId.VideoId
                    });
                }
                //ready for next iteration if not on last iteration
				if (i != Math.Ceiling(double.Parse(GetAmount(args, playlistItemsListResponse.PageInfo.TotalResults.ToString())) / 50) - 1)
				{
                    playlistItemsListRequest.PageToken = playlistItemsListResponse.NextPageToken;
                    playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();
				}
            }
            //remove elements over specified limit
            if (args[Array.IndexOf(args, "-a") + 1].ToUpper() != "ALL")
                videos.RemoveRange(int.Parse(args[Array.IndexOf(args, "-a") + 1]), videos.Count - int.Parse(args[Array.IndexOf(args, "-a") + 1]));
            //serialize and output the enumerable
            string output = JsonSerializer.Serialize(videos);
            Console.WriteLine(output);
        }

        private async Task RunPlaylists(string[] args)
        {
            //configure yt api client
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = args[Array.IndexOf(args, "-k") + 1],
                ApplicationName = this.GetType().ToString()
            });

            //create a playlist request for all playlists of a channel
            var playlistListRequest = youtubeService.Playlists.List("snippet");
            playlistListRequest.ChannelId = args[Array.IndexOf(args, "-c") + 1];
            playlistListRequest.MaxResults = 50;

            List<Models.Playlist> playlists = new();
            var playlistListResponse = await playlistListRequest.ExecuteAsync();
            //for each available page of the response amount
            for (int i = 0; i < Math.Ceiling((double)playlistListResponse.PageInfo.TotalResults / 50); i++)
			{
                //for each playlists of the channel
				foreach (var item in playlistListResponse.Items)
				{
                    //create new playlist entry
                    Models.Playlist playlist = new()
                    {
                        ID = item.Id,
                        Name = item.Snippet.Title,
                        Videos = new()
                    };
                    //build playlist request
                    var playlistItemListRequest = youtubeService.PlaylistItems.List("snippet,contentDetails");
                    playlistItemListRequest.PlaylistId = item.Id;
                    playlistItemListRequest.MaxResults = 50;

                    var playlistItemListResponse = await playlistItemListRequest.ExecuteAsync();
                    //for each available page of the response amount
                    for (int inside = 0; inside < Math.Ceiling((double)playlistItemListResponse.PageInfo.TotalResults / 50); inside++)
					{
                        //foreach video in the playlist
						foreach (var video in playlistItemListResponse.Items)
						{
                            //private videos have no timestamp lol
                            if (video.ContentDetails.VideoPublishedAt == null)
                                continue;
                            //add video to playlist
                            playlist.Videos.Add(new()
                            {
                                Name = video.Snippet.Title,
                                Description = video.Snippet.Description,
                                CreationTime = video.ContentDetails.VideoPublishedAt.Value.ToUniversalTime(),
                                URL = video.Snippet.ResourceId.VideoId
                            });
						}
                        //ready for next iteration if not on last iteration
						if (inside != Math.Ceiling((double)playlistItemListResponse.PageInfo.TotalResults / 50) - 1)
						{
                            playlistItemListRequest.PageToken = playlistItemListResponse.NextPageToken;
                            playlistItemListResponse = await playlistItemListRequest.ExecuteAsync();
						}
					}
                    //add playlist item to list
                    playlists.Add(playlist);
				}
                //ready for next iteration if not on last iteration
                if (i != Math.Ceiling((double)playlistListResponse.PageInfo.TotalResults / 50) - 1)
				{
                    playlistListRequest.PageToken = playlistListResponse.NextPageToken;
                    playlistListResponse = await playlistListRequest.ExecuteAsync();
				}
			}
            //serialize and output the list
            string output = JsonSerializer.Serialize(playlists);
			Console.WriteLine(output);
        }

        /// <summary>
        /// get amount from args and return it or a fallback if not defined
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="fallbackAmount">value to use if no args defined</param>
        /// <returns>amount or fallback</returns>
        private static string GetAmount(string[] args, string fallbackAmount)
		{
            return args[Array.IndexOf(args, "-a") + 1].ToUpper() == "ALL" ? fallbackAmount : args[Array.IndexOf(args, "-a") + 1];
        }
    }
}
