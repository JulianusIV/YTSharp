using System.Collections.Generic;

namespace YTSharp.Models
{
	class Playlist
	{
		public string ID { get; set; }
		public string Name { get; set; }
		public List<Video> Videos { get; set; }
	}
}
