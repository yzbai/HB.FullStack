using HB.FullStack.Mobile.Skia;

namespace HB.FullStack.Mobile.Controls.Clock
{
    public class TimeBlockFigureGroup : SKFigureGroup<TimeBlockFigure>
    {
        public TimeBlockFigure? SelectedTimeBlockFigure { get; internal set; }

        public void SetTapped(TimeBlockFigure timeBlockFigure)
        {
            SelectedTimeBlockFigure = timeBlockFigure;

            foreach (TimeBlockFigure figure in Figures)
            {
                if (figure != timeBlockFigure)
                {
                    figure.State = TimeBlockFigureState.Normal;
                }
            }
        }

        public void SetAllBackToNormal()
        {
            SelectedTimeBlockFigure = null;

            foreach (TimeBlockFigure figure in Figures)
            {
                figure.State = TimeBlockFigureState.Normal;
            }
        }

        public void SetLongTapped(TimeBlockFigure timeBlockFigure)
        {
            SelectedTimeBlockFigure = timeBlockFigure;

            foreach (TimeBlockFigure figure in Figures)
            {
                if (figure != timeBlockFigure)
                {
                    figure.State = TimeBlockFigureState.Normal;
                }
            }
        }
    }
}