using Ace;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Solfeggio.Controls
{
    public partial class RangeBar
    {
        public RangeBar() => InitializeComponent();

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            this.EnumerateSelfAndVisualDescendants().OfType<Grid>().First().To(out Grid grid);
            Ace.Markup.Rack.GetColumns(grid).Split('*', '^').To(out var columns);
            var a = double.Parse(columns[0]);
            var b = double.Parse(columns[2]);
            var c = double.Parse(columns[4]);
            var f = (Maximum - Minimum) / (a + b + c);
            var delta = e.HorizontalChange / (ActualWidth * f);

            var l = a + delta;
            var r = c - delta;
            l = l < 0d ? 0d : l;
            r = r < 0d ? 0d : r;
            Ace.Markup.Rack.SetColumns(grid, $"{l}* ^ {b}* ^ {r}*");
        }
        private void Thumb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.EnumerateSelfAndVisualDescendants().OfType<Grid>().First().To(out Grid grid);
            Ace.Markup.Rack.GetColumns(grid).Split('*', '^').To(out var columns);
            var a = double.Parse(columns[0]);
            var b = double.Parse(columns[2]);
            var c = double.Parse(columns[4]);
            var f = (Maximum - Minimum) / (a + b + c);
            SelectionStart = Minimum + a * f;
            SelectionEnd = Maximum - c * f;
        }
    }
}
