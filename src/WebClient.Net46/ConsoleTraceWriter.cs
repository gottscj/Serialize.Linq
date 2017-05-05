using System;
using System.IO;
using System.Text;

namespace WebClient.Net46
{
    public class ConsoleTextWriter : TextWriter
    {
        private readonly StringBuilder _buffer;

        public ConsoleTextWriter()
        {
            _buffer = new StringBuilder();
        }

        public override void Write(char value)
        {
            switch (value)
            {
                case '\n':
                    return;
                case '\r':
                    Console.WriteLine(_buffer.ToString());
                    _buffer.Clear();
                    return;
                default:
                    _buffer.Append(value);
                    break;
            }
        }

        public override void Write(string value)
        {
            Console.WriteLine(value);

        }
        #region implemented abstract members of TextWriter
        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
