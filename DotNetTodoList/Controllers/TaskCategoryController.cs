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
    public class TaskCategoryController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=DESKTOP-KPCHONN\\SQLEXPRESS;Initial Catalog=DotNetToDoList;User ID=sa;Password=sasa@123;TrustServerCertificate=True";
        readonly List<string> status = new List<string> { "Pending", "In Progress", "Completed", "Overdue" };

        [HttpGet]
        public IActionResult GetCategory()
        {
            List<CategoryViewModel> categories = new List<CategoryViewModel>();

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT [CategoryID]
                          ,[CategoryName]
                      FROM [dbo].[TaskCategory]";
            SqlCommand cmd = new SqlCommand(query, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                CategoryViewModel categroy = new CategoryViewModel { Id = (int)dr["CategoryID"], Name = Convert.ToString(dr["CategoryName"]) };
                categories.Add(categroy);
            }

            connection.Close();

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public IActionResult GetCategoryById(int id)
        {
            CategoryViewModel? category;

            if (id <= 0)
            {
                return BadRequest("ID can't be equal or less than 0.");
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT [CategoryID]
                              ,[CategoryName]
                          FROM [dbo].[TaskCategory] WHERE CategoryID = @id";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            connection.Close();

            if (dt.Rows.Count == 0)
            {
                return NotFound("Your Requested Category can't be found!");
            }

            DataRow dr = dt.Rows[0];

            category = new CategoryViewModel { Id = (int)dr["CategoryID"], Name = Convert.ToString(dr["CategoryName"]) };

            return Ok(category);
        }

        [HttpPost]
        public IActionResult CreateCategory(CategoryViewModel category)
        {
            if (string.IsNullOrEmpty(category.Name))
            {
                return BadRequest("Name can't be null or empty.");
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"INSERT INTO [dbo].[TaskCategory]
                               ([CategoryName])
                        OUTPUT INSERTED.CategoryID
                         VALUES
                               (@name)";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@name", category.Name);

            object insertedId = cmd.ExecuteScalar();

            connection.Close();

            if (insertedId is null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error!");
            }

            return Ok($"Your Category is created with ID: {insertedId}");
        }

        [HttpPut("{id}")]
        public IActionResult UpgradeCategory(int id, CategoryViewModel category)
        {
            if(id <= 0)
            {
                return BadRequest("Id can't be less than or equal to 0");
            }
            if (string.IsNullOrEmpty(category.Name))
            {
                return BadRequest("Name can't be null or empty");
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"UPDATE [dbo].[TaskCategory]
                           SET [CategoryName] = @name
                         WHERE CategoryID = @id";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", category.Name);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            if(result == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error!");
            }

            return Ok("The category has been updated!");
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateCategory(int id, CategoryViewModel category)
        {
            if (id <= 0)
            {
                return BadRequest("Id can't be less than or equal to 0");
            }
            if (string.IsNullOrEmpty(category.Name))
            {
                return BadRequest("Name can't be null or empty");
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"UPDATE [dbo].[TaskCategory]
                           SET [CategoryName] = @name
                         WHERE CategoryID = @id";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", category.Name);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            if (result == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error!");
            }

            return Ok("The category has been updated!");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id can't be less than or equal to 0");
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"DELETE FROM [dbo].[TaskCategory]
                                WHERE CategoryID = @id";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            if(result == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error!");
            }

            return Ok("Category Deleted!");
        }
    }
}
