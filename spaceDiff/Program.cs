class program
{
    static int Main(string[] args)
    {
        spaceDiff.spaceDiff spaceDiff = new spaceDiff.spaceDiff();

        Console.WriteLine("spaceDiff v0.5");
        Console.WriteLine("I don't care if it wasn't Bob, this was all Gary's fault. He couldn't syncronize the");
        Console.WriteLine("letters in syncronize, nevermind swim like it. Written by: Christopher Laverdure");
        Console.WriteLine("=----------------------------------------------------------------------------------=");

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: spaceDiff.exe <file1> <file2>");
            return 1;
        }

        spaceDiff.loadFilesforComparsion(args[0], args[1]);

        spaceDiff.beginAnalysis();

        spaceDiff.displaydeletedData();

        return 0;
    }
}
