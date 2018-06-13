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
    public class LowerDefender : Player
    {
        private const int WORKING_AREA = 30;
        private const int MOST_FORWARD_POSSITION = 5;
        private const int MOST_HEIGHT_DISTANCE = -15;


        private const int WAIT_FOR_MSG_TIME = 10;


        public LowerDefender(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 30, 20);
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
                        GetMyPlayerDetailsByCoach().Pos.Value.X > MOST_FORWARD_POSSITION ||
                        GetMyPlayerDetailsByCoach().Pos.Value.Y < MOST_HEIGHT_DISTANCE)
                    {
                        GoToOriginSynced();
                    }
                    else
                    {
                        RushBallSynced();
                        if (GetMyPlayerDetailsByCoach().Pos.Value.X > 0)
                        {
                            PassToPossition(GetMostForwardPlayerPossition());
                        }
                    }
                }
                catch (Exception e)
                { }
            }




            // sleep one step to ensure that we will not send
            // two commands in one cycle.
            //WaitSimulatorStep();
        }

        private void RushBallSynced()
        {
            while (true)
            {

                if (goToBallCoordinates(1, 1)) break;
                WaitSimulatorStep();
            }
        }

        private void GoToOriginSynced()
        {
            while (!goToCoordinate(m_startPosition, 1))
            {
                WaitSimulatorStep();
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
