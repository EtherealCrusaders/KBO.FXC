using KBOFXC;

namespace KBO.FXC.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            int result = Program.Main([]);
            Assert.That(result, Is.EqualTo(0));
        }
    }
}