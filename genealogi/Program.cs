using System;
using System.Collections.Generic;
using System.Data;

namespace genealogi
{
    class Program
    {
        public static CRUDops db = new CRUDops();

        static void Main(string[] args)
        {
            db.CreateDatabase("MyFamily");
            db.CreateTable();
            db.ExecuteSQL("Delete from family");
            var mother = db.CreatePerson("Shmi", "Skywalker");
            var father = db.CreatePerson("The Force", "");
            var anakin = db.CreatePerson("Anakin", "Skywalker", mother, father, "41 BBY", "4 ABY","", "Tatooine", "DS-2 Death Star II Mobile Battle Station", "Endor system");
            father = anakin;
            mother = db.CreatePerson("Padme", "Amidala");
            var luke = db.CreatePerson("Luke", "Skywalker", mother, father, "19 BBY", "34 ABY", "", "Polis Massa", "", "Ahch-To");
            var leia = db.CreatePerson("Leia", "Skywalker", mother, father, "19 BBY", "35 ABY", "", "Polis Massa", "", "Ajan Kloss"); // adopted to Organa

            father = luke;
            mother = db.CreatePerson("Mara", "Jade");
            var child = db.CreatePerson("Ben", "Skywalker", mother, luke, "26.5 ABY", "", "Coruscant", "Coruscant", "", "");

            //var sibilings = db.GetHalfSiblings(anakin);

            //Console.WriteLine("Siblings:");
            //foreach (DataRow item in sibilings.Rows)
            //{
            //    Console.WriteLine($"{item["firstName"]} {item["lastName"]}");
            //}

            Console.WriteLine("Type in persons first name and last name of the person you want to manage");
            Console.Write("First name: ");
            string firstName = Console.ReadLine();
            Console.Write("Last name: ");
            string lastName = Console.ReadLine();

            Console.WriteLine("What do you want to do?");
            Console.WriteLine("1. Update");
            Console.WriteLine("2. Delete");
            Console.WriteLine("3. Show parents");
            Console.WriteLine("4. Show siblings");
            Console.WriteLine("5. Show names starting with specific letter");
            Console.WriteLine("6. Show childrens");

            do
            {
                var cki = Console.ReadKey();
                switch (cki.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        UpdateMenu(firstName, lastName);
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        DeleteMenu(firstName, lastName);
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        ShowParents(firstName, lastName);
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        ShowSibling(firstName, lastName);
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        ShowNamesStartingWith();
                        break;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        ShowChildrens(firstName, lastName);
                        break;
                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        OrderByPlaceOfBirth();
                        break;
                    default:
                        break;
                }
            } while (true);







            // Godkänt:

            // Väl godkänd

        }

        private static void OrderByPlaceOfBirth()
        {
            var orderedBirthPlace = db.OrderPeopleByPlaceOfBirth();
            if (orderedBirthPlace == null)
            {
                Console.WriteLine("Found nothing");
            }
            else
            {
                foreach (DataRow item in orderedBirthPlace.Rows)
                {
                    Console.WriteLine($"{item["firstName"]} {item["lastName"]} {item["countryOfBirth"]}");
                }
            }
        }

        private static void ShowChildrens(string firstName, string lastName)
        {
            var personId = db.GetPersonId(firstName, lastName);
            var children = db.GetChildren(personId);
            if (children == null)
            {
                Console.WriteLine("No children found");
            }
            else
            {
                foreach (DataRow item in children.Rows)
                {
                    Console.WriteLine($"{item["firstName"]} {item["lastName"]}");
                }
            }

            Console.ReadLine();
        }

        private static void ShowNamesStartingWith()
        {
            List<Person> persons = new List<Person>();
            Console.WriteLine("Search names by first letter of name");
            Console.WriteLine("first or last name?(type firstName or lastName)");
            var chosenName = Console.ReadLine();
            Console.Write("Choose a letter: ");
            string letter = Console.ReadLine();
            if (chosenName == "firstName")
            {
                persons = db.SearchFirstNameByFirstLetter(letter);
            }
            else if (chosenName == "lastName")
            {
                persons = db.SearchLastNameByFirstLetter(letter);
            }
            

            if (persons == null)
            {
                Console.WriteLine("No names found");
            }
            else
            {
                foreach (var item in persons)
                {
                    Console.WriteLine(item.FirstName + " " + item.LastName);
                }
            }


            Console.ReadLine();
        }

        private static void ShowSibling(string firstName, string lastName)
        {
            var parents = db.GetParents(firstName, lastName);
            var siblings = db.GetSiblings(parents[0].FirstName, parents[0].LastName, parents[1].FirstName, parents[1].LastName);

            if (siblings == null)
            {
                Console.WriteLine("No siblings found");
            }
            else
            {
                foreach (DataRow item in siblings.Rows)
                {
                    Console.WriteLine($"{item["firstName"]} {item["lastName"]}");
                }
            }

            Console.ReadLine();
        }

        private static void ShowParents(string firstName, string lastName)
        {
            var parents = db.GetParents(firstName, lastName);
            foreach (var item in parents)
            {
                if (item == null)
                {
                    Console.WriteLine("No parent found");
                }
                else
                {
                    Console.Write(item.FirstName + " " + item.LastName);
                }
            }

            Console.ReadLine();
        }

        private static void DeleteMenu(string firstName, string lastName)
        {
            Console.WriteLine("Are you sure you want to delete? y or n");
            string answer = Console.ReadLine();
            if (answer == "y")
            {
                db.DeletePerson(firstName, lastName);
                Console.WriteLine("Person deleted");
            }
        }

        private static void UpdateMenu(string firstName, string lastName)
        {
            Console.WriteLine("Choose what you want to change: ");
            Console.WriteLine("1. First name");
            Console.WriteLine("2. Last name");
            Console.WriteLine("3. Mother");
            Console.WriteLine("4. Father");
            Console.WriteLine("5. Birthdate");
            Console.WriteLine("6. Date of death");
            var thingToChange = "";
            string newValue = "";
            bool open = true;

            do
            {
                var cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.D1:
                        Console.WriteLine("First name");
                        thingToChange = "firstName";
                        Console.Write("Change this to: ");
                        newValue = Console.ReadLine();
                        open = false;
                        break;
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.D2:
                        Console.WriteLine("Last name");
                        thingToChange = "lastName";
                        Console.Write("Change this to: ");
                        newValue = Console.ReadLine();
                        open = false;
                        break;
                    case ConsoleKey.NumPad3:
                    case ConsoleKey.D3:
                        Console.WriteLine("Mother");
                        thingToChange = "mother";
                        Console.Write("Who is the mother(first name and last name): ");
                        newValue = Console.ReadLine();
                        open = false;
                        break;
                    case ConsoleKey.NumPad4:
                    case ConsoleKey.D4:
                        Console.WriteLine("Father");
                        thingToChange = "father";
                        Console.Write("Who is the father(first name and last name): ");
                        newValue = Console.ReadLine();
                        open = false;
                        break;
                    case ConsoleKey.NumPad5:
                    case ConsoleKey.D5:
                        Console.Write("Birth date: ");
                        thingToChange = "dateOfBirth";
                        newValue = Console.ReadLine();
                        open = false;
                        break;
                    case ConsoleKey.NumPad6:
                    case ConsoleKey.D6:
                        Console.Write("Date of death: ");
                        thingToChange = "dateOfDeath";
                        newValue = Console.ReadLine();
                        open = false;
                        break;
                }

            } while (open);

            db.UpdatePerson(firstName, lastName, thingToChange, newValue);

            Console.WriteLine("person updated");
            Console.ReadLine();
        }
    }
}
