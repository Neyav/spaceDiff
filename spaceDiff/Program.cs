class program
{
    static int Main(string[] args)
    {
        spaceDiff.spaceDiff spaceDiff = new spaceDiff.spaceDiff();

        spaceDiff.loadFilesforComparsion("fair.c", "fair1.c");

        spaceDiff.beginAnalysis();

        Console.WriteLine("Hello World!");

        return 0;
    }
}
