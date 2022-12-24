using System;

namespace TreeDemoEx
{
    class Program
    {
        static void Main(string[] args)
        {
            var building = new Space { Name = "Science Building", SquareFeet = 30000 };

            var storage = building.Children.Add(new Space { Name = "Storage", SquareFeet = 1200 });
            //var bin = storage.Children.Add(new Space { Name = "Bin", SquareFeet = 4 });

            /*
            foreach (var item in building.ChildNodes)
                Console.WriteLine(item.ToString());
            */
            

        }
    }
}
