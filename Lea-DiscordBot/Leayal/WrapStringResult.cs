using System.Drawing;

namespace Leayal
{
    public struct WrapStringResult
    {
        public string Result { get; }
        public SizeF Size { get; }
        internal WrapStringResult(string re, SizeF _size)
        {
            this.Result = re;
            this.Size = _size;
        }
    }
}
