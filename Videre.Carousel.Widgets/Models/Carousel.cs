using System;
using System.Collections.Generic;

namespace Videre.Carousel.Widgets.Models
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
        //public string Type { get; set; }
        public string PortalId { get; set; }
        public List<CarouselItem> Items { get; set; }
    }
}