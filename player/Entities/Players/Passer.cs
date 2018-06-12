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
    public class Passer : Player
    {
        private const int WAIT_FOR_MSG_TIME = 10;

        public Passer(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 31, 0);
        }

        public override void play()
        {
            // first ,ove to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);

            while (!m_timeOver)
            {
                try
                {
                    var ball = m_coach.GetSeenCoachObject("ball");
                    if (ball != null)
                    {
                        goToCoordinate(ball.Pos.Value);

                    }

                }
                catch (Exception e)
                {

                }
                // sleep one step to ensure that we will not send
                // two commands in one cycle.
                try
                {
                    Thread.Sleep(SoccerParams.simulator_step);
                }
                catch (Exception e)
                {

                }
            }
        }

        private SenseBodyInfo GetBodyInfo()
        {
            m_robot.SenseBody();
            SenseBodyInfo bodyInfo = null;
            while (bodyInfo == null)
            {
                Thread.Sleep(WAIT_FOR_MSG_TIME);
                bodyInfo = m_memory.getBodyInfo();
            }

            return bodyInfo;
        }
    }
}
