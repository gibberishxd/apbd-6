using apbd_5.Models;
using apbd_5.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace apbd_5.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetAnimals(string orderBy = "name")
    {
        // Ensure orderBy parameter is valid
        if (!IsValidOrderBy(orderBy))
        {
            return BadRequest("Invalid orderBy parameter. Accepted values: name, description, category, area.");
        }

        // Construct SQL query with optional sorting
        string orderByClause = $"ORDER BY {orderBy}";
        string sqlQuery = $"SELECT * FROM Animal {orderByClause};";

        // Execute SQL query
        List<Animal> animals = ExecuteAnimalQuery(sqlQuery);

        return Ok(animals);
    }
    
    [HttpPost]
    public IActionResult AddAnimal(AddAnimal animal)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "INSERT INTO Animal VALUES (@animalName,'','','')";
        command.Parameters.AddWithValue("@animalName", animal.Name);
        
        command.ExecuteNonQuery();

        return Created("", null);
    }
    
    private List<Animal> ExecuteAnimalQuery(string sqlQuery)
    {
        List<Animal> animals = new List<Animal>();

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(sqlQuery, connection);
            
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Animal animal = new Animal
            {
                IdAnimal = reader.GetInt32(reader.GetOrdinal("IdAnimal")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                Area = reader.GetString(reader.GetOrdinal("Area"))
            };
            animals.Add(animal);
        }

        return animals;
    }
    
    private bool IsValidOrderBy(string orderBy)
    {
        string[] validOrders = { "name", "description", "category", "area" };
        return Array.IndexOf(validOrders, orderBy.ToLower()) != -1;
    } 
    
    [HttpPut("{idAnimal}")]
    public IActionResult UpdateAnimal(int idAnimal, UpdateAnimal updatedAnimal)
    {
        // Validate the ModelState
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if the animal exists
        if (!AnimalExists(idAnimal))
        {
            return NotFound("Animal not found.");
        }

        // Construct the SQL query to update the animal
        string sqlQuery = @"
        UPDATE Animal
        SET Name = @Name,
            Description = @Description,
            Category = @Category,
            Area = @Area
        WHERE IdAnimal = @IdAnimal;";

        // Execute the SQL query
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(sqlQuery, connection);
    
        command.Parameters.AddWithValue("@Name", updatedAnimal.Name);
        command.Parameters.AddWithValue("@Description", updatedAnimal.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Category", updatedAnimal.Category);
        command.Parameters.AddWithValue("@Area", updatedAnimal.Area);
        command.Parameters.AddWithValue("@IdAnimal", idAnimal);

        connection.Open();
        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            return NoContent(); // Updated successfully
        }
        else
        {
            return StatusCode(500, "Failed to update animal.");
        }
    }

    private bool AnimalExists(int idAnimal)
    {
        string sqlQuery = "SELECT COUNT(*) FROM Animal WHERE IdAnimal = @IdAnimal;";

    
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(sqlQuery, connection);
    
        command.Parameters.AddWithValue("@IdAnimal", idAnimal);

        connection.Open();
        int count = (int)command.ExecuteScalar();

        return count > 0;
    }
    
    [HttpDelete("{idAnimal}")]
    public IActionResult DeleteAnimal(int idAnimal)
    {
        // Check if the animal exists
        if (!AnimalExists(idAnimal))
        {
            return NotFound("Animal not found.");
        }

        // Construct the SQL query to delete the animal
        string sqlQuery = "DELETE FROM Animal WHERE IdAnimal = @IdAnimal;";

        // Execute the SQL query
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(sqlQuery, connection);
    
        command.Parameters.AddWithValue("@IdAnimal", idAnimal);

        connection.Open();
        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            return NoContent(); // Deleted successfully
        }
        else
        {
            return StatusCode(500, "Failed to delete animal.");
        }
    }

    

    
    
}