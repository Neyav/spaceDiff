using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data.Common;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace spaceDiff
{
    internal class range
    {
        public int size { get; private set; }
        private int _start;
        private int _end;

        public int start
        { 
            get { return _start; }
            set { _start = value; size = _end - _start; }
        }
        public int end
        { 
            get { return _end; }
            set { _end = value; size = _end - _start; }
        }
    }

    // dualRange contains two ranges, one for the old file and one for the new file
    internal class dualRange
    {
        private range _oldRange;
        private range _newRange;
        public range oldRange
        {
            get { return _oldRange; }
            set { _oldRange = value; 
                if (size > 0 && _oldRange.size != size)
                    throw new Exception("Range size mismatch");
                else
                    size = _oldRange.size; }
        }
        public range newRange
        {
            get { return _newRange; }
            set { _newRange = value; 
                if (size > 0 && _newRange.size != size)
                    throw new Exception("Range size mismatch");
                else
                    size = _newRange.size; }
        }

        public int size { get; private set; }

        public dualRange()
        {
            _oldRange = new range();
            _newRange = new range();
        }
    }

    internal class spaceDiff
    {
        private List<int> [] oldbyteReferences;
        private List<int> [] newbyteReferences;

        private List<dualRange> masterRangeList;

        private byte[] oldFile;
        private byte[] newFile;
        private bool filesLoaded;

        private int max(int a, int b)
        {
            if (a > b)
                return a;
            else
                return b;
        }

        private int min(int a, int b)
        {
            if (a < b)
                return a;
            else
                return b;
        }

        private bool rangeOverlap(range a, range b)
        {
            if (this.max(a.end, b.end) - this.min(a.start, b.start) < (a.end - a.start) + (b.end - b.start))
                return true;

            return false;
        }
        private void pruneRange(ref List<dualRange> dualList)
        {
            int iSize = dualList.Count();

            for (int i = 0; i < iSize; i++)
            {
                for (int y = 0; y < iSize; y++)
                {
                    if (i == y)
                        continue;

                    if (i == iSize)
                        break;

                    // Out of sequence. remove the smaller of the two.
                    if ((dualList[i].oldRange.start > dualList[y].oldRange.start &&
                        dualList[i].newRange.start < dualList[y].newRange.start) ||
                        (dualList[i].oldRange.start < dualList[y].oldRange.start &&
                        dualList[i].newRange.start > dualList[y].newRange.start))
                    {
                        if (dualList[i].size < dualList[y].size)
                        {
                            dualList.RemoveAt(i);
                            if (i > 0)
                                i--;
                            iSize--;
                            continue;
                        }
                        else
                        {
                            dualList.RemoveAt(y);
                            if (y > 0)
                                y--;
                            iSize--;
                            continue;
                        }

                    }
                    // There is overlap on the ranges, which means we're duplicating efforts. Remove the smaller of the two.
                    if (rangeOverlap(dualList[i].oldRange, dualList[y].oldRange) ||
                                               rangeOverlap(dualList[i].newRange, dualList[y].newRange))
                    {
                        if (dualList[i].size < dualList[y].size)
                        {
                            dualList.RemoveAt(i);
                           if (i > 0)     
                                i--;
                           iSize--;
                        }
                        else
                        {
                            dualList.RemoveAt(y);
                           if (y > 0)
                                y--;
                            iSize--;
                        }
                    }

                }
            }
        }
        private dualRange calculateSizeOfRange(int aByte, int aOldReference, int aNewReference)
        {
            dualRange dualRange = new dualRange();
            range oldRange = new range();
            range newRange = new range();
            int oldStart = oldbyteReferences[aByte][aOldReference];
            int oldEnd = oldStart;
            int newStart = newbyteReferences[aByte][aNewReference];
            int newEnd = newStart;

            // Find the end of the range
            while ((oldStart > 0 && newStart > 0) && oldFile[oldStart - 1] == newFile[newStart - 1])
            {
                oldStart--;
                newStart--;
            }

            while ((oldEnd < (oldFile.Length - 1) && (newEnd < newFile.Length - 1)) && oldFile[oldEnd + 1] == newFile[newEnd + 1])
            {
                oldEnd++;
                newEnd++;
            }

            oldRange.start = oldStart;
            oldRange.end = oldEnd;
            newRange.start = newStart;
            newRange.end = newEnd;

            dualRange.oldRange = oldRange;
            dualRange.newRange = newRange;

            return dualRange;
        }

        private void findRangesWithin(int aByte, int aOldReference)
        {
            List<dualRange> rangeList = new List<dualRange>();

            for (int i = 0; i < newbyteReferences[aByte].Count();i++)
            {
                dualRange dualRange = this.calculateSizeOfRange(aByte, aOldReference, i);
                
                // We only care about ranges that are 5 bytes or more because they are the only ones that will save space.
                if (dualRange.size > 5)
                {
                    rangeList.Add(dualRange);
                }
            }

            pruneRange(ref rangeList);

            // Add this range list to the master list now.
            masterRangeList.AddRange(rangeList);
        }

        private void findRangesfor(int aByte)
        {
            for ( int i = 0; i < oldbyteReferences[aByte].Count(); i++)
            {
                this.findRangesWithin(aByte, i);
            }
        }
        public bool beginAnalysis()
        {
            if (!filesLoaded)
            {
                Console.WriteLine("There are no files loaded for analysis!");

                return false;
            }
            
            for (int i = 0; i < oldFile.Length; i++)
            {
                oldbyteReferences[oldFile[i] + 128].Add(i);
            }
            
            for (int i = 0; i < newFile.Length; i++)
            {
                newbyteReferences[newFile[i] + 128].Add(i);
            }

            for (int i = 0; i < 256; i++)
            {
                this.findRangesfor(i);
            }

            Console.WriteLine("Found {0} ranges", masterRangeList.Count());

            // Final prune of the master list, just to make sure.
            pruneRange(ref masterRangeList);

            // Sort it.
            masterRangeList.Sort((x, y) => x.oldRange.start.CompareTo(y.oldRange.start));

            // Print it.
            //foreach (dualRange dualRange in masterRangeList)
            //{
            //   Console.WriteLine("Old Range: {0} - {1} New Range: {2} - {3} Size: {4}", dualRange.oldRange.start, dualRange.oldRange.end, dualRange.newRange.start, dualRange.newRange.end, dualRange.size);
            //}

            return true;
        }
        public bool loadFilesforComparsion(string file1, string file2)
        {
            if (!File.Exists(file1))
            {
                Console.WriteLine("File {0} does not exist", file1);
                return false;
            }

            if (!File.Exists(file2)) 
            { 
                Console.WriteLine("File {0} does not exist", file2);
                return false; 
            }

            oldFile = File.ReadAllBytes(file1);
            newFile = File.ReadAllBytes(file2);

            Console.WriteLine("Loaded {0} bytes from {1} to be compared to {2} bytes from {3}.", oldFile.Length, file1, newFile.Length, file2);

            filesLoaded = true;

            return true;
        }

        // Sorry Kayla for party rocking. bwaaw, ba ba ba ba bwaaw.
        public void displaydeletedData ()
        {
            int currentPos = 0;
            int firstDisplayRange = 0;
            int maxCol = Console.WindowWidth;
            int maxRow = Console.WindowHeight;
            int currentCol = 0;
            int currentRow = 0;
            bool displayOld = true;
            bool quit = false;
            // Display all the data from oldFile, colouring the background as red for data that isn't part of a range, and black for data that is.
            while (!quit)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                currentCol = currentRow = 0;
                if (displayOld)
                {
                    for (int i = currentPos; i < oldFile.Length; i++)
                    {
                        bool found = false;
                        foreach (dualRange dualRange in masterRangeList)
                        {
                            if (i >= dualRange.oldRange.start && i <= dualRange.oldRange.end)
                            {
                                if (firstDisplayRange == 0)
                                    firstDisplayRange = dualRange.newRange.start;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                            Console.BackgroundColor = ConsoleColor.Black;
                        else
                            Console.BackgroundColor = ConsoleColor.Red;

                        // Write the ASCII character from oldFile
                        currentCol++;
                        if (currentCol > maxCol)
                        {
                            currentCol = 0;
                            currentRow++;
                            Console.WriteLine();
                        }
                        else if (oldFile[i] == '\n')
                        {
                            currentCol = 0;
                            currentRow++;
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.Write((char)oldFile[i]);
                        }

                        if (currentRow > maxRow)
                            break;
                    }
                }
                else
                {
                    for (int i = currentPos; i < newFile.Length; i++)
                    {
                        bool found = false;
                        foreach (dualRange dualRange in masterRangeList)
                        {
                            if (i >= dualRange.newRange.start && i <= dualRange.newRange.end)
                            {
                                if (firstDisplayRange == 0)
                                    firstDisplayRange = dualRange.oldRange.start;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                            Console.BackgroundColor = ConsoleColor.Black;
                        else
                            Console.BackgroundColor = ConsoleColor.Green;

                        // Write the ASCII character from oldFile
                        currentCol++;
                        if (currentCol >= maxCol)
                        {
                            currentCol = 0;
                            currentRow++;
                            Console.WriteLine();
                        }
                        else if (newFile[i] == '\n')
                        {
                            currentCol = 0;
                            currentRow++;
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.Write((char)newFile[i]);
                        }

                        if (currentRow >= maxRow)
                            break;
                    }
                }

                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            while (displayOld)
                            {
                                if (currentPos > 0)
                                    currentPos--;

                                while (oldFile[currentPos] != '\n' && currentPos > 0)
                                    currentPos--;

                                break;
                            }
                            while (!displayOld)
                            {
                                if (currentPos > 0)
                                    currentPos--;

                                while (newFile[currentPos] != '\n' && currentPos > 0)
                                    currentPos--;

                                break;
                            }
                            break;
                        case ConsoleKey.DownArrow:
                            while (displayOld)
                            {
                                while (oldFile[currentPos] != '\n' && currentPos < oldFile.Length - 1)
                                    currentPos++;

                                currentPos++;

                                break;
                            }
                            while (!displayOld)
                            {
                                while (newFile[currentPos] != '\n' && currentPos < newFile.Length - 1)
                                    currentPos++;

                                currentPos++;

                                break;
                            }

                            break;
                        case ConsoleKey.D:
                            displayOld = true;
                            currentPos = firstDisplayRange;
                            break;
                        case ConsoleKey.I:
                            displayOld = false;
                            currentPos = firstDisplayRange;
                            break;
                        case ConsoleKey.Q:
                            quit = true;
                            break;
                        default:
                            break;

                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                    firstDisplayRange = 0;

                }
            }
        }

        public spaceDiff()
        {
            // Initalize the arrays
            oldbyteReferences = new List<int>[256];
            newbyteReferences = new List<int>[256];

            masterRangeList = new List<dualRange>();

            for (int i = 0; i < 256; i++)
            {
                oldbyteReferences[i] = new List<int>();
                newbyteReferences[i] = new List<int>();
            }

            filesLoaded = false;
        }
    }
}
