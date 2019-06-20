using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Resource.Vod.Entity
{
    public class PlayAuth
    {
        public string RequestId { get; set; }
        public string Auth { get; set; }
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string CoverURL { get; set; }
        public float? Duration { get; set; }
    }
}
