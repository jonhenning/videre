using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Videre.Core.Widgets.Models
{
    public class Carousel
    {
        public Carousel()
        {
            Items = new List<CarouselItem>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Style { get; set; }
        public string PortalId { get; set; }
        public List<CarouselItem> Items { get; set; }
    }

    public class CarouselItem
    {
        public string ImageUrl { get; set; }
        public string Text { get; set; }    //TODO: LOCALIZE!
    }


}