using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.DrawingCore;

namespace Leayal
{
    public static class TextRenderer
    {
        internal static readonly char[] SpaceOnly = { ' ' };

        public static WrapStringResult WrapString(Graphics gr, string originaltext, int preferedWidth, System.DrawingCore.Font _font)
        {
            if (originaltext.IndexOf("\n") > -1)
            {
                float width = 0, height = 0;
                StringBuilder sb = new StringBuilder();
                WrapStringResult re;
                bool first = true;
                using (StringReader sr = new StringReader(originaltext))
                    while (sr.Peek() > -1)
                    {
                        re = WrapString(gr, sr.ReadLine(), preferedWidth, _font);
                        if (first)
                        {
                            first = false;
                            sb.Append(re.Result);
                        }
                        else
                        {
                            sb.AppendLine();
                            sb.Append(re.Result);
                        }
                        width = Math.Max(width, re.Size.Width);
                        height = height + re.Size.Height;
                    }
                return new WrapStringResult(sb.ToString(), new SizeF(width, height));
            }
            else
            {
                List<string> _list = new List<string>();
                SizeF s = new SizeF(preferedWidth, _font.Height), ss;
                StringBuilder sb = new StringBuilder(originaltext.Length);
                bool first = true;
                string[] splitted = originaltext.Split(SpaceOnly);
                string str;
                float _height = 0F, _width = 0F;
                for (int i = 0; i < splitted.Length; i++)
                {
                    str = splitted[i];
                    if (first)
                    {
                        first = false;
                        sb.Append(str);
                    }
                    else
                        sb.AppendFormat(" {0}", str);
                    ss = gr.MeasureString(sb.ToString(), _font, s);
                    if (ss.Width >= preferedWidth)
                    {
                        _list.Add(sb.ToString());
                        _width = Math.Max(ss.Width, _width);
                        _height = _height + ss.Height;
                        sb.Clear();
                        first = true;
                    }
                }
                if (_list.Count == 0)
                {
                    s = gr.MeasureString(sb.ToString(), _font, s);
                    _width = s.Width;
                    _height = s.Height;
                    _list.Add(sb.ToString());
                }
                else
                {
                    str = sb.ToString();
                    if (!string.IsNullOrEmpty(str))
                    {
                        s = gr.MeasureString(sb.ToString(), _font, s);
                        _width = Math.Max(s.Width, _width);
                        _height = _height + s.Height;
                        _list.Add(str);
                    }
                }
                first = true;
                sb.Clear();
                foreach (string sstr in _list)
                {
                    if (first)
                    {
                        first = false;
                        sb.Append(sstr);
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.Append(sstr);
                    }
                }
                _list.Clear();
                return new WrapStringResult(sb.ToString(), new SizeF(_width, _height));
            }
        }
    }
}
