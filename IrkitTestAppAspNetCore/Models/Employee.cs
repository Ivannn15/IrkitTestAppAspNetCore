using System;
using System.Collections.Generic;

#nullable disable

namespace IrkitTestAppAspNetCore
{
    public partial class Employee
    {
        public int Id { get; set; }
        public string AuthorId { get; set; }
        public string Content { get; set; }
        public string FirstName { get; set; }
        public string KindName { get; set; }
        public string Performer { get; set; }
        public string ReferenceList { get; set; }
        public DateTime RegDate { get; set; }
    }
}
