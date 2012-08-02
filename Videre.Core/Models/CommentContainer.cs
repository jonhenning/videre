using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Models
{
    public class CommentContainer
    {
        public CommentContainer()
        {
            Comments = new List<Comment>();
        }

        public string Id { get; set; }
        public string PortalId { get; set; }
        public string ContainerType { get; set; }
        public string ContainerId { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
