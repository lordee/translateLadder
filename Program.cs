using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace translateLadder
{
    class Program
    {
        static List<string> multiOrbRung = new List<string>();
        static List<string> ANBRungs = new List<string>();
        static void Main(string[] args)
        {           
            // read in csv
            List<string> records = new List<string>(); 
            using (StreamReader sr = new StreamReader(File.OpenRead(@"/home/danni/Code/HLadderAll.csv")))  
            {  
                string file = sr.ReadToEnd();  
                records = new List<string>(file.Split('\n'));  
            }  

            int multiOrbCount = 0;
            int multiANBCount = 0;

            // records if every line, split into a list of rungs
            List<string> missingCmd = new List<string>();
            
            List<List<UnprocessedLine>> unprocessedRungs = new List<List<UnprocessedLine>>();
            List<UnprocessedLine> unprocessedRung = new List<UnprocessedLine>();
            List<string> mathCommands = myDict.MathCommands();
            string currRung = "";
            int lineCounter = 0;
            foreach (string r in records)
            {
                lineCounter++;
                //string[] cellsb = r.Replace("\"", "").Split(" ");
                string[] cellsb = r.Replace("\"", "").Split(new char[0]);
                if (cellsb.Length > 1)
                {
                    string one = cellsb[0];
                    // discard lines that mean nothing
                    if (one != "LNO" && !one.Contains("CPU=") && one.Length > 0)
                    {
                        List<string> cellsList = new List<string>();
                        
                        foreach (string c in cellsb)
                        {
                            if (c.Length > 0)
                            {
                                cellsList.Add(c);
                            }
                        }

                        string[] cells = cellsList.ToArray();
                        
                        if (cells.Length > 3)
                        {
                            // clean up starting bracket
                            cells[2] = cells[2].Replace("(", "");
                        }

                        // get rung value (last value)
                        string last = cells[cells.Length - 1];
                        bool newRung = false;
                        if (last.Contains("(") && last.Contains(")"))
                        {
                            if (currRung != last)
                            {
                                newRung = true;
                            }
                            currRung = last;
                        }
                        UnprocessedLine uLine = new UnprocessedLine(cells, currRung);

                        // if it's a new current rung, add to new unprocessed rung
                        // if it's same current rung as last line, add to same rung list
                        if (!newRung || unprocessedRung.Count == 0)
                        {
                            unprocessedRung.Add(uLine);
                        }
                        else
                        {
                            // new rung
                            unprocessedRungs.Add(unprocessedRung);
                            unprocessedRung = new List<UnprocessedLine>();
                            unprocessedRung.Add(uLine);
                        }
                        
                    }
                }
            }
            // add the last unprocessed rung
            unprocessedRungs.Add(unprocessedRung);

            Dictionary<string, string> cmds = myDict.Commands();
            List<ProcessedRung> processedRungs = new List<ProcessedRung>();

            foreach (List<UnprocessedLine> rung in unprocessedRungs)
            {
                // Test if branch
                int orbCheck = rung.Count(r => r.text.Contains("ORB"));
                int anbCheck = rung.Count(r => r.text.Contains("ANB"));
                bool skipBranch = false;
                
                // let's not do multiple branches for now
                if (orbCheck > 1 && anbCheck >= 1)
                {
                    multiOrbRung.Add(rung[0].rungNum);
                    //Console.WriteLine("MULTIPLE BRANCH - SKIPPED");
                    skipBranch = true;
                    multiOrbCount++;
                }

                if (anbCheck > 1)
                {
                    multiOrbRung.Add(rung[0].rungNum);
                    //Console.WriteLine("MULTIPLE BRANCH - SKIPPED");
                    skipBranch = true;
                    multiANBCount++;
                }
                List<ProcessedLine> ProcessedLines = new List<ProcessedLine>();
                if (!skipBranch)
                {
                    
                    foreach (UnprocessedLine line in rung)
                    {
                        
                        string lnNum = line.text[0];
                        int lnNumi = Convert.ToInt16(lnNum.Replace("(", "").Replace(")", ""));
                        string oldCmd = "";
                        string oldCmdMath = "";
                        string cmd = "";
                        string mem = "";
                        bool hasClosingBracket = false;
                        string rngNum = line.rungNum;
                        string twob = line.text[1];
                        bool math = TestMath(line.text);

                        if (math)
                        {

                            MathType mt = new MathType(line.text);
                            string a = "";
                            string b = "";
                            string c = "";

                            switch (mt.newType)
                            {
                                case "ADD":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    // if second cell isn't a command, treat it as memory
                                    if (cmds.ContainsKey(twob))
                                    {
                                        // shouldn't happen?
                                        throw new NotImplementedException();
                                    }
                                    else
                                    {
                                        a = TCCheck(line.text[3]);
                                        b = TCCheck(line.text[5]);
                                        c = TCCheck(line.text[1]);

                                        mem = a + "," + b + "," + c;
                                    }
                                break;
                                case "DIV":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    // if second cell isn't a command, treat it as memory
                                    if (cmds.ContainsKey(twob))
                                    {
                                        // shouldn't happen?
                                        throw new NotImplementedException();
                                    }
                                    else
                                    {
                                        a = TCCheck(line.text[3]);
                                        b = TCCheck(line.text[5]);
                                        c = TCCheck(line.text[1]);

                                        mem = a + "," + b + "," + c;
                                    }
                                    break;
                                case "SUB":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    // if second cell isn't a command, treat it as memory
                                    if (cmds.ContainsKey(twob))
                                    {
                                        // shouldn't happen?
                                        throw new NotImplementedException();
                                    }
                                    else
                                    {
                                        a = TCCheck(line.text[3]);
                                        b = TCCheck(line.text[5]);
                                        c = TCCheck(line.text[1]);

                                        mem = a + "," + b + "," + c;
                                    }
                                    break;
                                case "MUL":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    // if second cell isn't a command, treat it as memory
                                    if (cmds.ContainsKey(twob))
                                    {
                                        // shouldn't happen?
                                        throw new NotImplementedException();
                                    }
                                    else
                                    {
                                        a = TCCheck(line.text[3]);
                                        b = TCCheck(line.text[5]);
                                        c = TCCheck(line.text[1]);

                                        mem = a + "," + b + "," + c;
                                    }
                                    break;
                                case "MOV":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    if (cmds.ContainsKey(twob))
                                    {
                                        a = TCCheck(line.text[4]);
                                        b = TCCheck(line.text[2]);
                                        if (a.Substring(0, 1).ToUpper() != "W"
                                            && b.Substring(0, 1).ToUpper() != "W"
                                            && a.Substring(0, 1).ToUpper() != "D"
                                            && b.Substring(0, 1).ToUpper() != "D")
                                        {
                                            cmd = "MOVE_BOOL";
                                            mem = "MB1,";
                                        }
                                        mem += a + "," + b;
                                    }
                                    else
                                    {
                                        a = TCCheck(line.text[3]);
                                        b = TCCheck(line.text[1]);
                                        if (a.Substring(0, 1).ToUpper() != "W"
                                            && b.Substring(0, 1).ToUpper() != "W"
                                            && a.Substring(0, 1).ToUpper() != "D"
                                            && b.Substring(0, 1).ToUpper() != "D")
                                        {
                                            cmd = "MOVE_BOOL";
                                            mem = "MB1,";
                                        }
                                        mem += a + "," + b;
                                    }
                                break;
                                case "EQU":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    a = TCCheck(line.text[2]);
                                    b = TCCheck(line.text[4]);
                                    mem = a + "," + b;
                                break;
                                case "LES":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    a = TCCheck(line.text[2]);
                                    b = TCCheck(line.text[4]);
                                    mem = a + "," + b;
                                break;
                                case "GRT":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    a = TCCheck(line.text[2]);
                                    b = TCCheck(line.text[4]);
                                    mem = a + "," + b;
                                    break;
                                case "NEQ":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    a = TCCheck(line.text[2]);
                                    b = TCCheck(line.text[4]);
                                    mem = a + "," + b;
                                break;
                                case "LEQ":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    a = TCCheck(line.text[2]);
                                    b = TCCheck(line.text[4]);
                                    mem = a + "," + b;
                                break;
                                case "GEQ":
                                    oldCmdMath = mt.oldType;
                                    oldCmd = line.text[1];
                                    cmd = mt.newType;
                                    a = TCCheck(line.text[2]);
                                    b = TCCheck(line.text[4]);
                                    mem = a + "," + b;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                            // check if there is an OR statement or something along with math
                            if (cmds.ContainsKey(twob))
                            {
                                switch (twob)
                                {
                                    case "ORI":
                                    case "OR":
                                        ProcessedLine p = ProcessedLines[ProcessedLines.Count - 1];
                                        switch (p.oldCmd)
                                        {
                                            case "LD":
                                            case "LDI":
                                                p.cmd = "[" + p.cmd;
                                            break;
                                            case "ORI":
                                            case "OR":
                                                p.hasClosingBracket = false;
                                            break;
                                        }
                                        // comma between cmds, last bracket in processing of rung
                                        cmd = "," + cmd;
                                        hasClosingBracket = true;
                                    break;
                                }
                            }

                        }
                        else
                        {
                            if (ProcessedLines.Count > 0)
                            {
                                if (ProcessedLines[0].rungNum == "(01079)")
                                    {
                                        bool mer = true;
                                    }
                            }
                            
                            Branch branch = null;
                            switch (twob)
                            {
                                case "LD":
                                    oldCmd = twob;
                                    cmd = "XIC";
                                    mem = line.text[2];
                                break;
                                case "LDI":
                                    oldCmd = twob;
                                    cmd = "XIO";
                                    mem = line.text[2];
                                break;
                                case "ANB":
                                    /*branch = ProcessBranch(line, ProcessedLines, rung);
                                    // and branch, links two branches, no processing?
                                    oldCmd = branch.oldCmd;
                                    mem = branch.mem;
                                    cmd = branch.cmd;
                                    hasClosingBracket = branch.hasClosingBracket;*/
                                    oldCmd = "ANB";
                                    mem = "";
                                    cmd = "";
                                    hasClosingBracket = false;
                                break;
                                case "ORB":
                                    branch = ProcessBranch(line, ProcessedLines, rung);
                                    oldCmd = branch.oldCmd;
                                    mem = branch.mem;
                                    cmd = branch.cmd;
                                    hasClosingBracket = branch.hasClosingBracket;               
                                break;
                                case "RES":
                                    oldCmd = twob;
                                    cmd = "OTU";
                                    mem = line.text[2];
                                break;
                                case "SET":
                                    oldCmd = twob;
                                    cmd = "OTL";
                                    mem = line.text[2];
                                break;
                                case "OUT":
                                    oldCmd = twob;
                                    cmd = "OTE";
                                    mem = line.text[2];
                                    
                                    if (mem.Substring(0, 2) == "TD")
                                    {
                                        cmd = "TON";
                                        
                                        string c4 = line.text[3];
                                        c4 = c4.Substring(0, c4.IndexOf("s"));
                                        double c4i = Convert.ToDouble(c4) * 1000;
                                        int c5 = Convert.ToInt32(line.text[4]);
                                        int fi = Convert.ToInt32(c4i) * c5;
                                        mem = mem + "," + fi + ",0";
                                        oldCmd = line.text[2] + " " + line.text[3] + " " + line.text[4];
                                    }
                                    
                                break;
                                case "ORI":
                                case "OR":
                                    oldCmd = twob;
                                    if (oldCmd == "OR")
                                    {
                                        cmd = "XIC";
                                    }
                                    else
                                    {
                                        cmd = "XIO";
                                    }
                                    
                                    mem = line.text[2];

/*
                                    // if line before is an AND, go one higher up again
                                    // if it's a bracketed command, go up and put bracket around start
                                    bool replaced = false;
                                    bool closeBracketFound = false;
                                    int lnBefore = ProcessedLines.Count - 1;
                                    while (!replaced)
                                    {
                                        ProcessedLine p = ProcessedLines[lnBefore];
                                        if (!closeBracketFound)
                                        {
                                            switch (p.oldCmd)
                                            {
                                                case "LD":
                                                case "LDI":
                                                    p.cmd = "[" + p.cmd;
                                                    replaced = true;
                                                break;
                                                case "ORI":
                                                case "OR":
                                                    p.cmd = ","
                                                    + p.cmd.Replace(",", "");
                                                    p.hasClosingBracket = false;

                                                    replaced = true;
                                                break;
                                                case "ORB":
                                                    // that means closing bracket, so find opening bracket and wrap that
                                                    closeBracketFound = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (p.cmd.Contains("["))
                                            {
                                                p.cmd = "[" + p.cmd;
                                                replaced = true;
                                            }
                                        }

                                        lnBefore--;
                                    }*/
                                      // comma between cmds, last bracket in processing of rung
                                    cmd = "," + cmd;
                                    hasClosingBracket = true;
                                    // go up one, if OR, remove that closing bracket
                                    // if and, keep going to a non and
                                    bool replaced = false;
                                    int lnBefore = ProcessedLines.Count;
                                    int loopCount = 0;
                                    int ldsNeeded = 1;
                                    while (!replaced)
                                    {
                                        loopCount++;
                                        lnBefore--;
                                        ProcessedLine p = ProcessedLines[lnBefore];

                                        switch (p.oldCmd)
                                        {
                                            case "LD":
                                            case "LDI":
                                                ldsNeeded--;
                                                if (ldsNeeded == 0)
                                                {
                                                    p.cmd = "[" + p.cmd;
                                                    replaced = true;
                                                }
                                            break;
                                            case "ORI":
                                            case "OR":
                                                if (loopCount == 1)
                                                {
                                                    p.cmd = "," + p.cmd.Replace(",", "");
                                                    p.hasClosingBracket = false;
                                                    replaced = true;
                                                }                                               
                                            break;
                                            case "ANB":
                                            case "ORB":
                                                ldsNeeded++;
                                            break;
                                        }
                                    }
                                break;
                                case "MPS":
                                    oldCmd = twob;
                                    cmd = "XIO";
                                    mem = "MPS";
                                break;
                                case "MRD":
                                    oldCmd = twob;
                                    cmd = "XIO";
                                    mem = "MRD";
                                break;
                                case "MPP":
                                    oldCmd = twob;
                                    cmd = "XIO";
                                    mem = "MPP";
                                break;
                                case "NOT":
                                    oldCmd = twob;
                                    cmd = "XIO";
                                    mem = "NOT";
                                break;
                                case "AND":
                                    oldCmd = twob;
                                    cmd = "XIC";
                                    mem = line.text[2];
                                break;
                                case "ANI":
                                    oldCmd = twob;
                                    cmd = "XIO";
                                    mem = line.text[2];
                                break;
                                case "[":
                                case "]":
                                    // if it's only command on the line or command + rungnum, ignore it
                                    if (line.text.Length == 2 
                                    || (line.text.Length == 3 && line.rungNum == line.text[line.text.Length - 1]))
                                    {
                                        
                                    }
                                    else
                                    {
                                        throw new NotImplementedException();
                                    }
                                break;
                                default:
                                    // cmd not found
                                    missingCmd.Add(twob);
                                    //throw new NotImplementedException();
                                    break;
                            }
                        }

                        if (cmd.Length > 0 || oldCmd.Length > 0)
                        {
                            ProcessedLine ln = new ProcessedLine(lnNum, oldCmd, oldCmdMath, cmd, mem, rngNum, hasClosingBracket);
                            ProcessedLines.Add(ln);
                        }
                    }
                }
                else
                {
                    string lnNum = rung[0].text[0];
                    string oldCmd = "SKIPPEDBRANCH";
                    string oldCmdMath = "";
                    string cmd = "";
                    string mem = "";
                    string rngNum = rung[0].text[rung[0].text.Length - 1];

                    ProcessedLine ln = new ProcessedLine(lnNum, oldCmd, oldCmdMath, cmd, mem, rngNum, false);
                    ProcessedLines.Add(ln);
                }

                ProcessedRung processedRung = ProcessLines(ProcessedLines);
                processedRung.rungCount = processedRungs.Count;
                processedRungs.Add(processedRung);
            }

            foreach (ProcessedRung pr in processedRungs)
            {
                Console.WriteLine(pr.text + " " + pr.rungNum);
                
            }
/* 
            foreach(string mc in missingCmd)
            {
                Console.WriteLine("MISSING COMMAND: " + mc);
            }
            Console.WriteLine("Multi ORB Count: " + multiOrbCount.ToString());
            Console.WriteLine("Multi ANB Count: " + multiANBCount.ToString());
 
            for (int miss = 1; miss <= 7389; miss++)
            {
                if (!processedRungs.Where(
                        e => Convert.ToInt32(e.rungNum.Replace("(", "").Replace(")", "")) == miss).Any())
                {
                    Console.WriteLine("NO RUNG: " + miss.ToString());
                }
            }

            var uniq = new HashSet<string>(multiOrbRung.ToArray());

            foreach (string s in uniq)
            {
                Console.WriteLine("SKIPPED RUNG: " + s);
            }
*/
            // build XML document
            XDocument xmlDoc = BuildXML(processedRungs);
            // decode data due to bad allen bradley sw not following xml spec, save doc
            string data = HttpUtility.HtmlDecode(xmlDoc.ToString());
            data = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" 
                + Environment.NewLine + data;
            using (StreamWriter file = new StreamWriter(File.Create(@"/home/danni/Code/HLadderAll.L5X")))
            {
                file.WriteLine(data);
            }

            using (StreamWriter file = new StreamWriter(File.Create(@"/home/danni/Code/ANBLines.txt")))
            {
                foreach (string s in ANBRungs)
                {
                    file.WriteLine(s);
                }
            }

            // now we merge files
            MergeFiles();
        }

        static private Branch ProcessBranch(UnprocessedLine line, List<ProcessedLine> ProcessedLines
                    , List<UnprocessedLine> unprocessedLines)
        {
            string oldCmd = line.text[1];
            string cmd = "";
            string mem = "";
            bool hasClosingBracket = true;
            


            bool secondLastCmdIsANB = false;
            /* special case, import fails if ANB (is last command (so last bracket)
                or following commands are only AND/ANI)
                and opening bracket goes on first command
                */
           /* string lineNum = line.text[0];
            bool secondLastCmdIsANB = true;
            bool foundLine = false;
            foreach (UnprocessedLine ul in unprocessedLines)
            {
                if (ul.text[0] == lineNum)
                {
                    foundLine = true;
                }
                if (foundLine)
                {
                    // check if every command from here is an AND variant
                    switch (ul.text[1])
                    {
                        case "AND":
                        case "ANI":
                        case "ANB":
                        case "OUT":
                        case "[": // math command
                        case "]": // math command

                            // do nothing
                        break;
                        default:
                            // some non-and is here
                            if (!TestMath(ul.text))
                            {
                                secondLastCmdIsANB = false;
                            }
                        break;
                    }
                }
                if (!secondLastCmdIsANB)
                {
                    break;
                }
            }*/

            /*
            special case: if ANB is wrapping commands that are only ANDS
            , no brackets needed (this includes a sub bracketed OR)
            
             */

            // loop backwards through processed lines to find opening bracket
            int ldsNeeded = 2;
            int pLineCount = ProcessedLines.Count;
            bool searching = true;
            bool hasPlacedComma = false;
            while (searching)
            {
                pLineCount--;
                ProcessedLine pl = ProcessedLines[pLineCount];
                if (ldsNeeded == 0)
                {
                    searching = false;
                }
                else
                {         
                    switch (pl.oldCmd)
                    {
                        case "LD":
                        case "LDI":
                            ldsNeeded--;
                            if (ldsNeeded == 0)
                            {
                                if (secondLastCmdIsANB && pl.lineNumi == 1)
                                {
                                    // no opening bracket, no closing bracket
                                    hasClosingBracket = false;
                                }
                                else
                                {
                                    // we have found opening bracket
                                    pl.cmd = "[" + pl.cmd;
                                }
                                
                                searching = false;
                            }
                            else
                            {
                                switch (oldCmd)
                                {
                                    case "ANB":
                                        // nothing
                                    break;
                                    case "ORB":
                                        if (ldsNeeded == 1 && !hasPlacedComma)
                                        {
                                            pl.cmd = "," + pl.cmd.Replace(",", "");
                                            hasPlacedComma = true;
                                        }
                                    break;
                                }
                            }
                        break;
                        case "ORI":
                        case "OR":
                            // is this needed?
                            //pl.hasClosingBracket = false;
                        break;
                        case "ORB":
                            ldsNeeded++;
                        break;
                        case "ANB":
                            ldsNeeded++;
                        break;
                    }
                }
            }

            Branch b = new Branch(oldCmd, cmd, mem, hasClosingBracket);
            return b;
        }

        static private void MergeFiles()
        {
            // load xml docs
            XDocument all = XDocument.Load(File.Open(@"/home/danni/Code/HLadderAll.L5X", FileMode.Open));
            XDocument merge = XDocument.Load(File.Open(@"/home/danni/Code/RungOverride.L5X", FileMode.Open));

            // now we need to merge documents
            List<XElement> newRungs = merge.Descendants("Rung").ToList();

            // go through old rungs, find rung number, if rung matches, insert it
            foreach (XElement rung in newRungs)
            {
                // figure out rung number
                string comment = rung.Descendants("Comment").First().Value;
                Console.WriteLine("MERGING XML NODE: " + comment);
                int rungNum = Convert.ToInt32(comment.Replace("#", "").Trim());
                
                // see if there's a rung with current rung num
                if (
                    all.Descendants("Rung").Where(
                        r => Convert.ToInt32(r.Descendants("Comment").First().Value
                            //.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).First()
                            .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None).First()
                            .Replace("#", "").Trim()) == rungNum
                    ).Any()
                )
                {
                    all.Descendants("Rung").Where(
                        r => Convert.ToInt32(r.Descendants("Comment").First().Value
                            //.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).First()
                            .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None).First()
                            .Replace("#", "").Trim()) == rungNum
                    ).First().ReplaceWith(rung);
                }
                else
                {
                    // get a rung that's close to it and then navigate
                    XElement close = all.Descendants("Rung").Where(
                        r => Convert.ToInt32(r.Attribute("Number").Value) == rungNum
                    ).First();
                    // figure out where we are
                    int closeRung = Convert.ToInt32(close.Descendants("Comment").First().Value
                                //.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).First()
                                .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None).First()
                                .Replace("#", "").Trim());

                    // now find element above where we want to insert and insert after it
                    // this will break if we ever need to "merge" lines at end
                    int parLine = rungNum - (closeRung - rungNum);
                    all.Descendants("Rung")
                        .Where(r => Convert.ToInt32(r.Attribute("Number").Value) == parLine)
                        .First()
                        .AddAfterSelf(rung);
                }
            }
            
            // renumber lines
            int count = 0;
            foreach (XElement e in all.Descendants("Rung"))
            {
                e.Attribute("Number").SetValue(count);
                count++;
            }
            all.Save(@"/home/danni/Code/HLadderMerged.L5X");
        }

        static private ProcessedRung ProcessLines(List<ProcessedLine> plList)
        {
            string start = @"<![CDATA[";
            string end = @"]]>";
            string rungNum = "";
            string cmd = "";
            string comment = "# " + plList[0].rungNum.Replace("(", "").Replace(")", "") + " " + Environment.NewLine;
            if (plList[0].rungNum == "(01079)")
            {
                bool mer2 = true;
            }
            ProcessedLine lastLine = null;
            foreach (ProcessedLine pl in plList)
            {
                if (pl.oldCmd.Contains("ORB"))
                {
                    if (lastLine != null)
                    {
                        if (lastLine.oldCmd.Contains("ANB"))
                        {
                            ANBRungs.Add("ANB BEFORE ORB: " + plList[0].rungNum);
                        }
                    }
                }
                if (rungNum.Length == 0 && pl.rungNum.Length != 0)
                {
                    rungNum = pl.rungNum;
                }

                if ((pl.mem.Contains("(") || pl.mem.Contains(")")
                    || pl.cmd.Length == 0 || pl.mem.Length == 0) && pl.oldCmd != "ORB" && pl.oldCmd != "ANB"
                    && pl.oldCmd != "SKIPPEDBRANCH")
                {
                    // shouldn't happen
                    throw new NotImplementedException();
                }

                // timer fix
                if ((pl.cmd.Contains("XIC") || pl.cmd.Contains("XIO")) && pl.mem.Contains("TD"))
                {
                    pl.mem += ".DN";
                }
                if (pl.mem.Contains("DIF"))
                {
                    if (pl.cmd.Substring(0,1) == "[")
                    {
                        pl.cmd = "[ONS";
                    }
                    else
                    {
                        pl.cmd = "ONS";
                    }
                }
                
                cmd += pl.cmd + (pl.mem.Length == 0 ? "" : "(" + pl.mem + ")");

                if (pl.hasClosingBracket)
                {
                    cmd += "]";
                }

                comment += (pl.MathLine ? pl.oldCmdMath : pl.oldCmd) + " ";
                lastLine = pl;
            }

            // special MPS, NOT, ANB rungs
            if (plList.Where(e => e.oldCmd == "NOT" || e.oldCmd == "MPS" || e.oldCmd == "SKIPPEDBRANCH"
                                || e.oldCmd == "MRP" || e.oldCmd == "MPP").Any())
            {
                cmd = "XIO(NOT)";
                comment = "# " + plList[0].rungNum.Replace("(", "").Replace(")", "") + " "
                + Environment.NewLine + "NOT, MPS, ANB, MRP, MPP or Skipped RUNG";
                multiOrbRung.Add(plList[0].rungNum);
            }

            string cmdString = start + cmd + ";" + end;
            comment = start + comment + end;        

            return new ProcessedRung(cmdString, rungNum, comment);
        }

        static private bool TestMath(string[] cells)
        {
            bool math = false;

            foreach (string c in cells)
            {
                switch (c)
                {
                                        case "=":
                    case "+":
                    case "==":
                    case "<":
                    case "<>":
                    case "<=":
                    case ">=":
                    case ">":
                    case "-":
                    case "*":
                    case "/":
                        math = true;
                    break;
                }
            }
            
            return math;
        }

        static private XDocument BuildXML(List<ProcessedRung> processedRungs)
        {
            XDocument doc = new XDocument(
                new XElement("RSLogix5000Content", new XAttribute("SchemaRevision", "1.0"), new XAttribute("SoftwareRevision", "20.01")
                , new XAttribute("TargetType", "Rung"), new XAttribute("TargetSubType", "RLL")
                , new XAttribute("ContainsContext", "true"), new XAttribute("Owner", "Engineering, Agile Projects Pty Ltd")
                , new XAttribute("ExportDate", "Wed Oct 25 09:24:04 2017"), new XAttribute("ExportOptions", "References DecoratedData Context Dependencies ForceProtectedEncoding AllProjDocTrans")
                    , new XElement("Controller", new XAttribute("Use", "Context"), new XAttribute("Name", "ProductionPLC")
                    , new XElement("DataTypes", new XAttribute("Use", "Context"))
                    , new XElement("Tags", new XAttribute("Use", "Context"))
                    , new XElement("Programs", new XAttribute("Use", "Context")
                        , new XElement("Program", new XAttribute("Use", "Context"), new XAttribute("Name", "MainProgram")
                            , new XElement("Routines", new XAttribute("Use", "Context")
                                , new XElement("Routine", new XAttribute("Use", "Context"), new XAttribute("Name", "MainRoutine"), new XAttribute("Type", "RLL")
                                    , new XElement("RLLContent"
                                        , processedRungs.Select(e => 
                                            new XElement("Rung", new XAttribute("Use", "Target"), new XAttribute("Number", e.rungCount), new XAttribute("Type", "N")
                                                , new XElement("Comment", e.comment), new XElement("Text", e.text)))
            ))))))));
            return doc;
        }

        static private string TCCheck(string s)
        {
            if (s.Contains("TC"))
            {
                s = s.Replace("TC", "TD");
                s += ".ACC";
            }
            return s;
        }
    }

class Branch
{
    public string oldCmd;
    public string cmd;
    public string mem;
    public bool hasClosingBracket;
    public Branch(string oldCmd, string cmd, string mem, bool hasClosingBracket)
    {
        this.oldCmd = oldCmd;
        this.cmd = cmd;
        this.mem = mem;
        this.hasClosingBracket = hasClosingBracket;
    }
}
class MathType
{
    public string newType;
    public string oldType;
    public MathType(string[] cells)
    {
        string t = "";
        string ot = "";
        foreach (string c in cells)
        {
            switch (c)
            {
                case "+":
                    t = "ADD";
                    ot = c;
                break;
                case "==":
                    t = "EQU";
                    ot = c;
                break;
                case "<":
                    t = "LES";
                    ot = c;
                break;
                case "<>":
                    t = "NEQ";
                    ot = c;
                break;
                case "<=":
                    t = "LEQ";
                    ot = c;
                break;
                case ">":
                    t = "GRT";
                    ot = c;
                    break;
                case ">=":
                    t = "GEQ";
                    ot = c;
                    break;
                case "*":
                    t = "MUL";
                    ot = c;
                    break;
                case "/":
                    t = "DIV";
                    ot = c;
                    break;
                case "-":
                    t = "SUB";
                    ot = c;
                    break;
                default:
                    // hide case in here to ensure fall through of cases
                    if (c == "=")
                    {
                        t = "MOV";
                        ot = c;
                    }
                break;
            }
        }

        newType = t;
        oldType = ot;
    }
}
    class myValues
    {
        public string one;
        public string twoCmd;
        public string twoMem;
        //public string three;
        //public string four;
        public string five;

        public myValues(string one, string twoCmd, string twoMem, string five)
        {
            this.one = one;
            this.twoCmd = twoCmd;
            this.twoMem = twoMem;
            this.five = five;
        }
    }

    class ProcessedRung
    {
        public string text;
        public string rungNum;
        public int rungCount;
        public string comment;
        public ProcessedRung(string text, string rungNum, string comment)
        {
            this.text = text;
            this.rungNum = rungNum;
            this.comment = comment;
        }
    }

    class UnprocessedLine
    {
        public string[] text;
        public string rungNum;
        public UnprocessedLine(string[] text, string rungNum)
        {
            this.text = text;
            this.rungNum = rungNum;
        }
    }

    class ProcessedLine
    {
        public string lineNum;
        public int lineNumi;
        public string rungNum;
        public string oldCmd;
        public string oldCmdMath = "";
        public bool MathLine = false;
        public string cmd;
        public string mem;
        public bool hasClosingBracket;
        public ProcessedLine(string lineNum, string oldCmd, string oldCmdMath, string cmd, string mem, string rungNum
                        , bool hasClosingBracket)
        {
            this.lineNum = lineNum;
            this.lineNumi = Convert.ToInt32(lineNum.Replace("(", "").Replace(")", ""));
            this.rungNum = rungNum;
            this.oldCmd = oldCmd;
            this.cmd = cmd;
            this.mem = mem;
            this.hasClosingBracket = hasClosingBracket;
            if (oldCmdMath.Length > 0)
            {
                this.oldCmdMath = oldCmdMath;
                MathLine = true;
            }
        }
    }

    static class myDict
    {
        static public Dictionary<string, string> Commands() 
        {
            Dictionary<string, string> commands = new Dictionary<string, string>();
            commands.Add("LD", "XIC");
            commands.Add("ANI", "XIO");
            commands.Add("OUT", "OTE");
            commands.Add("OR", "OR");
            commands.Add("LDI", "XIO");
            commands.Add("ORI", "XIO"); // disconcerting as in hitachi it's a branch for rung 9
            commands.Add("AND", "XIC"); // special logic for this, in case it's not just after an LD
            commands.Add("[", "");
            commands.Add("]", "");

            return commands;
        }

        static public List<string> MathCommands()
        {
            List<string> mc = new List<string>();
            mc.Add("ADD");
            mc.Add("EQU");
            mc.Add("NEQ");
            mc.Add("MOV");
            mc.Add("LES");
            mc.Add("LEQ");
            mc.Add("GEQ");
            mc.Add("GRT");
            mc.Add("SUB");
            mc.Add("MUL");
            mc.Add("DIV");


            return mc;
        }
    }
}