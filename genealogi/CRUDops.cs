using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace genealogi
{
    class CRUDops
    {

        /// <summary>
        /// Defines the connString.
        /// </summary>
        private string connString = @"Data Source=localhost;Integrated Security=true;database={0}";

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        public string database { get; set; } = "MyFamily";

        /// <summary>
        /// The Create method.
        /// </summary>
        /// <param name="firstName">The firstName<see cref="string"/>.</param>
        /// <param name="lastName">The lastName<see cref="string"/>.</param>
        public void Create(string firstName, string lastName)
        {
            /*
             * Does pretty much the same as  
             * Insert Into family (firstName,lastName) VALUES('Jason','Vorhees');
             */
            var sql = "Insert Into family (firstName,lastName) VALUES(@firstname,@lastname);";
            SqlConnection conn;
            SqlCommand cmd;
            conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@firstname", firstName);
            cmd.Parameters.AddWithValue("@lastname", lastName);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        /// <summary>
        /// The Create method.
        /// </summary>
        /// <param name="firstName">The firstName<see cref="string"/>.</param>
        /// <param name="lastName">The lastName<see cref="string"/>.</param>
        /// <param name="mother">The mother<see cref="int"/>.</param>
        /// <param name="father">The father<see cref="int"/>.</param>
        public void Create(string firstName, string lastName, int mother, int father, string birth, string death, string cityBorn, string countryBorn, string cityDead, string countryDead)
        {
            var sql = "Insert Into family (firstName,lastName,mother,father,dateOfBirth,dateOfDeath,cityOfBirth,countryOfBirth,cityOfDeath,countryOfDeath) VALUES(@firstname,@lastname,@mother,@father,@dateofbirth,@dateofdeath,@cityOfBirth,@countryOfBirth,@cityOfDeath,@countryOfDeath);";
            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@firstname", firstName);
            cmd.Parameters.AddWithValue("@lastname", lastName);
            cmd.Parameters.AddWithValue("@mother", mother);
            cmd.Parameters.AddWithValue("@father", father);
            cmd.Parameters.AddWithValue("@dateofbirth", birth);
            cmd.Parameters.AddWithValue("@dateofdeath", death);
            cmd.Parameters.AddWithValue("@cityOfBirth", cityBorn);
            cmd.Parameters.AddWithValue("@countryOfBirth", countryBorn);
            cmd.Parameters.AddWithValue("@cityOfDeath", cityDead);
            cmd.Parameters.AddWithValue("@countryOfDeath", countryDead);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        /// <summary>
        /// The Update method
        /// </summary>
        /// <param name="firstName">the first name of the object to update <see cref="string"/>.</param>
        /// <param name="lastName">the last name of the object to update <see cref="string"/>.</param>
        /// <param name="thingToChange">the column to update <see cref="string"/>.</param>
        /// <param name="newValue">the new value <see cref="string"/>.</param>
        /// <returns></returns>
        public void UpdatePerson(string firstName, string lastName, string thingToChange, string newValue)
        {
            var personId = GetPersonId(firstName, lastName);
            var person = GetPerson(personId);

            var parentFirstName = "";
            var parentLastName = "";
            var parentId = 0;

            List<string> parentNameList = new List<string>();
            if (thingToChange == "mother" || thingToChange == "father")
            {
                parentNameList = newValue.Split(' ').ToList();
                parentFirstName = parentNameList[0];
                parentLastName = parentNameList[1];
                parentId = GetPersonId(parentFirstName, parentLastName);
                newValue = parentId.ToString();
            }

            var sql = $"UPDATE family SET {thingToChange} = @newValue WHERE firstName = @firstName AND lastName = @lastName;";

            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@newValue", newValue);
            cmd.Parameters.AddWithValue("@firstName", firstName);
            cmd.Parameters.AddWithValue("@lastName", lastName);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        /// <summary>
        /// Delete person method
        /// </summary>
        /// <param name="firstName">person first name <see cref="string"/>.</param>
        /// <param name="lastName">person last name <see cref="string"/>.</param>
        public void DeletePerson(string firstName, string lastName)
        {
            var personId = GetPersonId(firstName, lastName);
            var sql = "DELETE FROM family WHERE id = @id";
            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", personId);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        /// <summary>
        /// Get the parents of a person
        /// </summary>
        /// <param name="firstName">person first name <see cref="string"/>.</param>
        /// <param name="lastName">person last name <see cref="string"/>.</param>
        /// <returns></returns>
        public List<Person> GetParents(string firstName, string lastName)
        {
            List<Person> parents = new List<Person>();
            Person person = new Person();
            var personId = GetPersonId(firstName, lastName);
            person = GetPerson(personId);
            var father = GetPerson(person.Father);
            var mother = GetPerson(person.Mother);
            parents.Add(father);
            parents.Add(mother);

            return parents;
        }

        /// <summary>
        /// Search all person with last name starting with specific letter
        /// </summary>
        /// <param name="letter">starting letter <see cref="string"/>.</param>
        /// <returns>The <see cref="List{T}"/>.</returns>
        public List<Person> SearchLastNameByFirstLetter(string letter)
        {
            var sql = "SELECT firstName, lastName FROM family WHERE lastName LIKE @letter";

            List<Person> persons = new List<Person>();
            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@letter", letter + "%");
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Person person = new Person();
                        person.FirstName = SafeGetString(reader, 0);
                        person.LastName = SafeGetString(reader, 1);
                        persons.Add(person);
                    }
                }
            }
            conn.Close();

            return persons;
        }


        /// <summary>
        /// Search all person with first name starting with specific letter
        /// </summary>
        /// <param name="letter">starting letter <see cref="string"/>.</param>
        /// <returns>The <see cref="List{T}<"/>.</returns>
        public List<Person> SearchFirstNameByFirstLetter(string letter)
        {
            var sql = "SELECT firstName, lastName FROM family WHERE firstName LIKE @letter";

            List<Person> persons = new List<Person>();
            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@letter", letter + "%");
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Person person = new Person();
                        person.FirstName = SafeGetString(reader, 0);
                        person.LastName = SafeGetString(reader, 1);
                        persons.Add(person);
                    }
                }
            }
            conn.Close();

            return persons;
        }


        /// <summary>
        /// The CreateDatabase method.
        /// </summary>
        public void CreateDatabase(string databaseName)
        {
            /*
             *  quick and dirty
             *  tries to create database, if it fails it assumes the database
             *  already exists
             */
            string sql = $"CREATE DATABASE {databaseName}";
            database = "Master";
            try
            {
                ExecuteSQL(sql);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            database = databaseName;
        }

        /// <summary>
        /// Creates a person in the database
        /// </summary>
        /// <param name="firstName">The firstName<see cref="string"/>.</param>
        /// <param name="lastName">The lastName<see cref="string"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CreatePerson(string firstName, string lastName)
        {
            var personId = GetPersonId(firstName, lastName);
            if (personId == 0)
            {
                Create(firstName, lastName);
                personId = GetPersonId(firstName, lastName);
            }
            return personId;
        }

        /// <summary>
        /// Creates a person in the database
        /// </summary>
        /// <param name="firstName">The firstName<see cref="string"/>.</param>
        /// <param name="lastName">The lastName<see cref="string"/>.</param>
        /// <param name="mother">The mother<see cref="int"/>.</param>
        /// <param name="father">The father<see cref="int"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CreatePerson(string firstName, string lastName, int mother, int father, string birth, string death, string cityBorn, string countryBorn, string cityDead, string countryDead)
        {
            var personId = GetPersonId(firstName, lastName);
            if (personId == 0)
            {
                Create(firstName, lastName, mother, father, birth, death, cityBorn, countryBorn, cityDead, countryDead);
                personId = GetPersonId(firstName, lastName);
            }
            return personId;
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        public void CreateTable()
        {
            /*
             *  quick and dirty
             *  tries to create a table, if it fails it assumes the table
             *  already exists
             */
            string sql = @"
                        CREATE TABLE [dbo].[family](
                        [id] [int] IDENTITY(1,1) NOT NULL,
                        [firstName] [nvarchar](50) NULL,
                        [lastName] [nvarchar](50) NULL,
                        [mother] [int] NULL,
                        [father] [int] NULL,
                        [dateOfBirth] [nvarchar](50) NULL,
                        [dateOfDeath] [nvarchar](50) NULL,
                        [cityOfBirth] [nvarchar](50) NULL,
                        [countryOfBirth] [nvarchar](50) NULL,
                        [cityOfDeath] [nvarchar](50) NULL,
                        [countryOfDeath] [nvarchar](50) NULL,
                        ) ON [PRIMARY]
                        ";
            try
            {
                ExecuteSQL(sql);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// The common ExecuteSQL method.
        /// </summary>
        /// <param name="sql">The sql<see cref="string"/>.</param>
        public void ExecuteSQL(string sql)
        {
            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        /// <summary>
        /// The GetHalfSiblings.
        /// </summary>
        /// <param name="parent">The parent Id<see cref="int"/>.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public DataTable GetChildren(int parent)
        {
            /*
             *  looks for mother or father with the parent Id
             */

            var sql = @"
                SELECT * FROM family Where
                mother= @parent OR
                father = @parent
                ";

            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@parent", parent);
            var dt = new DataTable();
            var adapt = new SqlDataAdapter(cmd);
            adapt.Fill(dt);
            conn.Close();
            return dt;
        }

        /// <summary>
        /// The GetPersonId, gets the Id to a person.
        /// </summary>
        /// <param name="firstName">The firstName<see cref="string"/>.</param>
        /// <param name="lastName">The lastName<see cref="string"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetPersonId(string firstName, string lastName)
        {
            var sql = "SELECT TOP 1 Id from family Where firstName=@firstname and lastName=@lastname;";
            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@firstname", firstName);
            cmd.Parameters.AddWithValue("@lastname", lastName);
            var dt = new DataTable();
            var adapt = new SqlDataAdapter(cmd);
            adapt.Fill(dt);
            conn.Close();

            if (dt.Rows.Count == 0) return 0;

            var row = dt.Rows[0];
            var id = (int)row["Id"];
            return id;
        }

        /// <summary>
        /// The GetSiblings.
        /// </summary>
        /// <param name="motherId">The motherId<see cref="int"/>.</param>
        /// <param name="fatherId">The fatherId<see cref="int"/>.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public DataTable GetCommonChildrens(int motherId, int fatherId)
        {
            var sql = @"
                SELECT * FROM family Where
                mother= @mother AND
                father = @father;
                ";

            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mother", motherId);
            cmd.Parameters.AddWithValue("@father", fatherId);
            var dt = new DataTable();
            var adapt = new SqlDataAdapter(cmd);
            adapt.Fill(dt);
            conn.Close();
            return dt;
        }

        /// <summary>
        /// The GetSiblings.
        /// </summary>
        /// <param name="motherFirstName">The motherFirstName<see cref="string"/>.</param>
        /// <param name="motherLastName">The motherLastName<see cref="string"/>.</param>
        /// <param name="fatherFirstName">The fatherFirstName<see cref="string"/>.</param>
        /// <param name="fatherLastName">The fatherLastName<see cref="string"/>.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public DataTable GetSiblings(string fatherFirstName, string fatherLastName, string motherFirstName, string motherLastName)
        {
            var sql = @"
                SELECT * FROM family Where
                mother= (SELECT Id From family where firstName=@motherfirst and lastName=@motherlast) AND
                father = (SELECT Id From family where firstName=@fatherfirst and lastName=@fatherlast);
                ";

            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@motherfirst", motherFirstName);
            cmd.Parameters.AddWithValue("@motherlast", motherLastName);
            cmd.Parameters.AddWithValue("@fatherfirst", fatherFirstName);
            cmd.Parameters.AddWithValue("@fatherlast", fatherLastName);
            var dt = new DataTable();
            var adapt = new SqlDataAdapter(cmd);
            adapt.Fill(dt);
            conn.Close();
            return dt;
        }

        /// <summary>
        /// Gets the person object by id
        /// </summary>
        /// <param name="id">person id</param>
        /// <returns>The <see cref="Person"/>.</returns>
        public Person GetPerson(int id)
        {
            var sql = @"SELECT * FROM family WHERE Id = @id";

            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            Person person = new Person();

            using (SqlCommand command = new SqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@id", id);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        person.Id = reader.GetInt32(0);
                        person.FirstName = SafeGetString(reader, 1);
                        person.LastName = SafeGetString(reader, 2);
                        person.Mother = SafeGetInt(reader, 3);
                        person.Father = SafeGetInt(reader, 4);
                        person.BirthDate = SafeGetString(reader, 5);
                        person.DateOfDeath = SafeGetString(reader, 6);
                    }
                }
            }
            conn.Close();

            return person;
        }

        /// <summary>
        /// Display persons ordered by they place of birth, if no place is found they are excluded from the list
        /// </summary>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public DataTable OrderPeopleByPlaceOfBirth()
        {
            var sql = @"SELECT * FROM family WHERE countryOfBirth IS NOT NULL ORDER BY countryOfBirth";
            var conn = new SqlConnection(string.Format(connString, database));
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            var dt = new DataTable();
            var adapt = new SqlDataAdapter(cmd);
            adapt.Fill(dt);
            conn.Close();
            return dt;
        }

        /// <summary>
        /// Prevent null return from db
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="colIndex"></param>
        /// <returns></returns>
        public static int SafeGetInt(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt32(colIndex);
            return 0;
        }

        /// <summary>
        /// Prevent null return from db
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="colIndex"></param>
        /// <returns></returns>
        public static string SafeGetString(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }
    }
}
