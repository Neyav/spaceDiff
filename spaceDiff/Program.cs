class program
{
    static int Main(string[] args)
    {
        spaceDiff.spaceDiff spaceDiff = new spaceDiff.spaceDiff();

        Console.WriteLine("spaceDiff v0.5");
        Console.WriteLine("I don't care if it wasn't Bob, this was all Gary's fault. He couldn't syncronize the");
        Console.WriteLine("letters in syncronize, nevermind swim like it. Written by: Christopher Laverdure");
        Console.WriteLine("=----------------------------------------------------------------------------------=");

        spaceDiff.loadFilesforComparsion("fair.c", "fair1.c");

        spaceDiff.beginAnalysis();

        spaceDiff.displaydeletedData();

        return 0;
    }
}
