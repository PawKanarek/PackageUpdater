using PackageUpdater;
using System.Collections.Generic;
using Xunit;

namespace Tests
{
    public class ConsoleManagerTests
    {
        [Fact]
        public void DisplayHelpOnManagerWithoutInputs_Call_Works()
        {
            //arrange 
            var manager = new ConsoleManager(null);

            //act
            manager.DisplayHelp();

            //assert
            Assert.True(true);
        }

        [Fact]
        public void DisplayHelpOnManagerWithSingleInput_Call_Works()
        {
            //arrange 
            var param = new ConsoleInputParameter("x", "x", "x", null);
            var manager = new ConsoleManager(new List<ConsoleInputParameter> { param });

            //act
            manager.DisplayHelp();

            //assert
            Assert.True(true);
        }

        [Fact]
        public void ParseArgumentsOnManagerWithMultiInput_Call_Works()
        {
            //arrange 
            var param = new ConsoleInputParameter("x", "x", "x", null);
            var manager = new ConsoleManager(new List<ConsoleInputParameter> { param, param, param });

            //act
            manager.DisplayHelp();

            //assert
            Assert.True(true);
        }

        [Fact]
        public void ParseArguments_CallWithNull_Works()
        {
            //arrange 
            var manager = new ConsoleManager(null);

            //act
            manager.ParseArguments(null);

            //assert
            Assert.True(true);
        }

        [Fact]
        public void ParseArguments_CallWithValidArguments_Works()
        {
            //arrange s
            var actionFired = false;
            var param = new ConsoleInputParameter("x", "y", "z", (_) => actionFired = true);
            var manager = new ConsoleManager(new List<ConsoleInputParameter> { param });

            //act
            manager.ParseArguments(new[] { "x" });

            //assert
            Assert.True(actionFired);
        }

        [Fact]
        public void ParseArguments_CallWithInvalidArguments_Works()
        {
            //arrange s
            var actionFired = false;
            var param = new ConsoleInputParameter("x", "y", "z", (_) => actionFired = true);
            var manager = new ConsoleManager(new List<ConsoleInputParameter> { param });

            //act
            manager.ParseArguments(new[] { "y" });

            //assert
            Assert.False(actionFired);
        }
    }
}