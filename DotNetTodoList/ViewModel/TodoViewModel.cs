namespace DotNetTodoList.ViewModel
{
    public class TodoViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? CategoryID { get; set; }
        public int? PriorityLevel { get; set; }
        public string? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
