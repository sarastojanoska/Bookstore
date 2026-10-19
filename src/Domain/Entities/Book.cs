namespace Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }

        public int AuthorId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? SubTitle { get; set; }
    }
}
