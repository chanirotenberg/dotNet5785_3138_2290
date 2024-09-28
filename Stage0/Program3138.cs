partial class Program
{
    private static void Main(string[] args)
    {
        Welcome3138();
        Welcome2290();
        Console.ReadKey();
    }

    static partial void Welcome2290();

    private static void Welcome3138()
    {
        Console.WriteLine("Enter your name:");
        string x = Console.ReadLine();
        Console.WriteLine("{0}, welcome to my first console ", x);
    }
}