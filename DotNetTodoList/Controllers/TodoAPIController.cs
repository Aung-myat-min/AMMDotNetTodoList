using Dapper;
using DotNetTodoList.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace DotNetTodoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoAPIController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=DESKTOP-KPCHONN\\SQLEXPRESS;Initial Catalog=DotNetToDoList;User ID=sa;Password=sasa@123;TrustServerCertificate=True;";
        [HttpGet]
        public IActionResult GetTodos()
        {
            List<TodoViewModel> todos = new List<TodoViewModel>();

            string query = @"SELECT [TaskID]
                              ,[TaskTitle]
                              ,[TaskDescription]
                              ,[CategoryID]
                              ,[PriorityLevel]
                              ,[Status]
                              ,[DueDate]
                              ,[CreatedDate]
                              ,[CompletedDate]
                          FROM [dbo].[ToDoList]";

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            SqlCommand cmd = new SqlCommand(query, connection);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {

                TodoViewModel todo = new TodoViewModel
                {
                    Id = Convert.ToInt32(reader["TaskID"]),
                    Title = Convert.ToString(reader["TaskTitle"]),
                    Description = Convert.ToString(reader["TaskDescription"]),
                    IDCategory = Convert.ToInt32(reader["CategoryId"]),
                    Level = Convert.ToInt32(reader["PriorityLevel"]),
                    TodoStatus = Convert.ToString(reader["Status"]),
                    TodoDueDate = Convert.ToDateTime(reader["DueDate"]),
                    TodoCreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                    TodoCompletedDate = reader["CompletedDate"] is DBNull ? null : Convert.ToDateTime(reader["CompletedDate"])
                };
                todos.Add(todo);

            }

            connection.Close();

            return Ok(todos);
        }

        [HttpGet("{id}")]
        public IActionResult GetTodoByID(int id)
        {
            if (id == 0 || id < 0)
            {
                return BadRequest("Id can't be 0 or Negative");
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT [TaskID]
                          ,[TaskTitle]
                          ,[TaskDescription]
                          ,[CategoryID]
                          ,[PriorityLevel]
                          ,[Status]
                          ,[DueDate]
                          ,[CreatedDate]
                          ,[CompletedDate]
                      FROM [dbo].[ToDoList] WHERE TaskID = @id";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            connection.Close();

            if (dt.Rows.Count == 0)
            {
                return NotFound("Sorry, Todo is not found.");
            }

            DataRow dr = dt.Rows[0];
            TodoViewModel todo = new TodoViewModel
            {
                Id = Convert.ToInt32(dr["TaskID"]),
                Title = Convert.ToString(dr["TaskTitle"]),
                Description = Convert.ToString(dr["TaskDescription"]),
                IDCategory = Convert.ToInt32(dr["CategoryID"]),
                Level = Convert.ToInt32(dr["PriorityLevel"]),
                TodoStatus = Convert.ToString(dr["Status"]),
                TodoDueDate = Convert.ToDateTime(dr["DueDate"]),
                TodoCreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                TodoCompletedDate = dr["CompletedDate"] is DBNull ? null : Convert.ToDateTime(dr["CompletedDate"])

            };

            return Ok(todo);
        }
    }
}
