using System.Text;

namespace Leayal
{
    class WeirdStringBuilder
    {
        public int Position { get; set; }
        private StringBuilder sb;
        public WeirdStringBuilder()
        {
            this.sb = new StringBuilder();
            this.Position = 0;
        }

        public override string ToString()
        {
            return this.sb.ToString();
        }

        public void Append(string str)
        {
            if (this.Position == this.sb.Length)
            {
                this.Position += str.Length;
                this.sb.Append(str);
            }
            else
            {
                this.Position += str.Length;
                this.sb.Insert(this.Position, str);
            }
        }

        public void Append(char c)
        {
            if (this.Position == this.sb.Length)
            {
                this.Position++;
                this.sb.Append(c);
            }
            else
            {
                this.Position++;
                this.sb.Insert(this.Position, c);
            }
        }

        public void AppendLine(string str)
        {
            if (this.Position == this.sb.Length)
            {
                this.Position += str.Length + 1;
                this.sb.AppendLine(str);
            }
            else
            {
                this.Position += str.Length + 1;
                this.sb.Insert(this.Position, str + "\n");
            }
        }

        public void Backspace()
        {
            if (this.Position > 0)
            {
                this.sb.Remove(this.Position - 1, 1);
                this.Position--;
            }
        }

        public void Delete()
        {
            if (this.Position >= this.sb.Length) return;
            if (this.Position < (this.sb.Length - 1))
            {
                this.sb.Remove(this.Position, 1);
            }
        }

        public void AppendFormat(string format, params string[] param)
        {
            if (this.Position == this.sb.Length)
            {
                for (int i = 0; i < param.Length; i++)
                    this.Position += param[i].Length;
                this.sb.AppendFormat(format, param);
            }
            else
            {
                for (int i = 0; i < param.Length; i++)
                    this.Position += param[i].Length;
                this.sb.Insert(this.Position, string.Format(format, param));
            }
        }

        public void GoLeft()
        {
            if (this.Position <= 0) return;
            this.Position--;
        }

        public void GoRight()
        {
            if (this.Position >= this.sb.Length) return;
            this.Position++;
        }
    }
}
