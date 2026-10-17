using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }

        public int AuthorId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? SubTitle { get; set; }
    }
}

