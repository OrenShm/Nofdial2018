using RoboCup.Entities;
using RoboCup.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace RoboCup
{
    public class OrenLowerAttacker : Forward
    {
        public override bool OverY
        {
            get
            {
                return GetBallDetailsByCoach().Pos.Value.Y < -1 * MOST_HEIGHT_DISTANCE;
            }
        }

        public OrenLowerAttacker(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * -10, 20);
        }
    }
}
