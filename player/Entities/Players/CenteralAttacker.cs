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
    public class CenteralAttacker : Player
    {
        private const int WORKING_AREA = 30;
        private const int MOST_BACK_POSSITION = -10;


        private const int WAIT_FOR_MSG_TIME = 10;


        public CenteralAttacker(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 5, 10);
        }

        public override void play()
        {
            // first ,over to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);
            //Go to start possition in case the Move failed.
            GoToOriginSynced();
            while (!m_timeOver)
            {
                try
                {
                    if (GetDistanceToBall() > WORKING_AREA ||
                        GetMyPlayerDetailsByCoach().Pos.Value.X < MOST_BACK_POSSITION)
                    {
                        //GoToOriginSynced();
                        goToCoordinate(m_startPosition, 1);
                        WaitSimulatorStep();
                    }
                    else
                    {
                        //RushBallSynced();
                        if (goToBallCoordinates(1.5, m_sideFactor * 3) == false)
                        {
                            WaitSimulatorStep();
                            continue;
                        }
                        //WaitSimulatorStep();
                        else
                        {
                            m_robot.Kick(100, GetAngleToOpponentGoal());
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in lower main loop: " + e.Message);
                }
            }




            // sleep one step to ensure that we will not send
            // two commands in one cycle.
            //WaitSimulatorStep();
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
