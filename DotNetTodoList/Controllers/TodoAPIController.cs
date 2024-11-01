﻿using Dapper;
using DotNetTodoList.DataModel;
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
        readonly List<string> status = new List<string> { "Pending", "In Progress", "Completed", "Overdue" };

        [HttpGet]
        public IActionResult GetTodos(string sort = null)
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

            switch (sort)
            {
                case "duedate":
                    query += "ORDER BY DueDate DESC";
                    break;
                case "level":
                    query += "ORDER BY PriorityLevel DESC";
                    break;
            }

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

        [HttpPost]
        public IActionResult CreateTodo(TodoViewModel todo)
        {
            DateTime? createdDate;
            DateTime? completedDate;
            string columns = @"[TaskTitle]
                       ,[TaskDescription]
                       ,[CategoryID]
                       ,[PriorityLevel]
                       ,[Status]
                       ,[DueDate]";

            string values = @"@title
                      ,@description
                      ,@categoryId
                      ,@level
                      ,@status
                      ,@dueDate";

            if (String.IsNullOrEmpty(todo.Title))
            {
                return BadRequest("Todo title can't be null");
            }
            if (String.IsNullOrEmpty(todo.Description))
            {
                return BadRequest("Todo description can't be null");
            }
            if (todo.IDCategory == 0 || todo.IDCategory < 0)
            {
                return BadRequest("Category Id can't be null or negative");
            }
            if (todo.Level == 0 || todo.Level < 0 || todo.Level > 5)
            {
                return BadRequest("Priority Level must be between 1 and 5.");
            }
            if (String.IsNullOrEmpty(todo.TodoStatus))
            {
                return BadRequest("Todo Status can't be null");
            }
            else if (!status.Contains(todo.TodoStatus))
            {
                return BadRequest("Invalid Status. Status should be one of 'Pending', 'In Progress', 'Completed', 'Overdue'");
            }
            if (todo.TodoDueDate == null)
            {
                return BadRequest("Due Date can't be null.");
            }
            if (todo.TodoCreatedDate != null)
            {
                createdDate = todo.TodoCreatedDate;
                columns += " ,[CreatedDate]";
                values += " ,@createdDate";
            }
            else
            {
                createdDate = DateTime.Now;
            }
            if (todo.TodoCompletedDate != null)
            {
                completedDate = todo.TodoCompletedDate;
                columns += ", [CompletedDate]";
                values += " ,@completedDate";

                if (completedDate < createdDate)
                {
                    return BadRequest("Completed Date can't be earlier than Created Date");
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @$"INSERT INTO [dbo].[ToDoList]
                           ({columns})
                     OUTPUT INSERTED.TaskID
                     VALUES
                           ({values})";

                SqlCommand cmd = new SqlCommand(query, connection);
                if (createdDate != null)
                {
                    cmd.Parameters.AddWithValue("@createdDate", createdDate);
                }
                if (todo.TodoCompletedDate != null)
                {
                    cmd.Parameters.AddWithValue("@completedDate", todo.TodoCompletedDate);
                }
                cmd.Parameters.AddWithValue("@title", todo.Title);
                cmd.Parameters.AddWithValue("@description", todo.Description);
                cmd.Parameters.AddWithValue("@categoryId", todo.IDCategory);
                cmd.Parameters.AddWithValue("@level", todo.Level);
                cmd.Parameters.AddWithValue("@status", todo.TodoStatus);
                cmd.Parameters.AddWithValue("@dueDate", todo.TodoDueDate);

                var insertedId = cmd.ExecuteScalar();
                connection.Close();

                return Ok($"Your Todo is created with ID: {insertedId}");
            }
        }

        /// <summary>
        /// This PUT method can't update the Complete Date!
        /// </summary>
        /// <param name="id"></param>
        /// <param name="todo"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult UpgradeTodo(int id, TodoViewModel todo)
        {
            if (id <= 0)
            {
                return BadRequest("Id can't be less than 0 or Negative");
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            String query = @"UPDATE [dbo].[ToDoList]
                           SET [TaskTitle] = @TaskTitle
                              ,[TaskDescription] = @TaskDescription
                              ,[CategoryID] = @CategoryId
                              ,[PriorityLevel] = @PriorityLevel
                              ,[Status] = @Status
                              ,[DueDate] = @DueDate
                              ,[CreatedDate] = @CreatedDate
                         WHERE TaskID = @id";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@TaskTitle", todo.Title);
            cmd.Parameters.AddWithValue("@TaskDescription", todo.Description);
            cmd.Parameters.AddWithValue("@CategoryId", todo.IDCategory);
            cmd.Parameters.AddWithValue("@PriorityLevel", todo.Level);
            cmd.Parameters.AddWithValue("@Status", todo.TodoStatus);
            cmd.Parameters.AddWithValue("@DueDate", todo.TodoDueDate);
            cmd.Parameters.AddWithValue("@CreatedDate", todo.TodoCreatedDate);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            if (result == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internel Server Occured!");
            }

            return Ok("Todo Updated!");
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateTodo(int id, TodoViewModel todo)
        {
            if (id <= 0)
            {
                return BadRequest("Id can't be less than 0");
            }

            String variables = "";

            if (!(todo.Title is null))
            {
                variables += "[TaskTitle] = @TaskTitle, ";
            }
            if (!(todo.Description is null))
            {
                variables += "[TaskDescription] = @TaskDescription, ";
            }
            if (!(todo.IDCategory is null))
            {
                if (todo.IDCategory <= 0)
                {
                    return BadRequest("Invalid Category Id!");
                }
                variables += "[CategoryID] = @CategoryId, ";
            }
            if (!(todo.Level is null))
            {
                if (todo.Level < 0 || todo.Level > 5)
                {
                    return BadRequest("Invalid Priority Level!");
                }

                variables += "[PriorityLevel] = @PriorityLevel, ";
            }
            if (!(todo.TodoStatus is null))
            {
                if (!(status.Contains(todo.TodoStatus)))
                {
                    return BadRequest("Invalid Status! Status should be one of 'Pending', 'In Progress', 'Completed', 'Overdue'");
                }
                variables += "[Status] = @Status, ";
            }
            if (!(todo.TodoDueDate is null))
            {
                //if (DateTime.TryParse(todo.TodoDueDate.ToString()))
                //{
                //    return BadRequest("Invalid Due Date!");
                //}
                variables += "[DueDate] = @DueDate, ";
            }
            if (!(todo.TodoCreatedDate is null))
            {
                variables += "[CreatedDate] = @CreatedDate, ";
            }
            if (!(todo.TodoCompletedDate is null))
            {
                variables += "[CompletedDate] = @CompletedDate, ";
                if (todo.TodoStatus is null)
                {
                    variables += "[Status] = @Status, ";
                }
                todo.TodoStatus = "Completed";
            }

            variables = variables.Substring(0, variables.Length - 2);

            String query = @$"UPDATE [dbo].[ToDoList]
                           SET {variables}
                         WHERE TaskID = @id";

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();


            SqlCommand cmd = new SqlCommand(query, connection);
            if (!(todo.Title is null))
            {
                cmd.Parameters.AddWithValue("@TaskTitle", todo.Title);
            }
            if (!(todo.Description is null))
            {
                cmd.Parameters.AddWithValue("@TaskDescription", todo.Description);
            }
            if (!(todo.IDCategory is null))
            {
                cmd.Parameters.AddWithValue("@CategoryId", todo.IDCategory);
            }
            if (!(todo.Level is null))
            {
                cmd.Parameters.AddWithValue("@PriorityLevel", todo.Level);
            }
            if (!(todo.TodoStatus is null))
            {
                cmd.Parameters.AddWithValue("@Status", todo.TodoStatus);

            }
            if (!(todo.TodoDueDate is null))
            {
                cmd.Parameters.AddWithValue("@DueDate", todo.TodoDueDate);
            }
            if (!(todo.TodoCreatedDate is null))
            {
                cmd.Parameters.AddWithValue("@CreatedDate", todo.TodoCreatedDate);
            }
            if (!(todo.TodoCompletedDate is null))
            {
                cmd.Parameters.AddWithValue("@CompletedDate", todo.TodoCompletedDate);
            }
            cmd.Parameters.AddWithValue("@id", id);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            if (result == 0)
            {
                return BadRequest("Internal Server Error!");
            }

            return Ok("Your Todo has been updated!");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTodo(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id can't be less than 0");
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            String query = @"DELETE FROM [dbo].[ToDoList]
                            WHERE TaskID = @id";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            if (result == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error!");
            }

            return Ok("Todo Deleted!");
        }
    }

    [Route("api/complete/[controller]")]
    [ApiController]
    public class TodoAPICompleteController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=DESKTOP-KPCHONN\\SQLEXPRESS;Initial Catalog=DotNetToDoList;User ID=sa;Password=sasa@123;TrustServerCertificate=True;";

        [HttpPost("{id}")]
        public IActionResult MarkTodoComplete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id can't be less than or equal to 0.");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                DateTime completedDate = DateTime.Now;
                string status = "Completed";

                string query = @"UPDATE [dbo].[ToDoList]
                             SET [Status] = @status,
                                 [CompletedDate] = @completedDate
                             WHERE TaskID = @id";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@completedDate", completedDate);
                    cmd.Parameters.AddWithValue("@id", id);

                    int result = cmd.ExecuteNonQuery();

                    if (result == 0)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to mark the task as completed. Task ID may not exist.");
                    }
                }
            }

            return Ok("Todo is marked as Completed!");
        }
    }

}
