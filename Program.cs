using System;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using PedroDataAccess.Models;

namespace PedroDataAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            const string connectionString = "Server=localhost,1433;Database=balta;User ID=sa;Password=Kadoxi@234556";

            using (var connection = new SqlConnection(connectionString))
            {
                //CreateCategory(connection);
                //UpdateCategory(connection);
                //CreateManyCategory(connection);
                // ExecuteProcedure(connection);
                // ExecuteReadProcedure(connection);
                // ListCategories(connection);
                //ExecuteScalar(connection);
                //ReadView(connection);
                //OneToOne(connection);
                OneToMany(connection);
            }
        }
        static void ListCategories(SqlConnection connection)
        {
            var categories = connection.Query<Category>("SELECT [Id], [Title] FROM [Category]");

            foreach (var item in categories)
            {
                Console.WriteLine($"{item.Id} - {item.Title}");
            }
        }
        static void CreateCategory(SqlConnection connection)
        {
            var category = new Category();

            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "http://www.amazonaws.com";
            category.Description = "Categoria destinada a serviços AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSql = @"INSERT INTO 
                    [Category] 
                VALUES(
                    @id, 
                    @title, 
                    @url, 
                    @summary, 
                    @order, 
                    @description, 
                    @featured)"
                ;

            var rows = connection.Execute(insertSql, new
            {
                category.Id,
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured
            });

            Console.WriteLine($"Linhas inseridas : {rows}");
        }
        static void UpdateCategory(SqlConnection connection)
        {
            var updateQuery = @"UPDATE [Category] SET [Title]=@title WHERE [Id]=@id";

            var rows = connection.Execute(updateQuery, new
            {
                id = new Guid("af3407aa-11ae-4621-a2ef-2028b85507c4"),
                title = "Frontend 2023"
            });

            Console.WriteLine($"{rows} Registros Atualizados.");
        }
        static void CreateManyCategory(SqlConnection connection)
        {
            var category = new Category();

            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "http://www.amazonaws.com";
            category.Description = "Categoria destinada a serviços AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var category2 = new Category();

            category2.Id = Guid.NewGuid();
            category2.Title = "Categoria Nova";
            category2.Url = "nova categoria";
            category2.Description = "Categoria nova";
            category2.Order = 9;
            category2.Summary = "new";
            category2.Featured = false;

            var insertSql = @"INSERT INTO 
                    [Category] 
                VALUES(
                    @id, 
                    @title, 
                    @url, 
                    @summary, 
                    @order, 
                    @description, 
                    @featured)"
                ;

            var rows = connection.Execute(insertSql, new[]
            {
             new
                {
                    category.Id,
                    category.Title,
                    category.Url,
                    category.Summary,
                    category.Order,
                    category.Description,
                    category.Featured
                },
             new
                {
                    category2.Id,
                    category2.Title,
                    category2.Url,
                    category2.Summary,
                    category2.Order,
                    category2.Description,
                    category2.Featured
                }});


            Console.WriteLine($"Linhas inseridas : {rows}");
        }
        static void ExecuteProcedure(SqlConnection connection)
        {
            var procedure = "[spDeleteStudent]";
            var parameter = new { StudentId = "52D4B6B8-D2F9-4ADA-8C02-1F878259A382" };
            var affectedRows = connection.Execute(
                procedure,
                parameter,
                commandType: CommandType.StoredProcedure);

            Console.WriteLine($"{affectedRows} Linhas Afetadas");
        }
        static void ExecuteReadProcedure(SqlConnection connection)
        {
            var procedure = "[spGetCoursesByCategory]";
            var parameter = new { CategoryId = "09CE0B7B-CFCA-497B-92C0-3290AD9D5142" };
            var courses = connection.Query(
                procedure,
                parameter,
                commandType: CommandType.StoredProcedure);

            foreach (var course in courses)
            {
                Console.WriteLine(course.Title);
            }
        }
        static void ExecuteScalar(SqlConnection connection)
        {
            var category = new Category();
            category.Title = "Amazon AWS";
            category.Url = "http://www.amazonaws.com";
            category.Description = "Categoria destinada a serviços AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSql = @"
            INSERT INTO 
                    [Category]
                    OUTPUT inserted.[Id]
            VALUES(
                    NEWID(), 
                    @title, 
                    @url, 
                    @summary, 
                    @order, 
                    @description, 
                    @featured)";

            var id = connection.ExecuteScalar<Guid>(insertSql, new
            {
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured
            });

            Console.WriteLine($"A categoria inserida foi : {id}");
        }
        static void ReadView(SqlConnection connection)
        {
            var sql = "SELECT * FROM [vwCourses]";

            var courses = connection.Query(sql);

            foreach (var item in courses)
            {
                Console.WriteLine($"{item.Id} - {item.Title}");
            }
        }
        static void OneToOne(SqlConnection connection)
        {
            var sql = @"
            SELECT * FROM 
            [CareerItem] 
            INNER JOIN [Course] on [CareerItem].[CourseId] = [Course].[Id]";

            var items = connection.Query<CareerItem, Course, CareerItem>(
                sql,
                (careerItem, course) =>
                {
                    careerItem.Course = course;
                    return careerItem;
                }, splitOn: "Id");

            foreach (var item in items)
            {
                Console.WriteLine($"{item.Title} - Curso: {item.Course?.Title}");
            }
        }

        static void OneToMany(SqlConnection connection)
        {
            var sql = @"
            SELECT 
                [Career].[Id],
                [Career].[Title],
                [CareerItem].[CareerId],
                [CareerItem].[Title]
            FROM 
                [Career]
            INNER JOIN
                [CareerItem] ON [CareerItem].[CareerId] = [Career].[Id]
            ORDER BY
                [Career].[Title]";

            var careers = new List<Career>();

            var items = connection.Query<Career, CareerItem, Career>(
           sql, (career, item) =>
           {
               var car = careers.Where(x => x.Id == career.Id).FirstOrDefault();
               if (car == null)
               {
                   car = career;
                   car.Items.Add(item);
                   careers.Add(car);
               }
               else
               {
                   car.Items.Add(item);
               }
               return career;
           }, splitOn: "CareerId");

            foreach (var career in careers)
            {
                Console.WriteLine($"{career.Title}");
                foreach (var item in career.Items)
                {
                    Console.WriteLine($" - {item.Title}");
                }
            }

        }
    }
}
