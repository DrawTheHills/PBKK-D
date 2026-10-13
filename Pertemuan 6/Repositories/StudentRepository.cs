using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using StudentRegistrationApp.Models;

namespace StudentRegistrationApp.Repositories
{
    public class StudentRepository
    {
        private readonly string _connectionString = 
            @"Server=.\SQLEXPRESS;Database=StudentRegistrationDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public List<Program> GetPrograms()
        {
            var list = new List<Program>();
            const string sql = "SELECT ProgramId, ProgramCode, ProgramName FROM Programs WHERE IsActive = 1;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Program
                {
                    ProgramId = reader.GetInt32(reader.GetOrdinal("ProgramId")),
                    ProgramCode = reader.GetString(reader.GetOrdinal("ProgramCode")),
                    ProgramName = reader.GetString(reader.GetOrdinal("ProgramName"))
                });
            }
            return list;
        }

        public List<Student> GetAll()
        {
            var students = new List<Student>();
            const string sql = @"
                SELECT s.StudentId, s.NIM, s.Name, s.ProgramId, p.ProgramName, s.Gender
                FROM Students s
                INNER JOIN Programs p ON s.ProgramId = p.ProgramId
                ORDER BY s.StudentId DESC;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                students.Add(new Student
                {
                    StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                    NIM = reader.GetString(reader.GetOrdinal("NIM")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    ProgramId = reader.GetInt32(reader.GetOrdinal("ProgramId")),
                    ProgramName = reader.GetString(reader.GetOrdinal("ProgramName")),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? string.Empty : reader.GetString(reader.GetOrdinal("Gender"))
                });
            }
            return students;
        }

        public bool Add(Student student)
        {
            const string sql = @"
                INSERT INTO Students (NIM, Name, ProgramId, Gender)
                VALUES (@NIM, @Name, @ProgramId, @Gender);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@NIM", student.NIM);
            command.Parameters.AddWithValue("@Name", student.Name);
            command.Parameters.AddWithValue("@ProgramId", student.ProgramId);
            command.Parameters.AddWithValue("@Gender", string.IsNullOrEmpty(student.Gender) ? DBNull.Value : student.Gender);

            connection.Open();
            return command.ExecuteNonQuery() > 0;
        }

        public bool Delete(int studentId)
        {
            const string sql = "DELETE FROM Students WHERE StudentId = @StudentId;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@StudentId", studentId);

            connection.Open();
            return command.ExecuteNonQuery() > 0;
        }
    }
}
