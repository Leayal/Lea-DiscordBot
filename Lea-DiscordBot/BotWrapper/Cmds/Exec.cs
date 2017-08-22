using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Leayal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.IO;
using System.IO;
using System.Reflection;
using System;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.Linq;

namespace LeaDiscordBot.BotWrapper.Cmds
{
    public static class Exec
    {
        public static async Task ProcessMessage(Bot self, DiscordSocketClient client, SocketMessage message, string cmdPrefix, params string[] splittedMsg)
        {
            if (message.Source != MessageSource.User) return;
            if (message.Author.Id != self.Owner.Id) return;
            if (splittedMsg != null && splittedMsg.Length > 1)
            {
                string assemblyName = message.Author.Id.ToString();
                Microsoft.CodeAnalysis.Compilation compiler = null;
                SyntaxTree sourceCode = null;
                if (splittedMsg[0].IsEqual("vb", true))
                {
                    int rawcodesStart = message.Content.IndexOf("```vb");
                    if (rawcodesStart > -1)
                    {
                        rawcodesStart += 5;
                        int rawcodesEnd = message.Content.IndexOf("```", rawcodesStart);
                        if (rawcodesEnd > -1)
                        {
                            sourceCode = VisualBasicSyntaxTree.ParseText(message.Content.Substring(rawcodesStart, rawcodesEnd - rawcodesStart));
                            compiler = VisualBasicCompilation.Create(assemblyName, new[] { sourceCode });
                        }
                    }
                }
                else if (splittedMsg[0].IsEqual("cs", true) || splittedMsg[0].IsEqual("csharp", true))
                {
                    int rawcodesStart = message.Content.IndexOf("```csharp");
                    if (rawcodesStart > -1)
                    {
                        rawcodesStart += 9;
                        int rawcodesEnd = message.Content.IndexOf("```", rawcodesStart);
                        if (rawcodesEnd > -1)
                        {
                            sourceCode = CSharpSyntaxTree.ParseText(message.Content.Substring(rawcodesStart, rawcodesEnd - rawcodesStart));
                            compiler = CSharpCompilation.Create(assemblyName, new[] { sourceCode }, null, new CSharpCompilationOptions(OutputKind.NetModule, false, null, null, null, null, OptimizationLevel.Release, true, false, null, null, default(System.Collections.Immutable.ImmutableArray<byte>), null, Platform.AnyCpu, ReportDiagnostic.Default, 4, null, false, false, null, null, null, null, null, false));
                        }
                    }
                }
                else
                {
                    await SendInvalidParams(client, message, cmdPrefix, "Invalid .NET Language.");
                }

                if (sourceCode == null)
                {
                    await message.Channel.SendMessageAsync("Empty code cannot be execute, can it?\nBecause there is nothing to execute.");
                    return;
                }

                string entryPoint = null;
                if (splittedMsg.Length == 3)
                {
                    entryPoint = $"{assemblyName}.Main()";
                }
                else
                {
                    entryPoint = $"{assemblyName}.Main()";
                    // entryPoint = $"{splittedMsg[3]}.Main()";
                }

                using (RecyclableMemoryStream rms = new RecyclableMemoryStream(Program.memoryMgr))
                {
                    EmitResult emitresult = compiler.Emit(rms);
                    if (!emitresult.Success)
                    {
                        IEnumerable<Diagnostic> failures = emitresult.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);



                        foreach (Diagnostic diagnostic in failures)
                        {
                            Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        }
                    }
                    else
                    {
                        rms.Seek(0, SeekOrigin.Begin);
                        var asl = new AssemblyLoader();
                        var asm = asl.LoadFromStream(rms);

                        var type = asm.GetType("MyClassLib.SampleClasses.Sample");
                        dynamic obj = Activator.CreateInstance(type);
                        await message.Channel.SendMessageAsync("Empty code cannot be execute, can it?\nBecause there is nothing to execute.");
                        var asdads = obj.SayHello("John Doe");
                        Console.WriteLine(obj.SayHello("John Doe"));
                        
                    }
                }
                // Microsoft.CodeAnalysis.Compilation.GetRequiredLanguageVersion 


            }
            else
            {
                await SendInvalidParams(client, message, cmdPrefix);
            }
        }

        class AssemblyLoader : System.Runtime.Loader.AssemblyLoadContext
        {
            // Not exactly sure about this
            protected override Assembly Load(AssemblyName assemblyName)
            {
                return null;
                /*var deps = DependencyContext.Default;
                var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
                var assembly = Assembly.Load(new AssemblyName(res.First().Name));
                return assembly;*/
            }
        }

        private static async Task SendInvalidParams(DiscordSocketClient client, SocketMessage message, string cmdPrefix, string reason = "")
        {
            if (string.IsNullOrWhiteSpace(reason))
                await message.Channel.SendMessageAsync("```\n" +
                        "Usage:\n" +
                        cmdPrefix + "Exec <.NET Language> <Name> [Entry Point]\n" +
                        cmdPrefix + " <.NET Language>: \"csharp\" or \"cs\" for C#. Or \"vb\" for VisualBasic.NET." +
                        cmdPrefix + " <.Name>: The name of assembly." +
                        cmdPrefix + " [Entry Point]: Literally entry point. If this param is missing, the compiler will try to find Main() method." +
                        "Ex:" + cmdPrefix + "exec cs foo\n" +
                        "   " + cmdPrefix + "exec vb foo.bar(\"param\")\n" +
                        "```");
            else
                await message.Channel.SendMessageAsync(reason + "\n```\n" +
                        "Usage:\n" +
                        cmdPrefix + "Exec <.NET Language> <Name> [Entry Point]\n" +
                        cmdPrefix + " <.NET Language>: \"csharp\" or \"cs\" for C#. Or \"vb\" for VisualBasic.NET." +
                        cmdPrefix + " <.Name>: The name of assembly." +
                        cmdPrefix + " [Entry Point]: Literally entry point. If this param is missing, the compiler will try to find Main() method." +
                        "Ex:" + cmdPrefix + "exec cs foo\n" +
                        "   " + cmdPrefix + "exec vb foo.bar(\"param\")\n" +
                        "```");
        }
    }
}
