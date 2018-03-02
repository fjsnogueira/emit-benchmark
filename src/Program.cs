using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


namespace ParallelCompilation
{
    public class Program
    {
        static void Main(string[] args)
        {
            var compilations = CompilationData.Compilations;
            Console.WriteLine($"Testing with {compilations.Length} compilations");
            BenchmarkRunner
                .Run<CompilationBenchmark>(
                    ManualConfig
                        .Create(DefaultConfig.Instance)
                        .With(Job.Clr)
                        .With(ExecutionValidator.FailOnError));
        }
    }

    public class CompilationBenchmark
    {
        private static string GetSequentialOutputFolder() => GetOutputFolder("sequential");
        private static string GetParallelOutputFolder() => GetOutputFolder("parallel");

        private static string GetOutputFolder(string testType)
        {
            var temp = Path.GetTempPath();
            var runFolder = Path.GetRandomFileName();
            string sequentialOutputFolder = Path.Combine(temp, runFolder, testType);
            Directory.CreateDirectory(sequentialOutputFolder);
            return sequentialOutputFolder;
        }

        [Benchmark(Baseline = true)]
        public void EmitSequential()
        {
            var sequentialOutputFolder = GetSequentialOutputFolder();
            var compilations = CompilationData.Compilations;
            for (int i = 0; i < compilations.Length; i++)
            {
                compilations[i].Emit(Path.Combine(sequentialOutputFolder, "compilation" + i));
            }

        }

        [Benchmark]
        public void EmitParallel()
        {
            var parallelOutputFolder = GetParallelOutputFolder();
            var compilations = CompilationData.Compilations;
            Parallel.For(0, compilations.Length, i =>
            {
                compilations[i].Emit(Path.Combine(parallelOutputFolder, "compilation" + i));
            });
        }
    }

    public static class CompilationData
    {
        public static Compilation[] Compilations => CompilationsLazy.Value;
        private static Lazy<Compilation[]> CompilationsLazy = new Lazy<Compilation[]>(() =>
        {
            var compilations = new Compilation[100];
            for (int i = 0; i < 100; i++)
            {
                compilations[i] = CSharpCompilation.Create("compilation" + i, SyntaxTrees, References);
            }

            return compilations;
        }, LazyThreadSafetyMode.ExecutionAndPublication);
        public static SyntaxTree[] SyntaxTrees => SyntaxTreesLazy.Value;
        private static readonly Lazy<SyntaxTree[]> SyntaxTreesLazy =
            new Lazy<SyntaxTree[]>(() => new[] { CodeTree, ClassTree }, LazyThreadSafetyMode.ExecutionAndPublication);
        public static SyntaxTree CodeTree => CodeTreeLazy.Value;
        private static readonly Lazy<SyntaxTree> CodeTreeLazy =
            new Lazy<SyntaxTree>(() => CSharpSyntaxTree.ParseText(code), LazyThreadSafetyMode.ExecutionAndPublication);
        public static SyntaxTree ClassTree => ClassTreeLazy.Value;
        private static readonly Lazy<SyntaxTree> ClassTreeLazy =
            new Lazy<SyntaxTree>(() => CSharpSyntaxTree.ParseText(@class), LazyThreadSafetyMode.ExecutionAndPublication);
        public static MetadataReference[] References => ReferencesLazy.Value;
        private static readonly Lazy<MetadataReference[]> ReferencesLazy =
            new Lazy<MetadataReference[]>(() => new[] { CorlibReference, SystemCoreReference }, LazyThreadSafetyMode.ExecutionAndPublication);
        public static MetadataReference CorlibReference => CorlibReferenceLazy.Value;
        private static readonly Lazy<MetadataReference> CorlibReferenceLazy =
            new Lazy<MetadataReference>(() => MetadataReference.CreateFromFile(typeof(object).Assembly.Location), LazyThreadSafetyMode.ExecutionAndPublication);
        public static MetadataReference SystemCoreReference => SystemCoreReferenceLazy.Value;
        private static readonly Lazy<MetadataReference> SystemCoreReferenceLazy =
            new Lazy<MetadataReference>(() => MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), LazyThreadSafetyMode.ExecutionAndPublication);

        // taken from https://github.com/dotnet/samples/blob/master/csharp/classes-quickstart/
        const string code = @"
using System;

namespace classes
{
    class Program
    {
        static void Main(string[] args)
        {
            var account = new BankAccount("" < name > "", 1000);
            Console.WriteLine($""Account {account.Number} was created for {account.Owner} with {account.Balance} balance."");

            account.MakeWithdrawal(500, DateTime.Now, ""Rent payment"");
            Console.WriteLine(account.Balance);
            account.MakeDeposit(100, DateTime.Now, ""friend paid me back"");
            Console.WriteLine(account.Balance);

            Console.WriteLine(account.GetAccountHistory());

            // Test that the initial balances must be positive:
            try
            {
                var invalidAccount = new BankAccount(""invalid"", -55);
    }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(""Exception caught creating account with negative balance"");
                Console.WriteLine(e.ToString());
            }

            // Test for a negative balance
            try
            {
                account.MakeWithdrawal(750, DateTime.Now, ""Attempt to overdraw"");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(""Exception caught trying to overdraw"");
                Console.WriteLine(e.ToString());
            }
        }
    }
}

";
        const string @class = @"
using System;
using System.Collections.Generic;

namespace classes
{
    public class BankAccount
    {
        public string Number { get; }
        public string Owner { get; set; }
#region BalanceComputation
        public decimal Balance 
        {
            get
            {
                decimal balance = 0;
                foreach (var item in allTransactions)
                {
                    balance += item.Amount;
                }

                return balance;
            }
        }
#endregion

        private static int accountNumberSeed = 1234567890;
#region Constructor
        public BankAccount(string name, decimal initialBalance)
        {
            this.Number = accountNumberSeed.ToString();
            accountNumberSeed++;

            this.Owner = name;
            MakeDeposit(initialBalance, DateTime.Now, ""Initial balance"");
        }
#endregion

    #region TransactionDeclaration
    private List<Transaction> allTransactions = new List<Transaction>();
    #endregion

    #region DepositAndWithdrawal
    public void MakeDeposit(decimal amount, DateTime date, string note)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), ""Amount of deposit must be positive"");
        }
        var deposit = new Transaction(amount, date, note);
        allTransactions.Add(deposit);
    }

    public void MakeWithdrawal(decimal amount, DateTime date, string note)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), ""Amount of withdrawal must be positive"");
        }
        if (Balance - amount < 0)
        {
            throw new InvalidOperationException(""Not sufficient funds for this withdrawal"");
        }
        var withdrawal = new Transaction(-amount, date, note);
        allTransactions.Add(withdrawal);
    }
    #endregion

    #region History
    public string GetAccountHistory()
    {
        var report = new System.Text.StringBuilder();

        report.AppendLine(""Date\t\tAmount\tNote"");
        foreach (var item in allTransactions)
        {
            report.AppendLine($""{item.Date.ToShortDateString()}\t{item.Amount}\t{item.Notes}"");
        }

        return report.ToString();
    }
    #endregion
}
}
";
    }
}
