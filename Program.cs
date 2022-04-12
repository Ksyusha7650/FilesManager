using System;

class Program
{

    public static void Main()
    {
        List<Clasters> clasters = new List<Clasters>();
        List<Files> files = new List<Files>();
        Console.WriteLine("Программа по исправлению возможных ошибок файловой системы запущена.");
        FilesWork.ReadFile(ref clasters, ref files);
        Files.MakeClastersList(clasters, ref files);
        Console.WriteLine("Файлы и их кластеры: ");
        Files.OutputFiles(clasters, files);
        Clasters.CheckMissedClasters(clasters, ref files);
        Console.WriteLine("Исправлены потерянные кластеры:");
        Files.OutputFiles(clasters, files);
        Clasters.CheckCrossedClasters(ref clasters, files);
        Console.WriteLine("Исправлены пересекающиеся кластеры:");
        Files.OutputFiles(clasters, files);
        Clasters.CheckBadInFile(ref clasters, files);
        Console.WriteLine("Исправлено наличие bad кластеров:");
        Files.OutputFiles(clasters, files);
        Console.WriteLine(Environment.NewLine + "Так выглядит измененная архитектура файловой системы: " + Environment.NewLine);
        foreach (Clasters file in clasters)
        {
            Console.WriteLine(file.numberClaster + "-" + file.numberNextClaster);
        }
    }
}

public static class FilesWork
{
    private const string pathFat = "C:\\Users\\ksysh\\source\\repos\\FilesManager\\FAT.txt";
    private const string pathCla = "C:\\Users\\ksysh\\source\\repos\\FilesManager\\Clasters.txt";

    public static void ReadFile(ref List<Clasters> clasters, ref List<Files> files)
    {
        using (StreamReader sr = File.OpenText(pathFat))
        {
            string line = "";
            int nC = 0; Object nNC;
            while ((line = sr.ReadLine()) != null)
            {
                string[] nums = line.Split('-');
                nC = Int32.Parse(nums[0]);
                if (nums.Length > 1)
                    nNC = nums[1];
                else nNC = null;
                clasters.Add(new Clasters(nC, nNC));
            }
        }
        foreach (Clasters file in clasters)
        {
            Console.WriteLine(file.numberClaster + "-" + file.numberNextClaster);
        }
        using (StreamReader sr = File.OpenText(pathCla))
        {
            string line = "";
            char name = 'A';
            while ((line = sr.ReadLine()) != null)
            {
                List<Clasters> clastersList = new List<Clasters>();
                int firstClaster = 0;
                string[] lines = line.Split(':');
                name = char.Parse(lines[0]);
                firstClaster = Int32.Parse(lines[1]);
                clastersList.Add(clasters[firstClaster - 1]);
                files.Add(new Files(name, clastersList));
            }
        }
        foreach (Files file in files)
        {
            Console.Write(file.nameFile + ":");
            foreach (Clasters c in file.clasters)
            {
                Console.Write(c.numberClaster + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();

    }

}

public class Clasters
{
    public int numberClaster = 0;
    public Object numberNextClaster = 0;
    private const int COUNT_CLASTERS = 27;
    public Clasters(int nC, Object nNC)
    {
        numberClaster = nC;
        numberNextClaster = nNC;
    }
    public static Clasters FindClaster(int number, List<Clasters> cl)
    {
        return cl[number - 1];
    }
    public static void CheckMissedClasters(List<Clasters> cl, ref List<Files> f)
    {
        foreach (Clasters clast in cl)
        {
            bool check = false;
            if ((clast.numberNextClaster != null) && !(clast.numberNextClaster.Equals("bad")) && !(clast.numberNextClaster.Equals("eof")))
            {
                foreach (Files files in f)
                {
                    if (files.clasters.Contains(clast))
                    {
                        check = true;
                        break;
                    }
                }
                if (!check)
                {
                    int code = f.Last().nameFile + 1;
                    char name = (char)code;
                    List<Clasters> clastersList = new List<Clasters>();
                    clastersList.Add(clast);
                    Files file = new Files(name, clastersList);
                    Files.MakeClastersList(cl, ref file);
                    f.Add(file);
                }
            }
        }
    }
    public static Clasters? EmptyClaster(List<Clasters> used, List<Clasters> cl)
    {
        int index = 0;
        while (index < COUNT_CLASTERS - 1)
        {
            if ((used.Contains(cl[index])) || (cl[index].numberNextClaster != null))
                index++;
            else return cl[index];
        }
        return null;
    }
    public static void CheckCrossedClasters(ref List<Clasters> cl, List<Files> f)
    {
        List<Clasters> allUsedClasters = new List<Clasters>(), distinctUsedClasters = new List<Clasters>();

        foreach (Files files in f)
        {
            foreach (Clasters clast in files.clasters)
            {
                allUsedClasters.Add(clast);
            }
        }

        foreach (Files files in f)
        {
            int index = 0;

            for (index = 0; index < files.clasters.Count; index++)
            {
                if (distinctUsedClasters.Contains(files.clasters[index]))
                {
                    Object next = files.clasters[index].numberNextClaster;
                    files.clasters[index] = EmptyClaster(allUsedClasters, cl);
                    files.clasters[index].numberNextClaster = next;
                    if (index > 0)
                        cl[files.clasters[index - 1].numberClaster - 1].numberNextClaster = files.clasters[index].numberClaster;
                    allUsedClasters.Add(files.clasters[index]);
                }
                distinctUsedClasters.Add(files.clasters[index]);
            }
        }
    }

    public static void CheckBadInFile(ref List<Clasters> cl, List<Files> f)
    {
        List<Clasters> allUsedClasters = new List<Clasters>();

        foreach (Files files in f)
        {
            foreach (Clasters clast in files.clasters)
            {
                allUsedClasters.Add(clast);
            }
        }

        foreach (Files files in f)
        {

            for (int index = 0; index < files.clasters.Count; index++)
            {
                if (files.clasters[index].numberNextClaster.Equals("bad"))
                {
                    Clasters empty = EmptyClaster(allUsedClasters, cl);
                    empty.numberNextClaster = "eof";
                    files.clasters[index] = empty;
                    cl[files.clasters[index].numberClaster - 1] = empty;
                }
            }
        }
    }
}

public class Files
{
    public char nameFile;
    public List<Clasters> clasters;

    public Files(char name, List<Clasters> el)
    {
        nameFile = name;
        clasters = el;
    }
    public static void MakeClastersList(List<Clasters> cl, ref List<Files> f)
    {
        foreach (Files file in f)
        {
            int nextCl = 0;
            Clasters t = file.clasters[0];
            Object next = t.numberNextClaster;
            while ((!next.Equals("eof")) && (!next.Equals("bad")))
            {
                nextCl = Convert.ToInt32(next);
                Clasters nextClaster = Clasters.FindClaster(nextCl, cl);
                file.clasters.Add(nextClaster);
                next = nextClaster.numberNextClaster;
            }
        }
    }
    public static void MakeClastersList(List<Clasters> cl, ref Files f)
    {
        int nextCl = 0;
        Clasters t = f.clasters[0];
        Object next = t.numberNextClaster;
        while (!next.Equals("eof"))
        {
            nextCl = Convert.ToInt32(next);
            Clasters nextClaster = Clasters.FindClaster(nextCl, cl);
            if (next.Equals("bad")) return;
            else f.clasters.Add(nextClaster);
            next = nextClaster.numberNextClaster;
        }
    }
    public static void OutputFiles(List<Clasters> cl, List<Files> files)
    {
        foreach (Files file in files)
        {
            Console.Write(file.nameFile + ":");
            foreach (Clasters c in file.clasters)
            {
                Console.Write(c.numberClaster + " ");
            }
            Console.WriteLine();
        }
    }
}

