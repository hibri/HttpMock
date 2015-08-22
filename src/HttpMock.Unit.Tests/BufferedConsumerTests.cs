using System;
using System.Text;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
    [TestFixture]
    public class BufferedConsumerTests
    {
        [Test]
        public void Should_return_data_in_order()
        {
            string expected = "1234";
            string data = "";
            
            var bufferedConsumer = new BufferedConsumer(s => { data = s; }, exception => { });

            bufferedConsumer.OnData(new ArraySegment<byte>(Encoding.UTF8.GetBytes("1")), () => { });
            bufferedConsumer.OnData(new ArraySegment<byte>(Encoding.UTF8.GetBytes("2")), () => { });
            bufferedConsumer.OnData(new ArraySegment<byte>(Encoding.UTF8.GetBytes("3")), () => { });
            bufferedConsumer.OnData(new ArraySegment<byte>(Encoding.UTF8.GetBytes("4")), () => { });
            
             
            bufferedConsumer.OnEnd();

            Assert.That(data, Is.EqualTo(expected));
        }
    }
}