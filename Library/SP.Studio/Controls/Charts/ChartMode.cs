using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Controls.Charts
{
    /// <summary>
    /// 报表类型
    /// </summary>
    public enum ChartMode
    {
        Area2D, Bar2D,
        Bubble, Column2D,
        Column3D, Doughnut2D,
        Doughnut3D, FCExporter,
        Line, Marimekko,
        MSArea, MSBar2D,
        MSBar3D, MSColumn2D,
        MSColumn3D, MSColumn3DLineDY,
        MSColumnLine3D, MSCombi2D,
        MSCombi3D, MSCombiDY2D,
        MSLine, MSStackedColumn2D,
        MSStackedColumn2DLineDY, Pareto2D,
        Pareto3D, Pie2D,
        Pie3D, Scatter,
        ScrollArea2D, ScrollColumn2D,
        ScrollCombi2D, ScrollCombiDY2D,
        ScrollLine2D, ScrollStackedColumn2D,
        SSGrid, StackedArea2D,
        StackedBar2D, StackedBar3D,
        StackedColumn2D, StackedColumn2DLine,
        StackedColumn3D, StackedColumn3DLine,
        StackedColumn3DLineDY, ZoomLine
    }
}
