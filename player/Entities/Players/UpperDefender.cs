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
    public class UpperDefender : Defender
    {
        public override bool OverY
        {
            get
            {
                return GetBallDetailsByCoach().Pos.Value.Y < MOST_HEIGHT_DISTANCE * -1;
            }
        }

        public UpperDefender(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 30, -20);
        }
    }
}
