namespace DotNetTodoList.ViewModel
{
    public class TodoViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? IDCategory { get; set; }
        public int? Level { get; set; }
        public string? TodoStatus { get; set; }
        public DateTime? TodoDueDate { get; set; }
        public DateTime? TodoCreatedDate { get; set; }
        public DateTime? TodoCompletedDate { get; set; }
    }
}
