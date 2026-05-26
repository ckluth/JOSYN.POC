using JOSYN.System.Frontend.JobHost.Attributes;
using System.Diagnostics;

#pragma warning disable IDE0130
namespace MyDemoJob;
#pragma warning restore IDE0130

public static class MyFirstJob
{
    [JobEntryPoint]
    public static MyResult Execute(MyArguments args)
    {
        Console.WriteLine("\nEXECUTING: MyResult Execute(MyArguments args)\n");
        
        //var s = Path.Combine(Path.GetTempPath(), "JOSYN", Process.GetCurrentProcess().ProcessName);
        //Console.WriteLine(s);
        //Console.ReadKey(true);

        //throw new Exception("MyDemoJob fucked up...");
        
        aaaaaaa();
        
        Console.WriteLine(args.Msg);

        return new MyResult
        {
            Count = args.Count + 1,
            Message = "Echo: " + args.Msg,
            Succeeded = true,
        };
    }


    private static void aaaaaaa()
    {
        bbbbbbbbbb();
    }
    private static void bbbbbbbbbb()
    {
        ccccccc();
    }

    private static void ccccccc()
    {
        Foo.Bar();
        
    }


    internal class Foo
    {
        internal static void Bar()
        {
            throw new Exception("OH NO - FUCK IT...");
        }
    }


    //[JobEntryPoint]
    //public static MyResult Execute2(string msg, int count)
    public static void Execute()
    {
        Console.WriteLine("Hey, executing MyFirstJob (ohne Parameter)\n");

        //throw new Exception("MyDemoJob fucked up...");
        
        Console.WriteLine("HELLO");
        

        //return new MyResult
        //{
        //    //Count = count + 1,
        //    Message = "OK",
        //    Succeeded = true,
        //};
    }







    //[ParallelExecutionAllowed]




    //[JobEntryPoint]
    //public static MyResult Execute2(string msg, int count)
    public static MyResult Execute2(string msg)
    {
        Console.WriteLine("Hey, executing MyFirstJob (mit Parameterliste)\n");

        //throw new Exception("MyDemoJob fucked up...");
        Console.WriteLine("Msg:" + msg);
        
        Console.ReadKey();

        return new MyResult
        {
            //Count = count + 1,
            Message = "Echo: " + msg,
            Succeeded = true,
        };
    }

    //[JobEntryPoint]
    public static void Execute3(string msg, int count)
    {
        Console.WriteLine("Hey, executing MyFirstJob3 (void)\n");
        
        Console.WriteLine(msg);
        Console.WriteLine(count);

        //throw new Exception("MyDemoJob fucked up...");
        
    }

//    [JobEntryPoint]
    public static void Execute4()
    {
        Console.WriteLine("Hey, executing MyFirstJob4 (void, no params)\n");

        //throw new Exception("MyDemoJob fucked up...");

    }



    #region Later...

    [BeforeJobEntryPoint]
    internal static void Initialize()
    {
        //        JobRunner<MyArguments>.ConditionalParallelExecutionAllowed = (args, otherArgs) => !otherArgs.Contains(args);
    }

    #endregion


}

