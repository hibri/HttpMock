using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpMock;

namespace HttpMockConsole {
	class Program {
		static void Main(string[] args) {

			string url = "http://localhost:9191/endpoint";

			var httpMockRepository = HttpMockRepository.At(url);

			Console.Read();
		}
	}
}
