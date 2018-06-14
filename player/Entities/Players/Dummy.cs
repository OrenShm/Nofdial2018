using RoboCup.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using RoboCup.Infrastructure;

namespace RoboCup
{
    public class Dummy : Player
    {
        private const int WAIT_FOR_MSG_TIME = 10;

        public Dummy(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 10, 0);
        }

        public override void play()
        {

            // first ,ove to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);

            while (!m_timeOver)
            {
                try
                {
                    m_robot.Turn(90);
                    Console.WriteLine(GetMyPlayerDetailsByCoach().BodyAngle);
                }
                catch (Exception e)
                {

                }
                // sleep one step to ensure that we will not send
                // two commands in one cycle.
                try
                {
                    Thread.Sleep(10 * SoccerParams.simulator_step);
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
