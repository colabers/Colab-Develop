using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Colab.Develop
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class LogRichTextBox : RichTextBox
    {
        public LogRichTextBox() : base()
        {
            paragraph = new Paragraph();
            this.Document.Blocks.Add(paragraph);
        }

        private Paragraph paragraph;


        public void LogInfo(String info)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                paragraph.Inlines.Add(new Run(info));
            }));
        }

        public void LogError(String error)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                paragraph.Inlines.Add(new Run(error) { Foreground = Brushes.Red });
            }));
        }
    }
}
